using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;



// Author: ZiQi Li, COMP521
// This class will represent the Agents (NPCs) that moving in this map
public class Agent
{
    public Vector3 previousDest { get; set; }  // store the previous destination (use to count fails of attampts)
    public bool isWaiting { get; set; } = false;  // bool to indicate whether this agent is waiting the countdown time to be 0
    public GameObject highlightLine { get; set; }  // store the game object of the highlight line renderer
    public bool needReplanning { get; set; } = false;  // bool to indicate whether this agent is needing for replanning
    public int failsAttemps { get; set; } = 0;  // int to keep track the time the fails replanning (becomes 0 after reaches 3)
    // variable for experiment info
    public int numPathPlanned { get; set; } = 0;  // int to store the # of paths planned
    public int numReplanning { get; set; } = 0;  // int to store the # of replanning
    public int successReached { get; set; } = 0;  // int to store the # of plans successfully reached
    public float totalPlanningTime { get; set; } = 0;  // int to store the total planning time


    // game objects variable
    private GameObject agent = null;
    private Rigidbody agentBody = null;
    private GameObject goalObject = null;

    // agent attributes
    private float speed = 0;  // the speed of this agent
    private float agentRadius = 0;  // the radius of this agent
    private Color color;

    private float countDownTime = 0;  // count down timer

    // path finding variables
    private ArrayList path = new ArrayList();  // store the current path that this agent is followed
    private Graph originalRVGraph = null;  // store the original version of reduced visibility graph on this map (i.e. Not connecting with start and goal)
    private ArrayList currentLineFollowed = new ArrayList();  // array to store the current line segment (2 vector3) on the path that agent is following


    // constructor 
    public Agent(Graph graph, float speed)
    {
        originalRVGraph = graph;
        this.speed = speed;

    }

