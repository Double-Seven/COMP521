using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SomeEnum;
using UnityEngine.UI;
using System;

//Author: ZiQi Li, for Comp521 A2, McGill University
public class GameFlowManager : MonoBehaviour
{
    public InitializationManager initManager;
    public PlayerController playerController;
    public float gravity = -9f;
    public GameObject balloonLinePrefab;
    public GameObject stringLinePrefab;
    public Text windForceText;


    private ArrayList cannonBallsList;  //array list that store the shoot cannon balls in the game
    private Vector3[] terrainCollisionPoints = new Vector3[8];  //array store the points of the terrain collision line
    private Vector3[] waterCollisionPoints = new Vector3[2];

    //boundary values:
    private float leftBoundary = 0f;
    private float rightBoundary = 18f;
    private float bottonBoundary = -5f;
    private float topBoundary = 5.5f;
    private float waterLineStart = 7f;
    private float waterLineEnd = 10.5f;
    private float waterLineLevel = -1.35f;
    private float mountainTop = 1f;  //1

    //other variables
    private float timeCounter = 0f;  //time counter for balloon generation
    private float timeCounter2 = 0f;  //time counter for wind force changing
    private float balloonRisingForce = 0.1f;
    private float maxWindForce = 0.3f;
    private float currentWindForce = 0f;  //the current wind force will be generated randomly within the range [-maxWindForce, maxWindForce]
    private ArrayList balloonsList;

