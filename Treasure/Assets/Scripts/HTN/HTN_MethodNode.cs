using System;
public class HTN_MethodNode: HTN_node
{

    private Task taskName;  // store the task type of this method node

    public HTN_MethodNode(Task taskName)
    {
        this.taskName = taskName;
    }

    /// <summary>
    /// Function to verify the precondition of the node using the current worldState and the Task name (enum)
    /// </summary>
    /// <param name="worldState"></param>
    /// <returns>Return true if the preconditions are satisfied by the current World State, otherwise return false</returns>
    public bool verifyPrecondition(HTN_WorldState worldState)
    {
        Task taskName = this.taskName;
        /*
        // use switch of the task name to check precondition (maybe have a better way to do it??)
        switch (taskName)
        {
            //case Task.ROOT_TASK:
                //return true;  // since root task has not precondition

            // Method Tasks:
            case Task.DISTANCE_ATTACK:
                // if the task is the Method: DISTANCE_ATTACK, check the world state
                if (worldState.isInAttackRange && worldState.hasObstablesOnMap)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.CRATE_ATTACK:
                // if the task is the Method: DISTANCE_ATTACK, check the world state
                if (worldState.isInAttackRange && worldState.hasObstablesOnMap)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.ROCK_ATTACK:
                // if the task is the Method: DISTANCE_ATTACK, check the world state
                if (worldState.isInAttackRange && worldState.hasObstablesOnMap)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.IDLE:
                // if the task is the Method: IDLE, return always true, since it doesn't depend on the World State
                return true;
        }
        */
        //return false;  // for unexcepted error

        // for simplify the tree, we only put precondition for primitive task, so every method will be valid
        return true;
    }


}
