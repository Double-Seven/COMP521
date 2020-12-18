using System;
using System.Collections;




// Author: ZiQi Li
// A class represents the node of a HTN (hierarchical task network) tree, using for NPC AI decisions
// Note: this class HTN_node has child classes: Methond node, Compound node and Primitive node
public class HTN_node
{
    protected ArrayList children = new ArrayList();  // the list stores the children nodes of this node
    //private NodeType thisNodeType { get; set; }  // store the type of this node
    protected ArrayList preConditions = new ArrayList();


    public HTN_node()
    {
        
    }

    /// <summary>
    /// Function to add child node to this node
    /// </summary>
    /// <param name="childNode"></param>
    public void addChild(HTN_node childNode)
    {
        this.children.Add(childNode);
    }


    /// <summary>
    /// Function to get the children nodes of this node
    /// </summary>
    /// <returns></returns>
    public ArrayList getChildren()
    {
        return this.children;
    }

    /*
    /// <summary>
    /// Function to return the type of this node
    /// </summary>
    /// <returns></returns>
    public NodeType getType()
    {
        return this.thisNodeType;
    }*/




}
