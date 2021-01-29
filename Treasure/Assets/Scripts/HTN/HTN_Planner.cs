using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// Author: ZiQi Li
// This class represents the Simple forward planner for a HTN tree
public class HTN_Planner
{
    private System.Object agent;

    private HTN_WorldState currentWorldState;  // store the current world state that the planner is considering
    private ArrayList plan = new ArrayList();  // list to store the plan output by the planner

    private float timer = 0f;  // using for applying 1second cool-down between each plan

    //private bool hasReplanning = false;  // used for update UI text

    public HTN_Planner(System.Object agent, HTN_WorldState ws)
    {
        this.agent = agent;
        this.currentWorldState = ws;
    }





    /// <summary>
    /// Function to generate a plan for a given root node of HTN tree
    /// </summary>
    /// <param name="rootNode"></param>
    public void planning(HTN_node rootNode)
    {
        HTN_WorldState state = this.currentWorldState.clone();  // make a copy of the current state
        ArrayList finalPlanList = new ArrayList();
        Stack<HTN_node> tasksNodes = new Stack<HTN_node>();

        // stack for store Method node for backtracking
        Stack<ArrayList> backTrackingMethods = new Stack<ArrayList>();  // each list in this stack is a list of methods
        // stack for store planList for backtracking
        Stack<ArrayList> backTrackingPlan = new Stack<ArrayList>();
        Stack<HTN_WorldState> states = new Stack<HTN_WorldState>();  // store the copy of states for backtracking


        tasksNodes.Push(rootNode);  // put the root node into the task stack

        // while the stack is not empty
        while(!(tasksNodes.Count == 0))
        {
            HTN_node taskNode = tasksNodes.Pop();

            // if the node is a compound node
            if(taskNode is HTN_CompoundNode)
            {
                ArrayList methods = ((HTN_CompoundNode)taskNode).findSatisfiedMethods(this.currentWorldState);
                //HTN_MethodNode method = ((HTN_CompoundNode)taskNode).findSatisfiedMethod(this.currentWorldState);
                HTN_MethodNode method = (HTN_MethodNode) methods[0];
                methods.RemoveAt(0);

                if (method != null)
                {
                    // store the plan and the next method node for backtracking when needed
                    backTrackingPlan.Push(new ArrayList(finalPlanList));  // clone the current plan list
                    backTrackingMethods.Push(methods);
                    states.Push(state.clone()); 

                    // push the subtasks of this method into the list (push backward since we want the first subtask to be pop next)
                    for(int i = method.getChildren().Count - 1; i >= 0; i--)
                    {
                        tasksNodes.Push((HTN_node) method.getChildren()[i]);
                    }
                    
                }
                else
                {
                    // restore to the saved backtracking state
                    state = states.Pop();
                    finalPlanList = backTrackingPlan.Pop();
                    tasksNodes.Clear();  // empty the tasksNode stack

                    // pop a methods list
                    ArrayList btMethods = backTrackingMethods.Pop();
                    while(btMethods.Count == 0)
                    {
                        btMethods = backTrackingMethods.Pop();
                    }

                    // get the first method in that list
                    HTN_MethodNode btMethodNode = (HTN_MethodNode) btMethods[0];
                    btMethods.RemoveAt(0);  // delete that method from the list
                    if (btMethods.Count > 0)
                        backTrackingMethods.Push(btMethods);  // put the method list back

                    // push the subtasks of this method into the list (push backward since we want the first subtask to be pop next)
                    for (int i = btMethodNode.getChildren().Count - 1; i >= 0; i--)
                    {
                        tasksNodes.Push((HTN_node)btMethodNode.getChildren()[i]);
                    }
                }

            }
            else  // primative node
            {
                // if the precondition is met
                if(((HTN_PrimitiveNode)taskNode).verifyPrecondition(state))
                {
                    // apply the effect of this task on the world state
                    ((HTN_PrimitiveNode)taskNode).applyTask(state);
                    finalPlanList.Add(taskNode);  // add this taskNode to the plan list

                    // if this primitive task is the last in the stack, our planning is finish
                    if(tasksNodes.Count == 0)
                    {
                        break;
                    }
                    
                }
                else
                {
                    // restore to the saved backtracking state
                    state = states.Pop();
                    finalPlanList = backTrackingPlan.Pop();
                    tasksNodes.Clear();  // empty the tasksNode stack

                    // pop a methods list
                    ArrayList btMethods = backTrackingMethods.Pop();
                    while (btMethods.Count == 0)
                    {
                        btMethods = backTrackingMethods.Pop();
                    }

                    // get the first method in that list
                    HTN_MethodNode btMethodNode = (HTN_MethodNode)btMethods[0];
                    btMethods.RemoveAt(0);  // delete that method from the list
                    if (btMethods.Count > 0)
                        backTrackingMethods.Push(btMethods);  // put the method list back

                    // push the subtasks of this method into the list (push backward since we want the first subtask to be pop next)
                    for (int i = btMethodNode.getChildren().Count - 1; i >= 0; i--)
                    {
                        tasksNodes.Push((HTN_node)btMethodNode.getChildren()[i]);
                    }
                }
            }

        }

        this.plan = finalPlanList;  // set the current plan to the final plan list
    }



