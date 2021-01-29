using System;
using System.Collections;
using UnityEngine;


// Author: ZiQi Li, COMP521
// This class will be used to store the reduced visibility graph of our map
public class Graph
{
    // inner class for Node (vertex)
    public class Node
    {
        public GameObject vertexObject { get; set; }
        private Vector3 position;
        private ArrayList neighbors = new ArrayList();  // stores the neighbor nodes of this node

        public Node(Vector3 position, GameObject vertexObject)
        {
            this.position = position;
            this.vertexObject = vertexObject;
        }

        public Vector3 getPosition()
        {
            return position;
        }

        // function returns the neighbors list of this node
        public ArrayList getNeighbors()
        {
            return neighbors;
        }

        // function to add neighbors to this node
        public void addNeightbor(Node neighbor)
        {
            if(neighbors.Contains(neighbor))
            {
                return;
            }
            else
            {
                neighbors.Add(neighbor);
            }
        }

        
    }

    /* We don't need edge for A* algorithm, just neighbor vertices is fine.
    // inner class for Edge 
    public class Edge
    {
        private Node[] edgeEnds = new Node[2];  // stores the 2 end nodes of this edge
        float edgeLength = 0;


        public Edge(Node node1, Node node2)
        {
            this.edgeEnds[0] = node1;
            this.edgeEnds[1] = node2;
            node1.addNeightbor(node2);
            node2.addNeightbor(node1);

            edgeLength = Vector3.Distance(edgeEnds[0].getPosition(), edgeEnds[1].getPosition());
        }

        // function returns the 2 ends of this edge
        public Node[] getNodes()
        {
            return edgeEnds;
        }

        // function returns the length of this edge
        public float getLength()
        {
            return edgeLength;
        }
    }*/



    private ArrayList reflexVertices = new ArrayList();
    //private ArrayList graphEdges = new ArrayList();

    public Graph()
    {

    }

    // function to add nodes to this graph
    public void addNode(Node node)
    {
        reflexVertices.Add(node);
    }

    /*
    // function to add edges to this graph
    public void addEdge(Node node1, Node node2)
    {
        reflexVertices.Add(new Edge(node1, node2));
    }*/

    // function to get the reflexVertices list
    public ArrayList getReflexVertices()
    {
        return reflexVertices;
    }


    // function to return a deep copy of this graph
    public Graph cloneGraph()
    {
        Graph copyGraph = new Graph();
        //ArrayList nodes = new ArrayList();

        // deep copy nodes
        foreach(Node node in this.reflexVertices)
        {
            copyGraph.addNode(new Node(node.getPosition(), node.vertexObject));
        }

        
        // add neighbors to the nodes we just copied
        for(int i=0; i < reflexVertices.Count; i++)
        {
            foreach(Node neighbor in ((Node) reflexVertices[i]).getNeighbors())
            {
                ((Node)copyGraph.getReflexVertices()[i]).addNeightbor((Node) copyGraph.getReflexVertices()[reflexVertices.IndexOf(neighbor)]);
            }
        }

        return copyGraph;
        
    }
}