    // Start is called before the first frame update
    void Start()
    {
        //update the terrainCollisionPoints and waterCollisionPoints from the initialization class
        GameObject terrainLine = (GameObject) initManager.getTerrainCollisionLine();
        LineRenderer terrainLineRenderer = terrainLine.GetComponent<LineRenderer>();
        terrainLineRenderer.GetPositions(terrainCollisionPoints);
        GameObject waterLine = (GameObject)initManager.getWaterCollisionLine();
        LineRenderer waterLineRenderer = waterLine.GetComponent<LineRenderer>();
        waterLineRenderer.GetPositions(waterCollisionPoints);

        //instantiate the array list
        balloonsList = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Update for physical motion
    void FixedUpdate()
    {
        //update the time counter, will be used for balloon generation
        timeCounter += Time.deltaTime;
        timeCounter2 += Time.deltaTime;

        //if a balloon is generated, we clear the time counter, since we want to generate one balloon every second
        if(generateBalloon(balloonLinePrefab, stringLinePrefab, timeCounter, balloonsList, waterLineStart, waterLineEnd, waterLineLevel))
        {
            timeCounter = 0f;  //reset the time counter
        }

        //change the wind force randomly every 2 seconds
        if(timeCounter2 > 2)
        {
            currentWindForce = UnityEngine.Random.Range(-maxWindForce, maxWindForce);
            timeCounter2 = 0f;  //reset the time counter
            SetMuzzleVelocityText();
        }

        //apply wind force on balloons
        applyWindOnBalloons(currentWindForce, balloonsList);


        cannonBallsList = playerController.getCannonBallsList();

        //update the existed cannon balls states and detect collision 
        for(int i = 0; i < cannonBallsList.Count; i++)
        {
            //update acc
            ((CannonBall) cannonBallsList[i]).updateVelocity(Time.deltaTime, new Vector2(0,gravity));
            //detect and handle collisions (with balloon, terrain and water)
            handleBalloonCollision(((CannonBall)cannonBallsList[i]), balloonsList);
            handleCannonBallCollision(((CannonBall)cannonBallsList[i]), terrainCollisionPoints, ColliderType.TERRAIN);
            ((CannonBall)cannonBallsList[i]).updatePosition(Time.deltaTime);
            ((CannonBall)cannonBallsList[i]).updateRotation();

            handleCannonBallCollision(((CannonBall)cannonBallsList[i]), waterCollisionPoints, ColliderType.WATER);
            
        }

        //update the position and update the line renderers (draw) for each balloon generated
        foreach (Balloon b in balloonsList)
        {
            //handle collision between balloon and terrain line
            handleBalloonTerrainCollision(b, terrainCollisionPoints);

            b.updatePosition(Time.deltaTime, new Vector3(0, balloonRisingForce, 0));
            b.updateRenderer();
        }

        //remove out of screen and stationary balls
        for (int i = 0; i < cannonBallsList.Count; i++)
        {
            removeUselessBall(((CannonBall)cannonBallsList[i]), cannonBallsList);
        }

        //remove out of screen balloon
        for (int i = 0; i < balloonsList.Count; i++)
        {
            removeOutScrennBalloon(((Balloon)balloonsList[i]), balloonsList);
        }
    }


    //function to detect and handle collision bewtween cannon ball and collision lines (water, terrain and balloons)
    //taking one cannon ball and an array of points of the line (line with corners) as paramters, detect the collsion
    void handleCannonBallCollision(CannonBall ball, Vector3[] linePoints, ColliderType type)
    {
        GameObject ballObject = ball.GetGameObject();

        //check that if any line segment of the line is collided with the cannon ball
        for(int i = 0; i < linePoints.Length - 1; i++)
        {
            //if the line which the ball is colliding with is terrain
            if(type == ColliderType.TERRAIN)
            {
                // if the cannon ball is collided with the line
                if (CollisionCalculation.isCircleCollidesLine(linePoints[i].x, linePoints[i].y, linePoints[i + 1].x, linePoints[i + 1].y, ballObject.transform.position.x, ballObject.transform.position.y, ball.getRadius()))
                {
                    //if the ball is colliding with horizontal ground
                    if(linePoints[i].y - linePoints[i+1].y == 0)
                    {
                        //apply an inverse acceleration to the gravity to cancel out the gravity
                        ball.updateVelocity(Time.deltaTime, new Vector2(0, -gravity));
                        //apply a friction to the ball when rolling on horizontal ground
                        applyFrictionOfGround(ball);
                    }
                    else  //when the ball is not colliding with the horizontal terrain, push it back by a small amount to avoid triggering continuous collision detection
                    {
                        //push the ball away from the terrain line in its normal vector direction by a small
                        //amount to avoid penetration
                        Vector2 unitNormal = CollisionCalculation.getNormal(linePoints[i].x, linePoints[i].y, linePoints[i + 1].x, linePoints[i + 1].y);
                        ball.translatePosition(Time.deltaTime, unitNormal*1);
                    }

                    //handle the collision on terrain
                    bouncingAfterTerrainCollision(ball, linePoints[i].x, linePoints[i].y, linePoints[i + 1].x, linePoints[i + 1].y);
                    
                }
            }
            else  //if the line which the ball is colliding with is water
            {
                // if the cannon ball is collided with the line
                if (CollisionCalculation.isCircleCollidesLine(linePoints[i].x, linePoints[i].y, linePoints[i + 1].x, linePoints[i + 1].y, ballObject.transform.position.x, ballObject.transform.position.y, ball.getRadius()))
                {
                    //handle the collision on water
                    disappearAfterWaterCollision(ball, cannonBallsList);
                    
                }
            }

        }
    }


    //this function will handle the collision between cannon ball and any ballon body
    void handleBalloonCollision(CannonBall cb, ArrayList balloonList)
    {
        //vectors that store the position of points that construct the balloons
        Vector3[] bodyPositions;
        Vector3[] stringPositions;
        //get every ballon as line segment
        for(int k = 0; k < balloonsList.Count; k++)
        {
            bodyPositions = ((Balloon)balloonsList[k]).getBodyLinePositions();
            stringPositions = ((Balloon)balloonsList[k]).getStringLinePositions();

            //iterate through the Points to check if the cannon ball collided any line segment of the balloon string
            for (int i = 0; i < stringPositions.Length - 1; i++)
            {
                // if the cannon ball is collided with the line segment of a balloon string
                if (CollisionCalculation.isCircleCollidesLine(stringPositions[i].x, stringPositions[i].y, stringPositions[i + 1].x, stringPositions[i + 1].y, cb.GetGameObject().transform.position.x, cb.GetGameObject().transform.position.y, cb.getRadius()))
                {
                    //handle the collision by moving the string to avoid intersecting with the cannon ball
                    movingAfterCannonBallCollision(cb, ((Balloon)balloonsList[k]), stringPositions[i], stringPositions[i+1]);
                }

            }
                            
            //iterate through the Points to check if the cannon ball collided any line segment of the balloon body
            for (int i = 0; i < bodyPositions.Length - 1; i++)
            {
                // if the cannon ball is collided with the line segment of a balloon body
                if (CollisionCalculation.isCircleCollidesLine(bodyPositions[i].x, bodyPositions[i].y, bodyPositions[i + 1].x, bodyPositions[i + 1].y, cb.GetGameObject().transform.position.x, cb.GetGameObject().transform.position.y, cb.getRadius()))
                {
                    //handle the collision by destroy the balloon which has been collided
                    destroyAfterCannonBallCollision(((Balloon)balloonsList[k]), balloonList);
                    
                    break; //break since the balloon with current positions array is destroyed, so no further check has to be made
                }
                
            }
            
        }   
    }

    //this function will handle the collision between terrain and ballon
    void handleBalloonTerrainCollision(Balloon b, Vector3[] terrainLinePoints)
    {
        Vector3[] bodyPositions = b.getBodyLinePositions();
        Vector3[] stringPositions = b.getStringLinePositions();
        for (int j = 0; j < terrainLinePoints.Length - 1; j++)
        {
            //iterate through the Points to check if the cannon ball collided any line segment of the balloon body
            for (int i = 0; i < bodyPositions.Length - 1; i++)
            {
                //check if the Point of balloon is collided with any terrain line segment

                //if the balloon is collided with terrain
                //if (CollisionCalculation.isPointOnLine(bodyPositions[i].x, bodyPositions[i].y, terrainCollisionPoints[j].x, terrainCollisionPoints[j].y, terrainCollisionPoints[j + 1].x, terrainCollisionPoints[j + 1].y))
                if (CollisionCalculation.isCircleCollidesLine(terrainCollisionPoints[j].x, terrainCollisionPoints[j].y, terrainCollisionPoints[j + 1].x, terrainCollisionPoints[j + 1].y, bodyPositions[i].x, bodyPositions[i].y, 0.05f))
                {
                    //handle the collision
                    movingAfterLineCollision(b, bodyPositions[i], terrainCollisionPoints[j], terrainCollisionPoints[j + 1]);
                }
            }

            //check if the Point of string is collided with any terrain line segment
            for(int i = 0; i < stringPositions.Length - 1; i++)
            {
                //if the string is collided with terrain
                if (CollisionCalculation.isCircleCollidesLine(terrainCollisionPoints[j].x, terrainCollisionPoints[j].y, terrainCollisionPoints[j + 1].x, terrainCollisionPoints[j + 1].y, stringPositions[i].x, stringPositions[i].y, 0.05f))
                {
                    //handle the collision
                    movingAfterLineCollision(b, stringPositions[i], terrainCollisionPoints[j], terrainCollisionPoints[j+1]);
                }
            }
            
        }
    }

    //function for the disappearing solution of collison on water
    void disappearAfterWaterCollision(CannonBall ball, ArrayList ballList)
    {
        //when fall in water, remove the ball from the ball list and destroy it
        ballList.Remove(ball);
        Destroy(ball.GetGameObject());
    }


    //function for the bouncing solution of collison, taking a ball and a line represented by two points
    //as parameters
    void bouncingAfterTerrainCollision(CannonBall ball, float x1, float y1, float x2, float y2)
    {
        //calculate bouncing velocity using the formula V+ = V- + dotProduct(j,n)/m,
        //where V- is the velocity when colliding, V+ is the velocity after colliding,
        //dotProduct(j,n) = J which is the impulse, and j = -(1+epsilon) * m * Vn- where
        //Vn- = dotProduct(V-,n).

        Vector2 unitNormal = CollisionCalculation.getNormal(x1, y1, x2, y2);  //calculate the unit normal of the line 
        float epsilon = 0.55f;  //the coefficient of restitution (0~1, when it's 1 we have perfect bouncing)
        float mass = 1f;  //mass of the ball

        float VnCollided =  unitNormal.x * ball.getVelocity().x + unitNormal.y * ball.getVelocity().y;  //Vn-: the normal component of collided velocity = dot product of normal and velocity

        float j = -(1 + epsilon) * mass * VnCollided;  //the impulse scalar j = -(1+epsilon) * m * Vn-
        Vector2 impulse = j * unitNormal;  //J

        ball.addVelocity(impulse/mass);  //update the velocity based on the impulse (V+ = V- + impulse/mass)
    }

    //function for destroying balloon after it's collided with a cannon ball
    void destroyAfterCannonBallCollision(Balloon b, ArrayList balloonList)
    {
        //when the cannon ball hit a balloon body, destroy the balloon
        Destroy(b.getBodyLine());
        Destroy(b.getStringLine());
        balloonList.Remove(b);
    }


    //function for changing the string motion after it's collided with cannon ball (the string shouldn't intersect will cannon ball)
    //the paramters are the cannon ball and the balloon whose string is collided with, and also the position of the two end of the line segment of the string
    void movingAfterCannonBallCollision(CannonBall ball, Balloon balloon, Vector3 startPoint, Vector3 endPoint)
    {
        //find the 2 Point objects of the balloon string according to the startPoint and endPoint
        //Balloon.Point point1 = null;
        Balloon.Point point2 = null;
        ArrayList strPoints = balloon.getStringPoints();
        for(int i = 0; i < strPoints.Count; i++)
        {
            /*
            if( ((Balloon.Point) strPoints[i]).getPosition().Equals(startPoint))
            {
                point1 = (Balloon.Point) strPoints[i];
            }*/
            if (((Balloon.Point)strPoints[i]).getPosition().Equals(endPoint))
            {
                point2 = (Balloon.Point)strPoints[i];
            }
        }

        
        Vector2 unitNormal = CollisionCalculation.getNormal(startPoint.x, startPoint.y, point2.getPosition().x, point2.getPosition().y);  //calculate the unit normal of the line 
        float epsilon = 0.1f;  //the coefficient of restitution (0~1, when it's 1 we have perfect bouncing)
        float mass = 1f;  //mass of the string

        float VnCollided = unitNormal.x * ball.getVelocity().x + unitNormal.y * ball.getVelocity().y;  //Vn-: the normal component of collided velocity = dot product of normal and velocity

        float j = -(1 + epsilon) * mass * VnCollided;  //the impulse scalar j = -(1+epsilon) * m * Vn-
        Vector2 impulse = -j * unitNormal;  //the force will apply to the line, not the cannon ball, so we use negative of the impulse

        point2.setAcceleration((impulse / mass));  //update the acceleration based on the impulse
    }


    //function for changing the balloon point motion after it's collided with line (the balloon Point shouldn't intersect will terrain)
    //the paramters are the balloon and the points of a line which the balloon is collided with, and also the position of the two end of the line segment
    void movingAfterLineCollision(Balloon balloon, Vector3 collidedPoint, Vector3 lineStart, Vector3 lineEnd)
    {
        //find the 2 Point objects of the balloon string according to the startPoint and endPoint
        //Balloon.Point point1 = null;
        Balloon.Point cPoint = null;
        ArrayList strPoints = balloon.getStringPoints();
        ArrayList bodyPoints = balloon.getBodyPoints();

        //iterate to find the contact point
        for (int i = 0; i < strPoints.Count; i++)
        {
            if (((Balloon.Point)strPoints[i]).getPosition().Equals(collidedPoint))
            {
                cPoint = (Balloon.Point)strPoints[i];
            }
        }
        for (int i = 0; i < bodyPoints.Count; i++)
        {
            if (((Balloon.Point)bodyPoints[i]).getPosition().Equals(collidedPoint))
            {
                cPoint = (Balloon.Point)bodyPoints[i];
            }
        }


        Vector2 unitNormal = CollisionCalculation.getNormal(lineStart.x, lineStart.y, lineEnd.x, lineEnd.y);  //calculate the unit normal of the line segment
        float epsilon = 1f;  //the coefficient of restitution (0~1, when it's 1 we have perfect bouncing)
        float mass = 0.1f;  //mass of the string

        float VnCollided = unitNormal.x * cPoint.getMovingDirection().x + unitNormal.y * cPoint.getMovingDirection().y;  //Vn-: the normal component of collided velocity = dot product of normal and velocity
        float multiplier = 30f;

        float j = -(1 + epsilon) * mass * -VnCollided * multiplier;  //the impulse scalar j = -(1+epsilon) * m * Vn-
        Vector2 impulse = -j * unitNormal;  //the force will apply to the line, not the cannon ball, so we use negative of the impulse

        foreach(Balloon.Point p in bodyPoints)
        {
            p.setAcceleration((impulse / mass));  //update the acceleration based on the impulse
        }
        foreach (Balloon.Point p in strPoints)
        {
            p.setAcceleration((impulse / mass));  //update the acceleration based on the impulse
        }

        //cPoint.setAcceleration((impulse / mass));  //update the acceleration based on the impulse
    }


    //function for the friction on the horizontal ground
    void applyFrictionOfGround(CannonBall ball)
    {
        //use a friction coefficient to decelerate the ball
        float frictionCoeff = 0.95f;
        ball.addVelocity(new Vector2(-ball.getVelocity().x * (1 - frictionCoeff), 0));
    }


    //function for applying horizontal wind to the balloon
    void applyWindOnBalloons(float windForce, ArrayList balloonList)
    {
        ArrayList balloonBodyList;  //reference to the Point list of the balloon body
        ArrayList balloonStringList;

        //check for every balloon in the balloon list
        foreach(Balloon b in balloonsList)
        {
            balloonBodyList = b.getBodyPoints();
            balloonStringList = b.getStringPoints();

            foreach(Balloon.Point p in balloonBodyList)
            {
                //if the Point y position is higher than the mountain top, apply wind force to it
                if(p.getPosition().y > mountainTop)
                {
                    p.addAcceleration(new Vector3(windForce, 0, 0));
                }
            }
        }
    }


    //function to remove cannon balls if they are out of screen or remain stationary for a long time
    void removeUselessBall(CannonBall ball, ArrayList ballList)
    {
        if(ball.getPosition().x < leftBoundary || ball.getPosition().x > rightBoundary || ball.getPosition().y < bottonBoundary)
        {
            //when out of scree, remove the ball from the ball list and destroy it
            ballList.Remove(ball);
            Destroy(ball.GetGameObject());
        }
        else if(Mathf.Abs(ball.getVelocity().x) < 0.0001f)
        {
            //the ball's x-velocity is really small (caused by friction), remove the ball from the ball list and destroy it
            //note that it must been a long time for the ball staying on the ground to have such a small x-velocty
            ballList.Remove(ball);
            Destroy(ball.GetGameObject());
        }
    }


    //function to generate randomly positionned balloons at about one per second and add it to the balloon list, return true if a balloon is generated
    bool generateBalloon(GameObject balloonPrefab, GameObject stringPrefab, float timeCount, ArrayList bList, float waterLineS, float waterLineE, float waterLineY)
    {
        //generate a balloon every second
        if(timeCount > 1f)
        {
            //find a random x-position on the water surface
            float spawnX = UnityEngine.Random.Range(waterLineS, waterLineE);
            Vector3 spawnPos = new Vector3(spawnX, waterLineY, 0);
            //generate 2 line objects for the creation of balloon
            GameObject bodyLine = Instantiate(balloonPrefab, this.transform.position, this.transform.rotation);
            GameObject stringLine = Instantiate(stringPrefab, this.transform.position, this.transform.rotation);
            Balloon newBalloon = new Balloon(bodyLine, stringLine, spawnPos);
            bList.Add(newBalloon);

            return true;
        }
        else
        {
            return false;
        }
    }

    //function to remove balloon if they are out of screen
    void removeOutScrennBalloon(Balloon b, ArrayList balloonList)
    {
        if (b.getTopPosition().x < leftBoundary || b.getTopPosition().x > rightBoundary || b.getTopPosition().y > topBoundary)
        {
            //when out of scree, remove the ball from the ball list and destroy it
            Destroy(b.getBodyLine());
            Destroy(b.getStringLine());
            balloonList.Remove(b);
        }
    }



    //set the wind speed text
    void SetMuzzleVelocityText()
    {
        float speedText;
        if(currentWindForce >= 0)
        {
            speedText = (float) Math.Round((double) (60.0f * currentWindForce),1);
            windForceText.text = "Current wind: East " + speedText.ToString() + " m/s";
        }
        else
        {
            speedText = (float)Math.Round((double)(60.0f * -currentWindForce), 1);
            windForceText.text = "Current wind: West " + speedText.ToString() + " m/s";
        }
    }

}
