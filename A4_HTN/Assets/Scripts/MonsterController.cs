using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // used for NavMesh path finding
using TMPro;


// Author: ZiQi Li
public class MonsterController : MonoBehaviour
{
    // Public variables
    public float wanderRadius = 0f;
    public float throwAngle = 0f;
    public float attackRange = 0f;
    public float yellingRange = 0f;
    public GameInitializer gameInitalizer;
    public PlayerContoller player;
    public TMP_Text monsterPlanText;
    public TMP_Text monsterYellingText;


    // --------------------------

    // Private variables
    private NavMeshAgent navMeshAgent;  // store the NavMeshAgent component of this monster
    private Vector3 currentDestination = Vector3.zero;  // stores the current destination of gotoObstacle function and wander function (used for task completness checking)
    private GameObject obstacleToBeThrown;  // store the current obstacle to be thrown by the monster
    private float yellingTimer = 0f;
    private float throwingTimer = 0f;

    // HTN
    private HTN_node rootNode;
    private HTN_Planner planner;
    private HTN_WorldState worldState;


    // --------------------------

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = this.gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.autoBraking = false;  // disable auto braking for continuous movement

        this.monsterYellingText.gameObject.SetActive(false);  // disable UI yelling text

        // generate an HTN tree for this monster
        rootNode = generateHTNtree();

        // generate a World State
        HTN_WorldState ws = new HTN_WorldState(false, false, false, false, false);  // set all false initially, let the update function to update WorldState
        this.worldState = ws;

