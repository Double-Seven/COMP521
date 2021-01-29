using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// Author: ZiQi Li, COMP521
public class InitializationManager : MonoBehaviour
{
    //--------
    // public variables
    public GameObject obstaclePrefab = null;
    public GameObject linePrefab = null;
    public GameObject vertexPrefab = null;
    public GameObject[] mapBoundaryPoints;


    public static InitializationManager initManager;  // instance of this class, will be used by other classes

    //--------
    // private variables

    // obstacle generation
    private static float verticalLevel = 0.5f;
    private static Vector2 obstacleScaleRange = new Vector2(0.8f, 1.1f);  // gives the (min,max) of obstacle random scaling range (on x and z)
    private static float[] obstacleRotateRange = {0f, 90f, 180f, 270f};  // gives the possible rotation direction of obstacles
    private static float[] obstaclePositionRange1 = {-7f, -4f, 2.5f, 3.3f};  // random position range: {xmin, xmax, zmin, zmax} 
    private static float[] obstaclePositionRange2 = { -5.5f, -2f, -2.5f, -1.2f };  // random position range: {xmin, xmax, zmin, zmax}
    private static float[] obstaclePositionRange3 = { 0f, 2.4f, 2.05f, 2.25f };  // random position range: {xmin, xmax, zmin, zmax}
    private static float[] obstaclePositionRange4 = { 2.8f, 5.5f, -2.7f, -2.6f };  // random position range: {xmin, xmax, zmin, zmax}
    private ArrayList obstacleGameObjects = new ArrayList();
    private float shrinkObstacleScale = -0.2f;  // scale after we shrink the obstacle to prevent agent colliding with obstacles when moving along a path which is around an obstacle

    // Reduced visibility graph
    private Graph reducedVisibilityGraph = null;  // store the reduced visibility graph of map containing obstacles



