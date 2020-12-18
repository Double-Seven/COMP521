using System;

// Author: ZiQi Li
// A enum class represents the possible Task name for a HTN tree
// Will be used for precondition check and task execution check (Use switch statement to check which Task is the function input task)
// Note: we only have task name for Method Node and Primitive Node, since Compound Node doesn't have precondition to check
public enum Task
{
    DISTANCE_ATTACK,  // method

    CRATE_ATTACK,  // method
    ROCK_ATTACK,  // method

    MOVE_TO_CRATE,  // primitive task
    MOVE_TO_ROCK,  // primitive task
    THROW_CRATE,  // primitive task
    THROW_ROCK,  // primitive task

    IDLE,  // method

    YELLING_WANDER,  // method
    ONLY_WANDER,  // method

    WANDER,  // primitive task
    YELLING  // primitive task

}