    /// <summary>
    /// Function to execute the current plan
    /// </summary>
    public void executePlan(float deltaTime)
    {

        MonsterController mc = ((MonsterController)this.agent);

        // if plan is not empty, execute tasks in the current plan
        if (this.plan.Count != 0)
        {
            HTN_PrimitiveNode taskNode = (HTN_PrimitiveNode)plan[0];

            Task taskName = taskNode.taskName;


            // use switch of the task name to check precondition (maybe have a better way to do it??)
            switch (taskName)
            {
                //case Task.ROOT_TASK:
                //return true;  // since root task has not precondition

                // Primitive Tasks:
                case Task.MOVE_TO_CRATE:
                    // if the task is the Primitive task: MOVE_TO_CRATE, check the world state
                    if (taskNode.verifyPrecondition(currentWorldState))
                    {
                        // execute task
                        if(mc.gotoObstacle(mc.gameInitalizer.getObstacleList(), Task.MOVE_TO_CRATE))
                        {
                            // remove this task from plan when return is true (task is complete)
                            this.plan.RemoveAt(0);  // remove the current task 
                        }
                           
                    }
                    else
                    {
                        // otherwise replan
                        planning(mc.getRootNode());
                        //hasReplanning = true;
                    }
                    break;

                case Task.MOVE_TO_ROCK:
                    // if the task is the Primitive task: MOVE_TO_ROCK, check the world state
                    if (taskNode.verifyPrecondition(currentWorldState))
                    {
                        // execute task
                        if (mc.gotoObstacle(mc.gameInitalizer.getObstacleList(), Task.MOVE_TO_ROCK))
                        {
                            // remove this task from plan when return is true (task is complete)
                            this.plan.RemoveAt(0);  // remove the current task 
                        }
                    }
                    else
                    {
                        // otherwise replan
                        planning(mc.getRootNode());
                        //hasReplanning = true;
                    }
                    break;

                case Task.THROW_CRATE:
                    // if the task is the Primitive task: THROW_CRATE, check the world state
                    if (taskNode.verifyPrecondition(currentWorldState))
                    {
                        // execute task
                        if (mc.throwObstacle())
                        {
                            // remove this task from plan when return is true (task is complete)
                            this.plan.RemoveAt(0);  // remove the current task 
                        }
                    }
                    else
                    {
                        // otherwise replan
                        planning(mc.getRootNode());
                        //hasReplanning = true;
                    }
                    break;

                case Task.THROW_ROCK:
                    // if the task is the Primitive task: THROW_ROCK, check the world state
                    if (taskNode.verifyPrecondition(currentWorldState))
                    {
                        // execute task
                        if (mc.throwObstacle())
                        {
                            // remove this task from plan when return is true (task is complete)
                            this.plan.RemoveAt(0);  // remove the current task 
                        }
                    }
                    else
                    {
                        // otherwise replan
                        planning(mc.getRootNode());
                        //hasReplanning = true;
                    }
                    break;

                case Task.YELLING:
                    // if the task is the Primitive task: YELLING, check the world state
                    if (taskNode.verifyPrecondition(currentWorldState))
                    {
                        // if yelling() return true: yelling is finish
                        if(mc.yelling())
                        {
                            this.plan.RemoveAt(0);  // remove the current task 
                        }
                    }
                    else
                    {
                        // otherwise replan
                        planning(mc.getRootNode());
                        //hasReplanning = true;
                    }
                    break;

                case Task.WANDER:

                    // execute task
                    if (mc.wander())
                    {
                        // remove this task from plan when return is true (task is complete)
                        this.plan.RemoveAt(0);  // remove the current task 
                    }
                    break;
            }
        }
        else
        {
            // cool-down period
            if (this.timer <= 1f)
            {
                this.timer += deltaTime;
            }
            else  // 1second cool-down between plans
            {
                this.timer = 0;  // reset the timer
                // make new plan
                planning(mc.getRootNode());
            }

        }
        

       
    }

    /*
    public void printPlan()
    {
        if(plan.Count != 0)
        {
            foreach(HTN_PrimitiveNode node in plan)
            {
                Debug.Log(node.taskName);
            }
        }
        else
        {
            Debug.Log("No plan");
        }
    }*/


    /// <summary>
    /// Function to return the current plan in list of string (task names)
    /// </summary>
    /// <returns></returns>
    public ArrayList getPlanInString()
    {
        ArrayList planInString = new ArrayList();

        if (plan.Count != 0)
        {
            foreach (HTN_PrimitiveNode node in plan)
            {
                planInString.Add(node.taskName.ToString());
            }
        }
        else
        {
            planInString.Add("1 second cool-down");
        }

        return planInString;
    }


    /// <summary>
    /// Function to update the world state in this planner
    /// </summary>
    /// <param name="ws"></param>
    public void updateWorldState(HTN_WorldState ws)
    {
        this.currentWorldState.isInAttackRange = ws.isInAttackRange;
        this.currentWorldState.isInFrontOfObstacle = ws.isInFrontOfObstacle;
        this.currentWorldState.hasCratesInRange = ws.hasCratesInRange;
        this.currentWorldState.isTheChosenObstacleDestroyed = ws.isTheChosenObstacleDestroyed;
    }
}