    // Awake is called before all classes
    void Awake()
    {
        initManager = this;

        obstacleGameObjects = generateObstacles(obstaclePrefab);  // randomly generate obstacles

        reducedVisibilityGraph = generateRVgraph(mapBoundaryPoints, obstacleGameObjects); // generate Reduced Visibility graph based on the obstacles and map boundary
   
        drawReducedVisibilityGraph(reducedVisibilityGraph);  // draw the Reduced Visibility graph

        placeVerticesObjects(reducedVisibilityGraph, vertexPrefab);  // place vertices game object

        changeObstaclesSize(obstacleGameObjects, new Vector3(shrinkObstacleScale, 0, shrinkObstacleScale-0.1f));  // decrease the obstacle size, prevent agent colliding with obstacles when moving along a path which is around an obstacle
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /* generateObstacles */
    // function to generate 3~4 obstacles with random L shape and random position on the map
    ArrayList generateObstacles(GameObject obstacle)
    {
        int numObstacles = Random.Range(0, 1.0f) >= 0.5f ? 3 : 4;  // gives either 3 or 4 randomly
        //print(numObstacles);

        ArrayList positions = new ArrayList();
        positions.Add(obstaclePositionRange1);
        positions.Add(obstaclePositionRange2);
        positions.Add(obstaclePositionRange3);
        positions.Add(obstaclePositionRange4);

        ArrayList obstacleList = new ArrayList();
        float[] pos = null;
        int randIndex = 0;
        float xPos = 0;
        float zPos = 0;
        Quaternion rotation;
        while (numObstacles > 0)
        {
            numObstacles--;
            // random position in given range
            randIndex = (int)Random.Range(0, positions.Count - 0.001f);  // generate a rand index
            pos = (float[]) positions[randIndex];
            positions.RemoveAt(randIndex);

            // random rotation and position
            xPos = Random.Range(pos[0], pos[1]);
            zPos = Random.Range(pos[2], pos[3]);
            rotation = Quaternion.Euler(0, obstacleRotateRange[(int)Random.Range(0, obstacleRotateRange.Length-0.001f)], 0);
            GameObject obs = Instantiate(obstacle, new Vector3(xPos, verticalLevel, zPos), rotation);

            // random scaling (size)  ERROR: collider won't be detected properly by Rarcast after rescaleing the obstacles
            // Fix Error:
            // Modifying a Transform doesn't immediately change physics components, so we have to call SyncTransforms to manually synchronize it 
            // i.e. Physics.SyncTransforms();
            obs.transform.localScale = new Vector3(Random.Range(obstacleScaleRange[0], obstacleScaleRange[1]), 1, Random.Range(obstacleScaleRange[0], obstacleScaleRange[1]));

            // add this generated obstacle to obstacle list
            obstacleList.Add(obs);
        }
        Physics.SyncTransforms();  // synchronize the rescaling we made for obstacles

        return obstacleList;
    }



    /// <summary>
    /// Function to change the obstacles' scale in the obstacleList (obstacle scale will += parameter scale)
    /// </summary>
    /// <param name="obstaclesList"></param>
    /// <param name="scale"></param>
    public void changeObstaclesSize(ArrayList obstaclesList, Vector3 scale)
    {
        foreach(GameObject obstacle in obstaclesList)
        {
            obstacle.transform.localScale += scale;
        }
    }

    /// <summary>
    /// Generate the reduced visibility graph of this map using Signed area of a triangle test (SAT)
    /// </summary>
    /// <param name="terrainCorners"></param>
    /// <param name="obstacles"></param>
    /// return the RVGraph
    Graph generateRVgraph(GameObject[] terrainCorners, ArrayList obstacles)
    {
        Graph rvgraph = new Graph();
        // To construct a reduced visibility graph:
        // a) if 2 reflex vertices are end points of the same edge -> join them with an edge
        // b) if 2 reflex vertices can be joined by a “bitangent line”, add an edge between them.

        ArrayList terrainVertices = new ArrayList();  // list stores terrain vertices as Node
        // Add terrain vertices into a list as node
        foreach (GameObject obj in terrainCorners)
        {
            terrainVertices.Add(new Graph.Node(obj.transform.position, obj));
        }


        // ------------------------
        // connect terrain reflex vertices to terrain reflex vertices if "bitangent", checking use SAT test
        // SAT test for 2D: say we have points P0, P1, P1
        // (p1x-p0x)(p2y-p0y)-(p2x-p0x)(p1y-p0y) > 0: p2 is “left” of p0->p1
        // (p1x-p0x)(p2y-p0y)-(p2x-p0x)(p1y-p0y) < 0: p2 is “right” of p0->p1
        // (p1x-p0x)(p2y-p0y)-(p2x-p0x)(p1y-p0y) = 0: p2 is colinear with of p0->p1
        Vector2 point1;
        Vector2 point2;
        Vector2 point1Prev;  // previous vertex of point1
        Vector2 point2Prev;  // previous vertex of point2
        Vector2 point1Next;  // next vertex of point1
        Vector2 point2Next;  // next vertex of point2
        Graph.Node node1;
        Graph.Node node2;
        Vector3 direction;  // used for raycast
        for (int i = 0; i < terrainVertices.Count; i++)
        {
            node1 = (Graph.Node)terrainVertices[i];

            for (int k = 0; k < terrainVertices.Count; k++)
            {
                node2 = (Graph.Node)terrainVertices[k];

                // skip if vertext1 and vertex2 are the same
                if (node1.Equals(node2))
                {
                    continue;
                }
                else
                {
                    // if both vertices are reflex vertices, do SAT test
                    if (node1.vertexObject.CompareTag("ReflexVertex") && node2.vertexObject.CompareTag("ReflexVertex"))
                    {
                        direction = (node2.getPosition() - node1.getPosition()).normalized;
                        // if there is obstacle between node1 and node2, continue
                        if (Physics.Raycast(node1.getPosition(), direction, Vector3.Distance(node1.getPosition(), node2.getPosition())))
                        {
                            continue;
                        }
                        else  // if no obstacle in between, do SAT test
                        {
                            // we have vertices on x-z plane (fixed y), so set 2D vector of x and z
                            point1 = new Vector2(node1.getPosition().x, node1.getPosition().z);
                            point2 = new Vector2(node2.getPosition().x, node2.getPosition().z);
                            // handle cases where point1 of point2 has index of 0 or Length of array
                            if (i == 0)
                            {
                                point1Prev = new Vector2(((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().x, ((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().z);
                                point1Next = new Vector2(((Graph.Node)terrainVertices[i + 1]).getPosition().x, ((Graph.Node)terrainVertices[i + 1]).getPosition().z);
                            }
                            else if (i == terrainVertices.Count - 1)
                            {
                                point1Prev = new Vector2(((Graph.Node)terrainVertices[i - 1]).getPosition().x, ((Graph.Node)terrainVertices[i - 1]).getPosition().z);
                                point1Next = new Vector2(((Graph.Node)terrainVertices[0]).getPosition().x, ((Graph.Node)terrainVertices[0]).getPosition().z);
                            }
                            else
                            {
                                point1Prev = new Vector2(((Graph.Node)terrainVertices[i - 1]).getPosition().x, ((Graph.Node)terrainVertices[i - 1]).getPosition().z);
                                point1Next = new Vector2(((Graph.Node)terrainVertices[i + 1]).getPosition().x, ((Graph.Node)terrainVertices[i + 1]).getPosition().z);
                            }

                            if (k == 0)
                            {
                                point2Prev = new Vector2(((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().x, ((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().z);
                                point2Next = new Vector2(((Graph.Node)terrainVertices[k + 1]).getPosition().x, ((Graph.Node)terrainVertices[k + 1]).getPosition().z);
                            }
                            else if (k == terrainVertices.Count - 1)
                            {
                                point2Prev = new Vector2(((Graph.Node)terrainVertices[k - 1]).getPosition().x, ((Graph.Node)terrainVertices[k - 1]).getPosition().z);
                                point2Next = new Vector2(((Graph.Node)terrainVertices[0]).getPosition().x, ((Graph.Node)terrainVertices[0]).getPosition().z);
                            }
                            else
                            {
                                point2Prev = new Vector2(((Graph.Node)terrainVertices[k - 1]).getPosition().x, ((Graph.Node)terrainVertices[k - 1]).getPosition().z);
                                point2Next = new Vector2(((Graph.Node)terrainVertices[k + 1]).getPosition().x, ((Graph.Node)terrainVertices[k + 1]).getPosition().z);
                            }

                            // if the prev and next node of the reflex vertices is one to the left and one to the right as the SAT result, node1 and node2 are not bitangent
                            if (((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) > 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) < 0)
                                 || ((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) < 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) > 0)
                                 || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) > 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) < 0)
                                 || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) < 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) > 0)
                                )
                            {
                                // in this case, node1 and node2 are not bitangent
                                continue;
                            }
                            else
                            {
                                // otherwise, they are bitangent, add them as neighbor in RVGraph
                                node1.addNeightbor(node2);
                                node2.addNeightbor(node1);
                            }
                        }
                    }
                }

            }

        }


        // ------------------------
        //ArrayList obstacleVertices = new ArrayList();  // list stores obstacle vertices as Node
        ArrayList obstaclesList = new ArrayList();  // list stores list of obstacle vertices
        // Add obstacles reflex vertices into a list and them put each list into a list of obstacle
        foreach (GameObject obj in obstacles)
        {
            ArrayList obsNodes = new ArrayList();  // store the list of vertices of this obstacle as Nodes
            Transform[] allChildren = obj.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if(child.tag != "Cube")
                {
                    obsNodes.Add(new Graph.Node(child.position, child.gameObject));
                }
            }

            obstaclesList.Add(obsNodes);
        }

        // ------------------------
        // connect obstacle reflex vertices to obstacle reflex vertices if "bitangent", checking use SAT test
        foreach(ArrayList obstacleVertices in obstaclesList)
        {
            foreach (ArrayList obstacleVertices2 in obstaclesList)
            {
                for (int i = 0; i < obstacleVertices.Count; i++)
                {
                    node1 = (Graph.Node)obstacleVertices[i];

                    for (int k = 0; k < obstacleVertices2.Count; k++)
                    {
                        node2 = (Graph.Node)obstacleVertices2[k];

                        // skip if vertext1 and vertex2 are the same
                        if (node1.Equals(node2))
                        {
                            continue;
                        }
                        else
                        {
                            // if both vertices are reflex vertices, do SAT test
                            if (node1.vertexObject.CompareTag("ReflexVertex") && node2.vertexObject.CompareTag("ReflexVertex"))
                            {
                                direction = (node2.getPosition() - node1.getPosition()).normalized;
                                // if there is obstacle between node1 and node2, continue
                                if (Physics.Raycast(node1.getPosition(), direction, Vector3.Distance(node1.getPosition(), node2.getPosition())))
                                {
                                    continue;
                                }
                                else  // if no obstacle in between, do SAT test
                                {
                                    // we have vertices on x-z plane (fixed y), so set 2D vector of x and z
                                    point1 = new Vector2(node1.getPosition().x, node1.getPosition().z);
                                    point2 = new Vector2(node2.getPosition().x, node2.getPosition().z);
                                    // handle cases where point1 of point2 has index of 0 or Length of array
                                    if (i == 0)
                                    {
                                        point1Prev = new Vector2(((Graph.Node)obstacleVertices[obstacleVertices.Count - 1]).getPosition().x, ((Graph.Node)obstacleVertices[obstacleVertices.Count - 1]).getPosition().z);
                                        point1Next = new Vector2(((Graph.Node)obstacleVertices[i + 1]).getPosition().x, ((Graph.Node)obstacleVertices[i + 1]).getPosition().z);
                                    }
                                    else if (i == obstacleVertices.Count - 1)
                                    {
                                        point1Prev = new Vector2(((Graph.Node)obstacleVertices[i - 1]).getPosition().x, ((Graph.Node)obstacleVertices[i - 1]).getPosition().z);
                                        point1Next = new Vector2(((Graph.Node)obstacleVertices[0]).getPosition().x, ((Graph.Node)obstacleVertices[0]).getPosition().z);
                                    }
                                    else
                                    {
                                        point1Prev = new Vector2(((Graph.Node)obstacleVertices[i - 1]).getPosition().x, ((Graph.Node)obstacleVertices[i - 1]).getPosition().z);
                                        point1Next = new Vector2(((Graph.Node)obstacleVertices[i + 1]).getPosition().x, ((Graph.Node)obstacleVertices[i + 1]).getPosition().z);
                                    }

                                    if (k == 0)
                                    {
                                        point2Prev = new Vector2(((Graph.Node)obstacleVertices2[obstacleVertices2.Count - 1]).getPosition().x, ((Graph.Node)obstacleVertices2[obstacleVertices2.Count - 1]).getPosition().z);
                                        point2Next = new Vector2(((Graph.Node)obstacleVertices2[k + 1]).getPosition().x, ((Graph.Node)obstacleVertices2[k + 1]).getPosition().z);
                                    }
                                    else if (k == obstacleVertices2.Count - 1)
                                    {
                                        point2Prev = new Vector2(((Graph.Node)obstacleVertices2[k - 1]).getPosition().x, ((Graph.Node)obstacleVertices2[k - 1]).getPosition().z);
                                        point2Next = new Vector2(((Graph.Node)obstacleVertices2[0]).getPosition().x, ((Graph.Node)obstacleVertices2[0]).getPosition().z);
                                    }
                                    else
                                    {
                                        point2Prev = new Vector2(((Graph.Node)obstacleVertices2[k - 1]).getPosition().x, ((Graph.Node)obstacleVertices2[k - 1]).getPosition().z);
                                        point2Next = new Vector2(((Graph.Node)obstacleVertices2[k + 1]).getPosition().x, ((Graph.Node)obstacleVertices2[k + 1]).getPosition().z);
                                    }

                                    // if the prev and next node of the reflex vertices is one to the left and one to the right as the SAT result, node1 and node2 are not bitangent
                                    if (((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) > 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) < 0)
                                         || ((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) < 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) > 0)
                                         || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) > 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) < 0)
                                         || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) < 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) > 0)
                                        )
                                    {
                                        // in this case, node1 and node2 are not bitangent
                                        continue;
                                    }
                                    else
                                    {
                                        // otherwise, they are bitangent, add them as neighbor in RVGraph
                                        node1.addNeightbor(node2);
                                        node2.addNeightbor(node1);
                                    }
                                }
                            }
                        }

                    }

                }


            }
        }


        // ------------------------
        // connect obstacle reflex vertices to terrain reflex vertices if "bitangent", checking use SAT test
        foreach (ArrayList obstacleVertices in obstaclesList)
        {
            for (int i = 0; i < obstacleVertices.Count; i++)
            {
                node1 = (Graph.Node)obstacleVertices[i];

                for (int k = 0; k < terrainVertices.Count; k++)
                {
                    node2 = (Graph.Node)terrainVertices[k];

                    // skip if vertext1 and vertex2 are the same
                    if (node1.Equals(node2))
                    {
                        continue;
                    }
                    else
                    {
                        // if both vertices are reflex vertices, do SAT test
                        if (node1.vertexObject.CompareTag("ReflexVertex") && node2.vertexObject.CompareTag("ReflexVertex"))
                        {
                            direction = (node2.getPosition() - node1.getPosition()).normalized;
                            // if there is obstacle between node1 and node2, continue
                            if (Physics.Raycast(node1.getPosition(), direction, Vector3.Distance(node1.getPosition(), node2.getPosition())))
                            {
                                continue;
                            }
                            else  // if no obstacle in between, do SAT test
                            {
                                // we have vertices on x-z plane (fixed y), so set 2D vector of x and z
                                point1 = new Vector2(node1.getPosition().x, node1.getPosition().z);
                                point2 = new Vector2(node2.getPosition().x, node2.getPosition().z);
                                // handle cases where point1 of point2 has index of 0 or Length of array
                                if (i == 0)
                                {
                                    point1Prev = new Vector2(((Graph.Node)obstacleVertices[obstacleVertices.Count - 1]).getPosition().x, ((Graph.Node)obstacleVertices[obstacleVertices.Count - 1]).getPosition().z);
                                    point1Next = new Vector2(((Graph.Node)obstacleVertices[i + 1]).getPosition().x, ((Graph.Node)obstacleVertices[i + 1]).getPosition().z);
                                }
                                else if (i == obstacleVertices.Count - 1)
                                {
                                    point1Prev = new Vector2(((Graph.Node)obstacleVertices[i - 1]).getPosition().x, ((Graph.Node)obstacleVertices[i - 1]).getPosition().z);
                                    point1Next = new Vector2(((Graph.Node)obstacleVertices[0]).getPosition().x, ((Graph.Node)obstacleVertices[0]).getPosition().z);
                                }
                                else
                                {
                                    point1Prev = new Vector2(((Graph.Node)obstacleVertices[i - 1]).getPosition().x, ((Graph.Node)obstacleVertices[i - 1]).getPosition().z);
                                    point1Next = new Vector2(((Graph.Node)obstacleVertices[i + 1]).getPosition().x, ((Graph.Node)obstacleVertices[i + 1]).getPosition().z);
                                }

                                if (k == 0)
                                {
                                    point2Prev = new Vector2(((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().x, ((Graph.Node)terrainVertices[terrainVertices.Count - 1]).getPosition().z);
                                    point2Next = new Vector2(((Graph.Node)terrainVertices[k + 1]).getPosition().x, ((Graph.Node)terrainVertices[k + 1]).getPosition().z);
                                }
                                else if (k == terrainVertices.Count - 1)
                                {
                                    point2Prev = new Vector2(((Graph.Node)terrainVertices[k - 1]).getPosition().x, ((Graph.Node)terrainVertices[k - 1]).getPosition().z);
                                    point2Next = new Vector2(((Graph.Node)terrainVertices[0]).getPosition().x, ((Graph.Node)terrainVertices[0]).getPosition().z);
                                }
                                else
                                {
                                    point2Prev = new Vector2(((Graph.Node)terrainVertices[k - 1]).getPosition().x, ((Graph.Node)terrainVertices[k - 1]).getPosition().z);
                                    point2Next = new Vector2(((Graph.Node)terrainVertices[k + 1]).getPosition().x, ((Graph.Node)terrainVertices[k + 1]).getPosition().z);
                                }

                                // if the prev and next node of the reflex vertices is one to the left and one to the right as the SAT result, node1 and node2 are not bitangent
                                if (((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) > 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) < 0)
                                     || ((point2.x - point1.x) * (point2Prev.y - point1.y) - (point2Prev.x - point1.x) * (point2.y - point1.y) < 0 && (point2.x - point1.x) * (point2Next.y - point1.y) - (point2Next.x - point1.x) * (point2.y - point1.y) > 0)
                                     || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) > 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) < 0)
                                     || ((point1.x - point2.x) * (point1Prev.y - point2.y) - (point1Prev.x - point2.x) * (point1.y - point2.y) < 0 && (point1.x - point2.x) * (point1Next.y - point2.y) - (point1Next.x - point2.x) * (point1.y - point2.y) > 0)
                                    )
                                {
                                    // in this case, node1 and node2 are not bitangent
                                    continue;
                                }
                                else
                                {
                                    // otherwise, they are bitangent, add them as neighbor in RVGraph
                                    node1.addNeightbor(node2);
                                    node2.addNeightbor(node1);
                                }
                            }
                        }
                    }

                }

            }
        }


        // ------------------------
        // add all terrain reflex vertices into the RVgraph
        foreach (Graph.Node node in terrainVertices)
        {
            if (node.vertexObject.CompareTag("ReflexVertex"))
            {
                rvgraph.addNode(node);
            }
        }
        // add all obstacle reflex vertices into the RVgraph
        foreach (ArrayList obstacleVertices in obstaclesList)
        {
            foreach(Graph.Node node in obstacleVertices)
            {
                if (node.vertexObject.CompareTag("ReflexVertex"))
                {
                    rvgraph.addNode(node);
                }
            }
        }

        return rvgraph;
    }


    


    /* drawReducedVisibilityGraph */
    // function that draw the reduced visibility graph based on the Graph object
    void drawReducedVisibilityGraph(Graph g)
    {
        GameObject graphLine;
        LineRenderer lineRenderer;
        ArrayList reflexVerticesList = g.getReflexVertices();
        ArrayList neighborsList;
        Vector3[] verticesPosition = new Vector3[2];

        // iterate all nodes and draw a line bewtween them and their neighbors vertices
        foreach(Graph.Node vertex in reflexVerticesList)
        {
            verticesPosition[0] = vertex.getPosition();

            neighborsList = vertex.getNeighbors();
            foreach(Graph.Node neighbor in neighborsList)
            {
                // generate an object with lineRender
                graphLine = Instantiate(linePrefab, this.transform.position, this.transform.rotation);
                lineRenderer = graphLine.GetComponent<LineRenderer>();
                verticesPosition[1] = neighbor.getPosition();

                lineRenderer.SetPositions(verticesPosition);  // draw a line
            }
        }
    }

    // function to place vertex object on reflex vertices
    void placeVerticesObjects(Graph rvgraph, GameObject vertexPrefab)
    {
        Vector3 position;
        foreach (Graph.Node node in rvgraph.getReflexVertices())
        {
            position = node.getPosition();
            position.y = 0.11f;
            Instantiate(vertexPrefab, position, Quaternion.Euler(0, 0, 0));
        }
    }


    /* getCloneReducedVisibilityGraph */
    // function to return the RVGraph of current map (used by GameFlowManager for agents path finding)
    public Graph getReducedVisibilityGraph()
    {
        return reducedVisibilityGraph;
    }

    public ArrayList getObstaclesList()
    {
        return this.obstacleGameObjects;
    }

    public float getObstacleShrinkScale()
    {
        return this.shrinkObstacleScale;
    }    
        


}