    /// <summary>
    /// function that make this agent follow the current path,
    /// return false if currently no path is to be taken (0 node in path)
    /// </summary>
    /// <param name="deltatime"></param>
    /// <returns>true if the agent is following a path, false if reaching the destination</returns>
    public bool followPath()
    {
        // if no node is in path, (reach destination) return false
        if(path.Count == 0)
        {
            this.agentBody.velocity = Vector3.zero;
            return false;
        }
        else
        {
            Graph.Node nextNode = (Graph.Node) path[0];
            Vector3 direction = (nextNode.getPosition() - this.agent.transform.position).normalized;

            // if there is another agent on the moving direction of this agent and the distance between them is < radius + 0.1f,
            // make this agent to replanning after 0.1s~0.5s

            // Bit shift the index of the layer (8) to get a bit mask
            // This would cast rays only against colliders in layer 8 (the layer for agents)
            int layerMask = 1 << 8;
            // No using layer, since we also wanna detect the situation where agent is stuck on wall

            RaycastHit hit;
            Ray ray = new Ray();
            ray.origin = agent.transform.position;
            ray.direction = direction;

            // we want to detect possible collision in 45degree angle infront of agent (22.5degree left and 22.5degree right)
            Vector3 rotateLeft = Quaternion.Euler(0, -22.5f, 0) * direction;
            Vector3 rotateRight = Quaternion.Euler(0, 22.5f, 0) * direction;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, agentRadius + 0.1f, layerMask) ||
                Physics.Raycast(agent.transform.position, rotateLeft, agentRadius + 0.1f, layerMask) ||
                Physics.Raycast(agent.transform.position, rotateRight, agentRadius + 0.1f, layerMask))
            //if (Physics.Raycast(agent.transform.position, direction, agentRadius + 0.15f))
            {
                //Debug.Log(hit.transform.name);
                // in this case, there is another agent on the way, so we need to replan
                this.needReplanning = true;
                this.numReplanning++;  // update info

                return false;
            }
            else
            {
                // if reaching the next node (distance <= 0.05f), remove the node from path node list
                if (Vector2.Distance(new Vector2(this.agent.transform.position.x, this.agent.transform.position.z), new Vector2(nextNode.getPosition().x, nextNode.getPosition().z)) <= 0.1f)
                {
                    path.RemoveAt(0);  // remove the current next node
                                       // if reach destination
                    if (path.Count == 0)
                    {
                        this.agentBody.velocity = Vector3.zero;
                        return false;
                    }
                    else
                    {
                        nextNode = (Graph.Node)path[0];
                        // move toward the new Next node
                        direction = (nextNode.getPosition() - this.agent.transform.position).normalized;
                        this.agentBody.velocity = direction * this.speed;
                    }
                }
                else // else, move toward the current Next node
                {
                    // move toward the new Next node
                    //this.agentBody.MovePosition(agent.transform.position + agent.transform.InverseTransformPoint(nextNode.getPosition()) * deltatime * this.speed);
                    this.agentBody.velocity = direction * this.speed;
                }
                return true;
            }

            
        }
    }


    // function that finds a path to the destination based on the reduced visibility graph using A* algorithm
    public void pathFinding(Vector3 destination)
    {
        // count how many time is spent on this Planning function
        float startTime = Time.realtimeSinceStartup;

        // update info, if destination is the same, update the fails attempts
        if(this.previousDest.Equals(destination))
        {
            this.failsAttemps++;
        }
        else  // if we have a new destination, reset the fail attempts
        {
            this.failsAttemps = 0;
        }
        this.numPathPlanned++;  // update # of path planned
        this.previousDest = destination;  // update previous destination


        ArrayList solutionPath = new ArrayList();
        Vector3 direction;
        direction = -(destination - agent.transform.position).normalized;  // note that we reverse the direction to avoid raycast doesn't work from inside a collider

        /*
        // change the obstacle back to its original scale
        InitializationManager.initManager.changeObstaclesSize(InitializationManager.initManager.getObstaclesList(), new Vector3(-InitializationManager.initManager.getObstacleShrinkScale(), 0, -InitializationManager.initManager.getObstacleShrinkScale() + 0.1f));
        //** Modifying a Transform doesn't immediately change physics components, so we have to call SyncTransforms to manually synchronize it 
        Physics.SyncTransforms();
        */

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;
        // This would cast rays only against colliders in layer 8 (agent layer)
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        // if the start (current position of agent) is visible to the destination, set the current path to [start -> destination]
        if (!Physics.Raycast(destination, direction, Vector3.Distance(agent.transform.position, destination), layerMask))
        {
            solutionPath.Add(new Graph.Node(this.agent.transform.position, null));
            solutionPath.Add(new Graph.Node(destination, null));
            this.path = solutionPath;  // set the current PF path for this agent to the solution path
        }
        else  // else, connect the start and the destination of this agent to the visible vertices of this clone RVGraph
        {
            Graph cloneGraph = originalRVGraph.cloneGraph();  // make a copy the the original RVGraph

            // connect the start and the destination of this agent to the visible vertices of this clone RVGraph
            ArrayList reflexVertices = cloneGraph.getReflexVertices();
            Graph.Node start = new Graph.Node(this.agent.transform.position, null);
            Graph.Node goal = new Graph.Node(destination, null);


            foreach (Graph.Node vertex in reflexVertices)
            {
                // note that we reverse the direction to avoid raycast doesn't work from inside a collider
                // In this case since we change the scaling of obstacle, our agent may be in obstacle when raycast!!
                direction = -(vertex.getPosition() - this.agent.transform.position).normalized;
                // connect the start to this reflex vertex if visible
                //if (!Physics.Raycast(this.agent.transform.position, direction, Vector3.Distance(this.agent.transform.position, vertex.getPosition()), layerMask))
                if (!Physics.Raycast(vertex.getPosition(), direction, Vector3.Distance(this.agent.transform.position, vertex.getPosition()), layerMask))
                {
                    vertex.addNeightbor(start);
                    start.addNeightbor(vertex);
                }

                direction = (vertex.getPosition() - destination).normalized;
                // connect the goal to this reflex vertex if visible
                if (!Physics.Raycast(destination, direction, Vector3.Distance(destination, vertex.getPosition()), layerMask))
                {
                    vertex.addNeightbor(goal);
                    goal.addNeightbor(vertex);
                }
            }

            // add the start node and goal node into the vertices list
            reflexVertices.Add(start);
            reflexVertices.Add(goal);



            /* Now we can apply A* alogorithm to find the path from start node to goal node on the RVGraph */
            // For A* algorithm, f(v) = g(v) + h(v), we will use Euclidean distance for heuristic func h(v)

            // Array that contains the optimal distance we've found from the starting state to the current node v
            // i.e. this array stores g(v), v is the current node
            float[] bestDistFound = new float[reflexVertices.Count];
            // initialize g(v): set g(v) = INF for all nodes except the start node
            for(int i = 0; i < bestDistFound.Length; i++)
            {
                // if it's the start node
                if(i == reflexVertices.IndexOf(start))
                {
                    bestDistFound[i] = 0;  // start node has g(start) = 0
                }
                else
                {
                    bestDistFound[i] = Mathf.Infinity;
                }
            }

            // Array that contains the Lower bound on cost of path from source to destination 
            // that passes through v
            // i.e. this array stores f(v)
            float[] lowerBoundDistToDest = new float[reflexVertices.Count];
            // initialize f(v): set f(v) = INF for all nodes except the start node
            for (int i = 0; i < lowerBoundDistToDest.Length; i++)
            {
                // if it's the start node
                if (i == reflexVertices.IndexOf(start))
                {
                    lowerBoundDistToDest[i] = Vector3.Distance(start.getPosition(), goal.getPosition());  // start node has f(start) = h(start)
                }
                else
                {
                    lowerBoundDistToDest[i] = Mathf.Infinity;
                }
            }

            // use a priority queue for this algorithm (node as item, float as priority)
            SimplePriorityQueue<Graph.Node, float> priorityQueue = new SimplePriorityQueue<Graph.Node, float>();
            // add start node to priority queue with priority f(start)
            priorityQueue.Enqueue(start, lowerBoundDistToDest[reflexVertices.IndexOf(start)]);

            // use an array to store whether node at (x,y) has been visited
            bool[] isVisited = new bool[reflexVertices.Count];


            // use a dictionary to keep track the parent of a node
            // ex: "key: value", value is the parent of the key 
            Dictionary<Graph.Node, Graph.Node> parents = new Dictionary<Graph.Node, Graph.Node>();
            parents.Add(start, null);  // the start node has no parent

            Graph.Node v;
            // while the priority queue is not empty
            while(priorityQueue.Count != 0)
            {
                v = priorityQueue.Dequeue();  // get the node v with minimum f(v) from queue Q and remove it

                // if v is the goal, them done, follow the parent pointers from v to get the path
                if(v.Equals(goal))
                {
                    solutionPath.Add(v);
                    Graph.Node parent = parents[v];
                    while(parent != null)
                    {
                        solutionPath.Add(parent);
                        parent = parents[parent];
                    }
                    // reverse the solution path, since currently is from goal to start
                    solutionPath.Reverse();
                    this.path = solutionPath;  // set the current PF path for this agent to the solution path
                    break;
                }

                isVisited[reflexVertices.IndexOf(v)] = true;  // set v to visited

                // iterate the neighbor u of v
                foreach(Graph.Node u in v.getNeighbors())
                {
                    // igore visited u
                    if(isVisited[reflexVertices.IndexOf(u)])
                    {
                        continue;
                    }

                    // if u is not in Q
                    if(!priorityQueue.Contains(u))
                    {
                        // g(u) = g(v) + d(v,u)
                        bestDistFound[reflexVertices.IndexOf(u)] = bestDistFound[reflexVertices.IndexOf(v)] + Vector3.Distance(v.getPosition(), u.getPosition());
                        // add u in Q with priority f(u) = g(u) + h(u)
                        float priority = bestDistFound[reflexVertices.IndexOf(u)] + Vector3.Distance(u.getPosition(), goal.getPosition());
                        priorityQueue.Enqueue(u, priority);

                        // set the parent of u to v
                        parents[u] = v;
                    }
                    // else if g(v) + d(u,v) < g(u)
                    else if(bestDistFound[reflexVertices.IndexOf(v)] + Vector3.Distance(v.getPosition(), u.getPosition()) < bestDistFound[reflexVertices.IndexOf(u)])
                    {
                        // update the g(u)=g(v)+d(v,u)
                        bestDistFound[reflexVertices.IndexOf(u)] = bestDistFound[reflexVertices.IndexOf(v)] + Vector3.Distance(v.getPosition(), u.getPosition());

                        // update the priority of u: f(u) in Queue
                        float priority = bestDistFound[reflexVertices.IndexOf(u)] + Vector3.Distance(u.getPosition(), goal.getPosition());
                        priorityQueue.UpdatePriority(u, priority);

                        // set the parent of u to v
                        parents[u] = v;
                    }
                }


            }

        }
        /*
        // change the obstacle back to its shrink scale
        InitializationManager.initManager.changeObstaclesSize(InitializationManager.initManager.getObstaclesList(), new Vector3(InitializationManager.initManager.getObstacleShrinkScale(), 0, InitializationManager.initManager.getObstacleShrinkScale() - 0.1f));
        //** Modifying a Transform doesn't immediately change physics components, so we have to call SyncTransforms to manually synchronize it 
        Physics.SyncTransforms();
        */

        // update info
        this.totalPlanningTime += (Time.realtimeSinceStartup - startTime);
    }

    // setter method for the GameObject of this agent
    public void setGameObject(GameObject agent)
    {
        this.agent = agent;
        this.agentBody = this.agent.GetComponent<Rigidbody>();
        this.agentRadius = this.agent.GetComponent<SphereCollider>().radius * this.agent.transform.lossyScale.x;
    }

    // setter method for the goal object
    public void setGoalGameObject(GameObject goal)
    {
        this.goalObject = goal;
    }

    // getter method for the GameObject of this agent
    public GameObject getGameObject()
    {
        return this.agent;
    }

    // getter method for the GameObject of the goal object
    public GameObject getGoalGameObject()
    {
        return this.goalObject;
    }

    // function to change the agent default color to its random color
    public void setRandColor()
    {
        // randomly assign a color to this agent when instantiating
        Color randColor = new Color();
        randColor.a = 1;
        randColor.r = (float) UnityEngine.Random.Range(0, 255) / 255;
        randColor.b = (float) UnityEngine.Random.Range(0, 255) / 255;
        randColor.g = (float) UnityEngine.Random.Range(0, 255) / 255;
        color = randColor;

        // get the renderer component of this agent
        MeshRenderer mr = this.agent.GetComponent<MeshRenderer>();
        mr.material.SetColor("_Color", this.color);
    }

    // function to get the color of this agent (use for goal object generation)
    public Color getColor()
    {
        return this.color;
    }

    // function to return the current path of this agent
    public ArrayList getPath()
    {
        return this.path;
    }

    // function to return the list (2 Nodes in list) that store the current line segment on the path that agent is following
    public ArrayList getCurrentLineSegmentFollowing()
    {
        return currentLineFollowed;
    }


    // function to set a time count down for this agent (will be used for choosing a new destination after a count down time)
    public void setTimer(float time)
    {
        this.countDownTime = time;  // set the count down time to time
    }

    /// <summary>
    /// function to check whether to count down timer is becoming 0 for this agent
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns>True if count down is finish (becomes 0)</returns>
    public bool checkTimer(float deltaTime)
    {
        this.countDownTime -= deltaTime;  // count the timer down by deltaTime
        if(this.countDownTime <= 0)
        {
            return true;  // return true if the count down timer becomes 0
        }
        else
        {
            return false;
        }
    }


    // function to high light the current line segment that agent is following
    public void updateHighLightLineSeg()
    {
        // when this agent is just created
        if(this.highlightLine == null)
        {
            if(this.path.Count >= 2)
            {
                Vector3 end1 = ((Graph.Node)path[0]).getPosition();
                Vector3 end2 = ((Graph.Node)path[1]).getPosition();
                this.currentLineFollowed.Add(end1);
                this.currentLineFollowed.Add(end2);
                this.highlightLine = GameFlowManager.gameFlowManager.highLightPath(end1, end2);  // set the current highlight line gameobject
            }
            else
            {
                Vector3 end1 = this.agent.transform.position;
                Vector3 end2 = ((Graph.Node)path[0]).getPosition();
                this.currentLineFollowed.Add(end1);
                this.currentLineFollowed.Add(end2);
                this.highlightLine = GameFlowManager.gameFlowManager.highLightPath(end1, end2);  // set the current highlight line gameobject
            }
        }
        else
        {
            if(path.Count >= 1)
            {
                // if the current path has to be update
                if (((Graph.Node)this.path[0]).getPosition() != (Vector3)this.currentLineFollowed[1])
                {
                    Vector3 end = ((Graph.Node)path[0]).getPosition();
                    //Vector3 end2 = ((Graph.Node)path[1]).getPosition();
                    this.currentLineFollowed[0] = this.currentLineFollowed[1];
                    this.currentLineFollowed[1] = end;
                    GameFlowManager.Destroy(this.highlightLine);  // destroy the current highlight line
                    this.highlightLine = GameFlowManager.gameFlowManager.highLightPath((Vector3)this.currentLineFollowed[0], end);  // set the new highlight line gameobject
                }
            }
        }
    }




}
