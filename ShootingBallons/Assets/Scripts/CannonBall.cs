using System;
using UnityEngine;
using SomeEnum;

//Author: ZiQi Li, for Comp521 A2, McGill University
//class for the shoot cannon balls, using for updating their position, velocity
public class CannonBall
{
    private GameObject cannonBall;
    private Vector3 position;
    private Vector3 velocity;



    public CannonBall(GameObject ballObject, Vector3 position, float muzzleVelocity, MuzzleDirection direction,  float muzzleAngle)
    {
        this.cannonBall = ballObject;
        this.position = position;
        //convert the vi to vix and viy
        if(direction == MuzzleDirection.RIGHT)
        {
            this.velocity.x = muzzleVelocity * Mathf.Cos(muzzleAngle * Mathf.PI / 180f);
            this.velocity.y = muzzleVelocity * Mathf.Sin(muzzleAngle * Mathf.PI / 180f);
        }
        else
        {
            this.velocity.x = -muzzleVelocity * Mathf.Cos(muzzleAngle * Mathf.PI / 180f);
            this.velocity.y = muzzleVelocity * Mathf.Sin(-muzzleAngle * Mathf.PI / 180f);
        }
    }


    //this function will update the postion of this cannonBall based on the current velocity
    //p' = p + v(detla t)
    public void updatePosition(float deltaTime)
    {
        this.cannonBall.transform.Translate(new Vector3(this.velocity.x, this.velocity.y) * deltaTime, Space.World);
        position = this.cannonBall.transform.position;
    }

    //this function will add an distance to the postion of this cannonBall using Translate
    public void translatePosition(float deltaTime, Vector2 motion)
    {
        this.cannonBall.transform.Translate(new Vector3(motion.x, motion.y) * deltaTime, Space.World);
        position = this.cannonBall.transform.position;
    }

    //this function will update the velocity of this cannonBall based on the acceleration (gravity in this case)
    //v' = v + a(detla t)
    public void updateVelocity(float deltaTime, Vector2 dVelocity)
    {
        //x-velocity stays the same if no wind resistance, we apply gravity to y-velocity
        this.velocity.x += dVelocity.x * deltaTime;
        this.velocity.y += dVelocity.y * deltaTime;
    }

    //this function will add a velocity to the current velocity
    public void addVelocity(Vector2 dVelocity)
    {
        this.velocity.x += dVelocity.x;
        this.velocity.y += dVelocity.y;
    }

    //this function will set a velocity to the current ball
    public void setVelocity(Vector2 velocity)
    {
        this.velocity.x = velocity.x;
        this.velocity.y = velocity.y;
    }

    //this function will update the rotation of the cannon ball (used to indicate the direction of motion of the ball)
    //The rotation (direction of motion) only depends on vx and vy
    public void updateRotation()
    {
        this.cannonBall.transform.eulerAngles = new Vector3(0, 0, 180f/Mathf.PI * Mathf.Atan(this.velocity.y/this.velocity.x));
    }

    //getter function for the game object
    public GameObject GetGameObject()
    {
        return this.cannonBall;
    }

    //getter function for the radius of this cannon ball
    public float getRadius()
    {
        return this.cannonBall.transform.localScale.x / 2;
    }

    //getter function for the postion of this cannonBall 
    public Vector3 getPosition()
    {
        return this.position;
    }

    //getter function for the velocity of this cannonBall 
    public Vector3 getVelocity()
    {
        return this.velocity;
    }




}