        // create a HTN planner
        planner = new HTN_Planner(this, worldState);
    }

    // Update is called once per frame
    void Update()
    {
        
        
        
        
    }

    void FixedUpdate()
    {
        /*
        timer += Time.deltaTime;
        //wander();
        //gotoObstacle(gameInitalizer.getObstacleList());
        if(timer >=3)
        {
            throwObstacle();
            timer = -200;
        }*/

        updateWorldState(worldState);  // update world state

        this.planner.executePlan(Time.deltaTime);  // execute HTN plan

        updatePlanText();  // update Plan UI text


        // yell text appears for 3 second
        if (this.monsterYellingText.gameObject.activeSelf)
        {
            // yell for 3 second
            if (yellingTimer <= 3f)
            { 
                yellingTimer += Time.deltaTime;
            }
            else
            {
                yellingTimer = 0;
                this.monsterYellingText.gameObject.SetActive(false);
            }      
        }


            //Debug.Log(yellingTimer);

    }


    /// <summary>
    /// Function for monster to yell!
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns>Return True if finish yelling, otherwise false</returns>
    public bool yelling()
    {
        // need 1 second for complete yelling
        if (yellingTimer <= 1f)
        {
            yellingTimer += Time.deltaTime;
            return false;
        }
        else
        {
            this.monsterYellingText.gameObject.SetActive(true);
            yellingTimer = 0;
            return true;
        }

    }


    /// <summary>
    /// Funtion to make the monster randomly wandering around
    /// </summary>
    /// <returns>True if the monster is arrived the destination, false if on the way to the destination</returns>
    public bool wander()
    {

        // if the previous action is finished, proceed wander action
        //if(this.isActionFinished == true)
        //{

        // if no destination yet
        if (this.currentDestination == Vector3.zero)
        {
            Vector3 randPosition = Random.insideUnitSphere * this.wanderRadius;  // get a rand point inside a sphere

            randPosition += this.transform.position;  // apply this rand position around the position of this monster(as center)
            NavMeshHit navHit;
            NavMesh.SamplePosition(randPosition, out navHit, this.wanderRadius, -1);  // find a closest point on navMesh map

            navMeshAgent.destination = navHit.position;  // the final position is the navHit position

            this.currentDestination = navMeshAgent.destination;  // set the current destination

            return false;  // return false since on the way to destination
        }
        else
        {
            // if reach the destination
            if (Vector3.Distance(this.transform.position, this.navMeshAgent.destination) <= 2f)
            {
                //this.isActionFinished = true;
                this.currentDestination = Vector3.zero;  // remove the destination position
                return true;  // return true since the monster arrives the destination (wander task complete)
            }
            else
            {
                return false;  // return false since on the way to destination
            }
        }

    }

    /// <summary>
    /// Function to navigate to the closest obstacle on the map
    /// </summary>
    /// <param name="obstacleList"></param>
    /// <param name="taskName"></param>
    /// <returns>True if the monster is arrived the destination, false if on the way to the destination</returns>
    public bool gotoObstacle(ArrayList obstacleList, Task taskName)
    {

        // if no destination yet
        if (this.currentDestination == Vector3.zero)
        {
            GameObject closestObs = null;
            float closestDist = Mathf.Infinity;

            // find the closest obstacle to the monster
            foreach (GameObject obstacle in obstacleList)
            {
                float dist = Vector3.Distance(this.gameObject.transform.position, obstacle.transform.position);

                // if the task is throw_crate, we choose the nearest crate to go, otherwise choose the nearest rock to go
                if (taskName == Task.MOVE_TO_CRATE)
                {
                    if (dist < closestDist && obstacle.tag == "Crate")
                    {
                        closestDist = dist;
                        closestObs = obstacle;
                    }
                }
                else
                {
                    if (dist < closestDist && obstacle.tag == "Rock")
                    {
                        closestDist = dist;
                        closestObs = obstacle;
                    }
                }


            }


            // set the navMesh destination to the closest obstacle
            NavMeshHit navHit;
            NavMesh.SamplePosition(closestObs.transform.position, out navHit, closestDist, -1);  // find a closest point on navMesh map to the obstacle position

            navMeshAgent.destination = navHit.position;  // the final position is the navHit position
            this.currentDestination = navMeshAgent.destination;  // set the current destination

            this.obstacleToBeThrown = closestObs;  // update the obstacle to be thrown

            return false;  // return false since on the way to destination
        }
        else
        {
            // if reach the destination
            if (Vector3.Distance(this.transform.position, this.navMeshAgent.destination) <= 4.5f)
            {
                //navMeshAgent.destination = this.gameObject.transform.position;  // stop the path finding
                //this.isActionFinished = true;
                this.currentDestination = Vector3.zero;  // remove the destination position
                return true;  // return true since the monster arrives the destination (wander task complete)
            }
            else
            {
                return false;
            }
        }
        

    }

    /// <summary>
    /// Function to throw the current to be thrown obstacle to the player
    /// (Note: the calculation reference: https://forum.unity.com/threads/how-to-calculate-force-needed-to-jump-towards-target-point.372288/
    /// Since the physics calculation is not the grading part of this assignment, I have used the above link as reference)
    ///
    /// Return true if finish throwing
    /// </summary>
    public bool throwObstacle()
    {
        // need 1 second for complete throwing
        if (throwingTimer <= 1f)
        {
            throwingTimer += Time.deltaTime;
            return false;
        }
        else
        {
            if (this.obstacleToBeThrown.gameObject != null)
            {
                // take the obstacle over head
                //Vector3 monsterPos = this.gameObject.transform.position;
                Vector3 monsterPos = this.obstacleToBeThrown.gameObject.transform.position;
                monsterPos.y += 1f;
                this.obstacleToBeThrown.transform.position = monsterPos;


                Rigidbody obsRB = this.obstacleToBeThrown.GetComponent<Rigidbody>();
                Vector3 targetPos = this.player.gameObject.transform.position;  // get the player postion (out target postion of throwing)


                float throwingAngle = this.throwAngle * Mathf.Deg2Rad;  // convert the throwing angle to rad
                float gravity = Physics.gravity.magnitude;

                // Positions of this object and the target on the same plane
                Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
                Vector3 planarObstaclePostion = new Vector3(obsRB.transform.position.x, 0, obsRB.transform.position.z);

                // Planar distance between objects
                float distance = Vector3.Distance(planarTarget, planarObstaclePostion);
                // Distance along the y axis between objects
                float yOffset = obsRB.transform.position.y - targetPos.y;

                float initialVelocity = (1 / Mathf.Cos(throwingAngle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(throwingAngle) + yOffset));

                Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(throwingAngle), initialVelocity * Mathf.Cos(throwingAngle));

                // Rotate our velocity to match the direction between the two objects
                //float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarMonsterPostion);
                float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarObstaclePostion) * (targetPos.x > obsRB.transform.position.x ? 1 : -1);
                Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

                // Fire!
                obsRB.velocity = finalVelocity;


                // Alternative way of fire:
                //obsRB.AddForce(finalVelocity * obsRB.mass, ForceMode.Impulse);

                //if(this.obstacleToBeThrown.tag == "crate")

                this.obstacleToBeThrown = null;
            }
            throwingTimer = 0;
            return true;
        }
       



    }


    /// <summary>
    /// Function to generate a HTN tree for this monster
    /// </summary>
    /// <returns></returns>
    HTN_node generateHTNtree()
    {
        // the image of this tree is shown in PDF file

        // root node
        HTN_CompoundNode root = new HTN_CompoundNode();

        // create children methods
        HTN_MethodNode method1 = new HTN_MethodNode(Task.DISTANCE_ATTACK);
        HTN_MethodNode method2 = new HTN_MethodNode(Task.IDLE);

        root.addChild(method1);
        root.addChild(method2);

        // create children tasks for method1
        //HTN_CompoundNode crate_attack = new HTN_CompoundNode();
        //HTN_CompoundNode rock_attack = new HTN_CompoundNode();
        HTN_CompoundNode distance_attack = new HTN_CompoundNode();

        HTN_MethodNode crate_attackM = new HTN_MethodNode(Task.CRATE_ATTACK);
        HTN_MethodNode rock_attackM = new HTN_MethodNode(Task.ROCK_ATTACK);

        crate_attackM.addChild(new HTN_PrimitiveNode(Task.MOVE_TO_CRATE));
        crate_attackM.addChild(new HTN_PrimitiveNode(Task.THROW_CRATE));
        rock_attackM.addChild(new HTN_PrimitiveNode(Task.MOVE_TO_ROCK));
        rock_attackM.addChild(new HTN_PrimitiveNode(Task.THROW_ROCK));

        //method1.addChild(crate_attack);
        //method1.addChild(rock_attack);
        method1.addChild(distance_attack);


        //crate_attack.addChild(crate_attackM);
        //rock_attack.addChild(rock_attackM);
        distance_attack.addChild(crate_attackM);
        distance_attack.addChild(rock_attackM);


        // add child task for method2
        HTN_CompoundNode idle = new HTN_CompoundNode();

        HTN_MethodNode yelling_wanderM = new HTN_MethodNode(Task.YELLING_WANDER);
        HTN_MethodNode wanderM = new HTN_MethodNode(Task.ONLY_WANDER);

        yelling_wanderM.addChild(new HTN_PrimitiveNode(Task.YELLING));
        yelling_wanderM.addChild(new HTN_PrimitiveNode(Task.WANDER));
        wanderM.addChild(new HTN_PrimitiveNode(Task.WANDER));

        method2.addChild(idle);
        idle.addChild(yelling_wanderM);
        idle.addChild(wanderM);


        //method2.addChild(new HTN_PrimitiveNode(Task.WANDER));



        return root;
    }

    /// <summary>
    /// Function to update the world state for this monster's HTN
    /// </summary>
    /// <param name="ws"></param>
    void updateWorldState(HTN_WorldState ws)
    {
        // update isInAttackRange
        if (Vector3.Distance(this.gameObject.transform.position, player.gameObject.transform.position) <= attackRange)
        {
            ws.isInAttackRange = true;
        }
        else
        {
            ws.isInAttackRange = false;
        }

        // update isInAttackRange
        if (Vector3.Distance(this.gameObject.transform.position, player.gameObject.transform.position) <= yellingRange)
        {
            ws.isInYellingRange = true;
        }
        else
        {
            ws.isInYellingRange = false;
        }


        // update hasCratesInRange, check if there is crate in attack range 
        bool hasCrates = false;
        foreach(GameObject obstacle in gameInitalizer.getObstacleList())
        {
            if (obstacle.tag == "Crate" && Vector3.Distance(this.gameObject.transform.position, obstacle.transform.position) <= attackRange)
            {
                hasCrates = true;
                break;
            }
        }
        if (hasCrates)
        {
            ws.hasCratesInRange = true;
        }
        else
        {
            ws.hasCratesInRange = false;
        }


        // update isInFrontOfObstacle
        if(this.obstacleToBeThrown)
        {
            if(Vector3.Distance(this.gameObject.transform.position, this.obstacleToBeThrown.transform.position) <= 6f)
            {
                ws.isInFrontOfObstacle = true;
            }
            else
            {
                ws.isInFrontOfObstacle = false;
            }
        }

        // update isTheChosenCrateDestroyed
        if(this.obstacleToBeThrown)
        {
            // if the current chosen obstacle is destroyed, change the world state isTheChosenCrateDestroyed to true
            if (this.obstacleToBeThrown.gameObject == null)
            {
                ws.isTheChosenObstacleDestroyed = true;
                //this.obstacleToBeThrown = null;
            }
            else
            {
                ws.isTheChosenObstacleDestroyed = false;
            }
        }

        // updata the world state in planner
        this.planner.updateWorldState(this.worldState);
    }

    /// <summary>
    /// Function to update the current plan on UI
    /// </summary>
    void updatePlanText()
    {
        string title = "Current Monster plan: [";
        ArrayList planName = this.planner.getPlanInString();

        for (int i = 0; i < planName.Count; i++)
        {
            title += planName[i];
            if(i < planName.Count-1)
                title += " -> ";
        }
        //title += "DONE]";
        title += "]";

        this.monsterPlanText.SetText(title);
    }


    public HTN_node getRootNode()
    {
        return this.rootNode;
    }



}
