using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


// Author: ZiQi Li, COMP521
public class GameFlowManager : MonoBehaviour
{
    //--------
    // public variables
    public GameObject agentPrefab = null;
    public GameObject goalPrefab = null;
    public GameObject highlightLinePrefab = null;
    public TMP_Text infoText = null;
    public int numOfInitAgents = 2;
    public float agentSpeed = 3f;  // the moving speed of agents
    // floors list
    public GameObject[] floors = new GameObject[12];
    // scripts instance
    public InitializationManager initManager;  // assign the instance of InitializationManager script for using its methods
    public static GameFlowManager gameFlowManager;  // used by other classes (Agents) to access GameFlowManager instance 

    //--------
    // private variables

    // agent spawning
    private ArrayList floorCorners;  // list that stores the corners (as array {minx, maxx, minz, maxz}) of each piece of floor
    private ArrayList agentsList = new ArrayList();  // list that stores all spawned agents in the map
    static private float agentRadius = 0;
    private ArrayList floorWeights;
    private Graph reducedVisibilityGraph = null;

    // Experiment variable
    private float timer = 0;  // timer using to double the number of agent every 30s
    private int numberOfAgents = 0;  // current number of agents




    // Start is called before the first frame update
    void Start()
    {
        // initialize //
        gameFlowManager = this;  // store the instance of this class in a public static field

        reducedVisibilityGraph = initManager.getReducedVisibilityGraph();
        agentRadius = agentPrefab.GetComponent<SphereCollider>().radius * agentPrefab.transform.lossyScale.x;  // radius in world space = local radius * scale
        // get the list that containing floors' 4corners as array
        floorCorners = generateFloorsList(floors);
        // run the initialize function for generateRandPointOnMap
        floorWeights = getFloorsWeights(floorCorners);

        //agentBody = testAgennt.GetComponent<Rigidbody>();

        // generate numOfInitAgents agents initially
        for (int i = 0; i < numOfInitAgents; i++)
        {
            spawnAgent(floorCorners, floorWeights, agentsList, agentPrefab, goalPrefab, reducedVisibilityGraph, agentSpeed);
        }

        numberOfAgents = numOfInitAgents;

    }



    // Physic Update is called once per frame
    void FixedUpdate()
    {
        // update info
        updateInfo(agentsList);

        timer += Time.deltaTime; // update the timer
        /*
        // double the number of agent every 30s
        if(timer >= 10)
        {
            timer = 0;
            // double the number of agents
            int numOfAgents = agentsList.Count;
            for (int i = 0; i < numOfAgents; i++)
            {
                //spawnAgent(floorCorners, floorWeights, agentsList, agentPrefab, goalPrefab);
            }
            numberOfAgents = agentsList.Count;

        }*/


        // RULE for agents' path finding:
        // Once they reach their destination they should wait 200-1000ms, and then choose a new destination.
        // Use a replanning strategy to help agents eventually get to their destination. An agent identiﬁed to
        // have been blocked should discard its current path plan, wait 100-500ms, and then compute and start
        // following a new plan to get to its destination. After 3 failed attempts to reach a given destination
        // an agent gives up and chooses a new random destination.

        float randTime = 0;
        Vector3 dest;
        GameObject goalObject;
        // update each agent in the agents list
        foreach (Agent ag in agentsList)
        {
            // make agent follow the path, if agent reach destination (return is false)
            if(!ag.followPath())
            {
                // if the agent is already in waiting state
                if(ag.isWaiting == true)
                {
                    // decrease the count down timer, if return value is true, means count down is finish
                    if(ag.checkTimer(Time.deltaTime))
                    {
                        // if this agent is also in need replanning state
                        if(ag.needReplanning)
                        {
                            // if fail attempt = 3, we will find a new rand goal
                            if (ag.failsAttemps >= 3)
                            {
                                ag.failsAttemps = 0;  // reset the fail attemps variable

                                // find a new goal for this agent
                                // generate the random destination for this agent
                                dest = generateRandPointOnMap(floorCorners, floorWeights);
                                dest.y = 0.11f;
                                goalObject = Instantiate(goalPrefab, dest, Quaternion.Euler(0, 0, 0));
                                MeshRenderer mr = goalObject.GetComponent<MeshRenderer>();
                                mr.material.SetColor("_Color", ag.getColor());
                                ag.setGoalGameObject(goalObject);  // set the goal object

                                dest.y = 0.1f;
                                // path find to the new destination
                                ag.pathFinding(dest);

                                // set waiting state to false
                                ag.isWaiting = false;
                                // set need replanning state to false
                                ag.needReplanning = false;
                            }
                            else  // otherwise we will replan to the current destination
                            {
                                ag.pathFinding(ag.previousDest);

                                // set waiting state to false
                                ag.isWaiting = false;
                                // set need replanning state to false
                                ag.needReplanning = false;
                            }
                        }
                        else  // in simple waiting state
                        {
                            // find a new goal for this agent
                            // generate the random destination for this agent
                            dest = generateRandPointOnMap(floorCorners, floorWeights);
                            dest.y = 0.11f;
                            goalObject = Instantiate(goalPrefab, dest, Quaternion.Euler(0, 0, 0));
                            MeshRenderer mr = goalObject.GetComponent<MeshRenderer>();
                            mr.material.SetColor("_Color", ag.getColor());
                            ag.setGoalGameObject(goalObject);  // set the goal object

                            dest.y = 0.1f;
                            // path find to the new destination
                            ag.pathFinding(dest);

                            // set waiting state to false
                            ag.isWaiting = false;
                        }

                    }
                }
                else if(ag.needReplanning) // if this agent needs replanning since it's blocked by another agent
                {
                    // set this agent to waiting state
                    ag.isWaiting = true;

                    // if fail attempt = 3, we have to remove the goal, since we will find a new rand goal
                    if(ag.failsAttemps >= 3)
                    {
                        Destroy(ag.getGoalGameObject(), 0.5f);
                    }

                    // remove the current path
                    ag.getPath().RemoveRange(0, ag.getPath().Count);

                    // set this agent's count down timer to a random number between 100-500ms -> 0.1~0.5s
                    randTime = UnityEngine.Random.Range(0.1f, 0.5f);
                    ag.setTimer(randTime);
                    ag.checkTimer(Time.deltaTime);
                   
                }
                else  // if just reaching the destination (set the count down timer for its new plan)
                {
                    ag.successReached++;  // update the successful reach counter

                    // set waiting state to true
                    ag.isWaiting = true; 

                    // destroy the goal game object after 0.5s
                    Destroy(ag.getGoalGameObject(), 0.5f);

                    // set this agent's count down timer to a random number between 0.2s~1s
                    randTime = UnityEngine.Random.Range(0.2f, 1f);
                    ag.setTimer(randTime);
                    ag.checkTimer(Time.deltaTime);
                }

            }

            ag.updateHighLightLineSeg(); // update the highlight of current line segment that agent is following 
        }

    }


