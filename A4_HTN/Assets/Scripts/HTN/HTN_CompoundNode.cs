using System;
using System.Collections;




// Author: ZiQi Li
// A child class of HTN_node represents the Compound Node
public class HTN_CompoundNode: HTN_node
{



    public HTN_CompoundNode()
    {
        
    }


    /// <summary>
    /// Funtion to return the first method that the World State satisfies its precondition
    /// (This function will only be called by a Compound node)
    /// </summary>
    /// <param name="worldState"></param>
    /// <returns></returns>
    public HTN_MethodNode findSatisfiedMethod(HTN_WorldState worldState)
    {
        // iterate all the chilren of this compound method (which should always be Method nodes)
        foreach (HTN_MethodNode method in this.children)
        {
            // return the first Method node which its precondition is satisfied by the current World State 
            if (method.verifyPrecondition(worldState))
            {
                return method;
            }
        }

        // if not found, return null
        return null;
    }


    /// <summary>
    /// Funtion to return all methods that the World State satisfies its precondition
    /// (This function will only be called by a Compound node)
    /// </summary>
    /// <param name="worldState"></param>
    /// <returns></returns>
    public ArrayList findSatisfiedMethods(HTN_WorldState worldState)
    {
        ArrayList satisfiedMethods = new ArrayList();

        // iterate all the chilren of this compound method (which should always be Method nodes)
        foreach (HTN_MethodNode method in this.children)
        {
            // return the first Method node which its precondition is satisfied by the current World State 
            if (method.verifyPrecondition(worldState))
            {
                satisfiedMethods.Add(method);
            }
        }

        
        return satisfiedMethods;
    }
}
