using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: ZiQi Li
// this class will use Steering Force for the motion of slimes (mice in the original Assignment description)
// The implementation of Steering Force is inspired by AleksandrHovhannisyan's "Steering Behavior Demo: Spherical Boids"
// reference: https://github.com/AleksandrHovhannisyan/Steering-Behaviors
public class SlimeController : MonoBehaviour
{
    // Public variables
    public float wanderRadius = 0f;  // the wander radius for slimes
    public float maxForce = 0f;  // the force limit
    public float maxSpeed = 0f;  // the speed limit of this slime

    // Private variables
    private Rigidbody thisRB;  // rigid body component of this slime
    private float desiredSeparationDistance;  // the disired separation distance for this slime
    private float desiredSeparationDistance_obstacles;  // the disired separation distance bewtween obstacles and this slime
    private float desiredSeparationDistance_walls;  // the disired separation distance bewtween wall and this slime
    private GameFlowManager gameFlowManager;

    private float wanderTimer = 1f;  // use for wander function
    private Vector3 currentWanderForce;

    void Awake()
    {
        thisRB = this.gameObject.GetComponent<Rigidbody>();

        //desiredSeparationDistance = this.transform.localScale.x * 2;  // initialize the desiredSeparationDistance
        desiredSeparationDistance = 3f;
        desiredSeparationDistance_obstacles = 4f;
        desiredSeparationDistance_walls = 2.1f;

        gameFlowManager = GameObject.FindWithTag("GameController").GetComponent<GameFlowManager>(); // get the instance of the game flow manager script
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Function to calculate the wander force for this slime
    /// </summary>
    /// <returns></returns>
    public Vector3 wander(float deltaTime)
    {
        // idea: generate a random point in a sphere(circle) in front of the current moving direction of this slime
        // so that the slime won't wander backward suddenly

        // set a new wander direction every 1 seconds, so the slimes won't "vibrate" due to turning at each frame
        if(wanderTimer >= 1f)
        {

            float rand = Random.Range(0, 1f);

            // slimes have 3% of chance to take a break
            if(rand <= 0.003f)
            {
                wanderTimer = -3f;  // take 3s + 0.5s break

                this.currentWanderForce = Vector3.zero;  // update the current wander force
                return Vector3.zero;
            }
            else
            {
                wanderTimer = 0;

                Vector3 velocityDirection = thisRB.velocity.normalized;
                Vector3 randPosition = Random.insideUnitSphere * this.wanderRadius;  // get a rand point inside a sphere
                randPosition += this.transform.position + velocityDirection;  // apply this rand position around the a point on the current moving direction of this monster(as center)
                randPosition.y = thisRB.transform.position.y;  // move on x-z plane

                Vector3 desiredVelocity = (randPosition - this.transform.position).normalized * maxSpeed;

                // Get the wander force
                Vector3 wanderForce = desiredVelocity - this.thisRB.velocity;
                wanderForce.y = 0;  // disable y axis movement

                // Check whether the wander force exceed the force limit
                if (wanderForce.magnitude > maxForce)
                {
                    wanderForce = wanderForce.normalized * maxForce;
                }

                this.currentWanderForce = wanderForce;  // update the current wander force
                return wanderForce;
            }

        }
        else
        {
            wanderTimer += deltaTime;
            return this.currentWanderForce;
        }

    }



    
    /// <summary>
    /// Function to calculate the flee force for this slime
    /// </summary>
    /// <returns></returns>
    public Vector3 flee()
    {
        Vector3 totalFleeVector = Vector3.zero;  // the total force caused by flee behavior of this slime

        ArrayList all_objs_to_avoid = new ArrayList();

        // add all object to avoid into the same list
        foreach (GameObject obstacle in gameFlowManager.agents_to_avoid)
        {
            all_objs_to_avoid.Add(obstacle);
        }
        foreach (GameObject obstacle in gameFlowManager.spots_to_avoid)
        {
            all_objs_to_avoid.Add(obstacle);
        }
        foreach (GameObject obstacle in gameFlowManager.slime_to_avoid)
        {
            all_objs_to_avoid.Add(obstacle);
        }

        int numCloseObjects = 0;  // keep count the objects which are too close to this slime

        // iterate the list of rest objects to avoid
        foreach (GameObject obj in all_objs_to_avoid)
        {
            // if the obj is not this slime itself
            if(obj != this.gameObject)
            {
                Vector3 objPostion = obj.transform.position;
                objPostion.y = this.transform.position.y;  // calculate position based on the same x-z plane
                float distance = Vector3.Distance(this.transform.position, objPostion);  // calculate the distance between this slime and the object

                // if the distance is too close, we have to add flee force
                if(distance < this.desiredSeparationDistance)
                {
                    numCloseObjects++;
                    // get the normalized vector from the object to this slime
                  
                    Vector3 normalSepVector = (this.transform.position - objPostion).normalized;

                    normalSepVector /= distance;  // if the distance is small, the separation vector will be big (we will flee with more force)
                    totalFleeVector += normalSepVector;
                }
            }
        }

        // iterate the obstacle list separately, since we will have different separation distance for obstacles
        foreach (GameObject obstacle in gameFlowManager.obstacles_to_avoid)
        {
            Vector3 objPostion = obstacle.transform.position;
            objPostion.y = this.transform.position.y;  // calculate position based on the same x-z plane
            float distance = Vector3.Distance(this.transform.position, objPostion);  // calculate the distance between this slime and the object

            // if the distance is too close, we have to add flee force
            if (distance < this.desiredSeparationDistance_obstacles)
            {
                numCloseObjects++;
                // get the normalized vector from the object to this slime

                Vector3 normalSepVector = (this.transform.position - objPostion).normalized;

                normalSepVector /= distance;  // if the distance is small, the separation vector will be big (we will flee with more force)
                totalFleeVector += normalSepVector;
            }
        }

        // iterate the wall list separately, since we will have different separation distance for obstacles
        foreach (GameObject wall in gameFlowManager.walls_to_avoid)
        {
            Vector3 objPostion = wall.transform.position;
            objPostion.y = this.transform.position.y;  // calculate position based on the same x-z plane
            float distance = Vector3.Distance(this.transform.position, objPostion);  // calculate the distance between this slime and the object

            // if the distance is too close, we have to add flee force
            if (distance < this.desiredSeparationDistance_walls)
            {
                numCloseObjects++;
                // get the normalized vector from the object to this slime

                Vector3 normalSepVector = (this.transform.position - objPostion).normalized;

                normalSepVector /= distance;  // if the distance is small, the separation vector will be big (we will flee with more force)
                totalFleeVector += normalSepVector;
            }
        }



        // if this slime has to flee from any object
        if (numCloseObjects > 0)
        {
            // calculate the average flee vector for this slime
            Vector3 avgFleeVector = (totalFleeVector / numCloseObjects).normalized;
            avgFleeVector *= maxSpeed;  // apply the max speed to this flee vector -> getting the expected flee velocity

            // compute the supplementary flee force we have to apply (expected velocity - current velocity)
            Vector3 fleeForce = avgFleeVector - this.thisRB.velocity;

            // Check whether the flee force exceed the force limit
            if(fleeForce.magnitude > this.maxForce)
            {
                fleeForce = fleeForce.normalized * maxForce;
            }

            fleeForce.y = 0;  // disable vertical force
            return fleeForce;
        }

        return Vector3.zero;  // return 0 force if no object is close to this slime (no flee force)

    }


    /// <summary>
    /// Detection of the collision with slime
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        // delete slime from the game scene when collided by monster or projectiles
        if (other.gameObject.tag == "Rock" || other.gameObject.tag == "Crate")
        {
            // only delete slimes when hitten by flying obstacles
            if (other.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
            {
                gameFlowManager.slime_to_avoid.Remove(this.gameObject);
                Destroy(this.gameObject);
            }

        }
        else if(other.gameObject.tag == "Monster")
        {
            gameFlowManager.slime_to_avoid.Remove(this.gameObject);
            Destroy(this.gameObject);
        }




    }

}