    //button action listenner function for PlayAgainButton
    void spawnButtonOnClick()
    {
        //reload the whole scene when pressing the playAgainButton
        //SceneManager.LoadScene("MainGame");
    }


    /* generateFloorsList */
    // function to generate a list containing the corners as array for every piece of floor
    ArrayList generateFloorsList(GameObject[] floorObjs)
    {
        ArrayList floorCorners = new ArrayList();
        // retrieve corners' coord for each piece of floors and put them in an List
        foreach(GameObject floor in floorObjs)
        {
            float[] coords = new float[4];  // {minx, maxx, minz, maxz}
            // get the corners of this floor and put the min/max of x/z axis of this floor into the list
            // we can use only 2 diagonal corners to determine the min/max of x/z axis
            coords[0] = floor.transform.GetChild(0).transform.position.x;
            coords[1] = floor.transform.GetChild(2).transform.position.x;
            coords[2] = floor.transform.GetChild(2).transform.position.z;
            coords[3] = floor.transform.GetChild(0).transform.position.z;
            floorCorners.Add(coords);
        }
        return floorCorners;
    }

    /* getFloorsWeights */
    // helper function for generateRandPointOnMap, calculating probability weight for each floor based on their area size
    ArrayList getFloorsWeights(ArrayList floorsList)
    {
        float totalArea = 0;
        // calculate the total area of floors
        foreach (float[] corners in floorsList)
        {
            // corners array: {minx, maxx, minz, maxz}
            totalArea += (corners[1] - corners[0]) * (corners[3] - corners[2]);
        }

        // calculate the weight for each floor (area/total area, add up to 1)
        ArrayList weights = new ArrayList();
        foreach (float[] corners in floorsList)
        {
            weights.Add(((corners[1] - corners[0]) * (corners[3] - corners[2])) / totalArea);
        }
        return weights;
    }


