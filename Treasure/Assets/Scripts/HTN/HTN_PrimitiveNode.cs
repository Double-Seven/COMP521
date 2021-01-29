using System;
public class HTN_PrimitiveNode: HTN_node
{


    public Task taskName { set; get; }  // the task name assigned with this node


    public HTN_PrimitiveNode(Task taskName)
    {
        this.taskName = taskName;
    }


    


    /// <summary>
    /// Function to apply the task of this primative node to change the world state
    /// </summary>
    public void applyTask(HTN_WorldState worldState)
    {
        Task taskName = this.taskName;

        // use switch of the task name to check precondition (maybe have a better way to do it??)
        switch (taskName)
        {
            //case Task.ROOT_TASK:
            //return true;  // since root task has not precondition

            // Primitive Tasks:
            case Task.MOVE_TO_CRATE:
                worldState.isInFrontOfObstacle = true;
                break;

            case Task.MOVE_TO_ROCK:
                worldState.isInFrontOfObstacle = true;
                break;

            case Task.THROW_CRATE:
                
            case Task.THROW_ROCK:

            case Task.WANDER:
                break;  // wander won't have a effect on the Game state
        }
    }


    /// <summary>
    /// Function to verify the precondition of the node using the current worldState and the Task name (enum)
    /// </summary>
    /// <param name="worldState"></param>
    /// <returns>Return true if the preconditions are satisfied by the current World State, otherwise return false</returns>
    public bool verifyPrecondition(HTN_WorldState worldState)
    {
        Task taskName = this.taskName;

        // use switch of the task name to check precondition (maybe have a better way to do it??)
        switch (taskName)
        {
            //case Task.ROOT_TASK:
                //return true;  // since root task has not precondition

            // Primitive Tasks:
            case Task.MOVE_TO_CRATE:
                // if the task is the Primitive task: MOVE_TO_CRATE, check the world state
                if (worldState.isInAttackRange && worldState.hasCratesInRange)
                {
                    return true;
                }
                else
                {
                    return false;
                }


            case Task.MOVE_TO_ROCK:
                // if the task is the Primitive task: MOVE_TO_ROCK, check the world state
                if (worldState.isInAttackRange)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            case Task.THROW_CRATE:
                // if the task is the Primitive task: THROW_CRATE, check the world state
                if (worldState.isInFrontOfObstacle && worldState.isInAttackRange && !worldState.isTheChosenObstacleDestroyed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.THROW_ROCK:
                // if the task is the Primitive task: THROW_ROCK, check the world state
                if (worldState.isInFrontOfObstacle && worldState.isInAttackRange && !worldState.isTheChosenObstacleDestroyed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.YELLING:
                // if the task is the Primitive task: YELLING, check the world state
                if (worldState.isInYellingRange)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case Task.WANDER:
                    return true;  // return always true for wander task
        }

        return false;  // for unexcepted error
    }


}