    /* generateRandPointOnMap */
    // Input: - a list of floor corners stored as array
    //        - a list of weights for each floor
    // function to randomly generate a point on the map
    // The idea of this function is take each piece of floor and assign them a weight based on they area,
    // so that small area floors won't have the same probability of having an agent spawning on them as large area floors
    Vector3 generateRandPointOnMap(ArrayList floorsList, ArrayList floorWeights)
    {
        float[] pickedFloor = new float[4];  // store {minx, maxx, minz, maxz} of the randomly picked floor
        float randNum = UnityEngine.Random.Range(0f, 1f);  // generate a rand number in [0, 1]
        float probability = 0; // an local variable to add up the probability as iterating
        //Debug.Log(randNum);
        for (int i = 0; i < floorsList.Count; i++)
        {
            probability += (float)floorWeights[i];
            // each floor will occupy a subrange in [0, 1] according to their weight
            // if the ranNum is in their range, this floor is chosen
            if (randNum <= probability)
            {
                // copy the array into the pickedFloor array
                for(int j = 0; j < pickedFloor.Length; j++)
                {
                    pickedFloor[j] = (floorsList[i] as float[])[j];
                }
                break;
            }
        }
        
        // Now reduced the picked floor range a little bit to avoid walls around it
        pickedFloor[0] += 0.3f; // minx
        pickedFloor[1] -= 0.3f; // maxx
        pickedFloor[2] += 0.3f; // minz
        pickedFloor[3] -= 0.3f; // maxz

        // generate a point inside the picked floor (test collision using SphereCast (ray cast with radius in 3D space)
        // the origin of the ray is at 3m above the ground level, the direction is toward the ground and the radius is a little bit larger than the radius of an agent
        Vector3 rayOrigin = new Vector3(UnityEngine.Random.Range(pickedFloor[0], pickedFloor[1]), 3, UnityEngine.Random.Range(pickedFloor[2], pickedFloor[3]));
        Vector3 direction = Vector3.down;
        float radius = agentRadius + 0.05f;
        float maxDistance = 3f - 2 * radius;
        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = rayOrigin;
        ray.direction = direction;
        // keep picking new rand point inside the picked floor if the previous rand point will cause collision with other colliders
        while (Physics.SphereCast(rayOrigin, radius, direction, out hit, maxDistance))
        {
            rayOrigin = new Vector3(UnityEngine.Random.Range(pickedFloor[0], pickedFloor[1]), 3, UnityEngine.Random.Range(pickedFloor[2], pickedFloor[3]));

            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 20f);
            //Debug.Log(hit.transform.name);

        }

        // return the rand point inside the floor without collision
        return new Vector3(rayOrigin.x, 0.1f, rayOrigin.z);
    }



    /* spawnAgent */
    // Input: -a list of floor corners stored as array
    //        -the current agents list in the map
    // function to randomly spwan agent inside the map without colliding with other agents or obstacle
    // And also make agents doing path finding to a random destination when spawning
    void spawnAgent(ArrayList floorCorners, ArrayList floorWeights, ArrayList agentList, GameObject agentPrefab, GameObject goalPrefab, Graph rvgraph, float agentSpeed)
    {
        // generate a random point on the map for generation of agent
        Vector3 spawnPoint = generateRandPointOnMap(floorCorners, floorWeights);
        
        // Generate a new agent
        Agent newAgent = new Agent(rvgraph, agentSpeed);
        newAgent.setGameObject(Instantiate(agentPrefab, spawnPoint, Quaternion.Euler(0, 0, 0)));  // instantiate this agent on the map
        newAgent.setRandColor();  // change the color of this agent to a rand color

        // generate the random destination for this agent
        Vector3 dest = generateRandPointOnMap(floorCorners, floorWeights);
        dest.y = 0.11f;
        GameObject goalObject = Instantiate(goalPrefab, dest, Quaternion.Euler(0, 0, 0));
        MeshRenderer mr = goalObject.GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color", newAgent.getColor());

        newAgent.setGoalGameObject(goalObject);  // set the current destination object (a circle) of new agent

        newAgent.pathFinding(dest);  // path find to the destination

        agentList.Add(newAgent);  // add this agent to the agent list
        /*
        ArrayList pathNodes = newAgent.getPath();
        foreach(object node in pathNodes)
        {
            Instantiate(goalPrefab, ((Graph.Node)node).getPosition(), Quaternion.Euler(0, 0, 0));
        }*/

    }

    // function to highlight the line segment that agent is currently taking
    public GameObject highLightPath(Vector3 end1, Vector3 end2)
    {
        // increase the height of line a little bit
        end1.y = 0.15f;
        end2.y = 0.15f;

        // generate an object with lineRender
        Vector3[] verticesPosition = new Vector3[2];
        GameObject line = Instantiate(highlightLinePrefab, this.transform.position, this.transform.rotation);
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        verticesPosition[0] = end1;
        verticesPosition[1] = end2;

        lineRenderer.SetPositions(verticesPosition);  // draw a line

        return lineRenderer.gameObject;
    }

    // function to update the experimental data
    void updateInfo(ArrayList agentsList)
    {
        int numPathPlanned = 0;  // int to store the # of paths planned
        int numReplanning = 0;  // int to store the # of replanning
        int successReached = 0;  // int to store the # of plans successfully reached
        float totalPlanningTime = 0;
        foreach (Agent ag in agentsList)
        {
            numPathPlanned += ag.numPathPlanned;
            numReplanning += ag.numReplanning;
            successReached += ag.successReached;
            totalPlanningTime += ag.totalPlanningTime;
        }

        totalPlanningTime *= 1000;  // convert second to ms (1s = 1000ms)

        infoText.SetText("Total simulation time: " + Time.realtimeSinceStartup + "s\n" +
                            "Current number of agents: " + numberOfAgents + "\n" +
                            "Number of paths planned: " + numPathPlanned + "\n" +
                            "Number of replannings: " + numReplanning + "\n" +
                            "Number of plans successfully reached: " + successReached + "\n" +
                            "Total planning time: " + totalPlanningTime + "ms\n" +
                            "COMP521 A3 by ZiQi Li");
    }
}
