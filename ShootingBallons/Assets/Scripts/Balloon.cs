using System;
using System.Collections;
using UnityEngine;


//Author: ZiQi Li, for Comp521 A2, McGill University
//class for the rising balloon, implementing using Verlet integration,
//used for updating their position with respecting to the constraints between line segements that form this balloon
public class Balloon
{
    /*-------------------------------------------------------------------------*/
    //an inner class for the invisible links between points which construct a balloon shape
    //Note: The link is not the Line connnecting points, it's invisible and defined to keep the skeleton of the shape
    public class Link
    {
        private Point point1;
        private Point point2;
        private float restingDistance; //the resting distance is the length constraint between the 2 points connected by this link

        public Link(Point pt1, Point pt2, float restingD)
        {
            this.point1 = pt1;
            this.point2 = pt2;
            this.restingDistance = restingD;
        }

        //this function will adjust the position of the 2 points connected by this Link to keep the distance between them
        //as the restingDistance (constraint of this link)
        public void solve()
        {
            float diffX = point1.getPosition().x - point2.getPosition().x;
            float diffY = point1.getPosition().y - point2.getPosition().y;
            //get the current distance between those 2 points
            float distance = CollisionCalculation.getDistance(point1.getPosition().x, point1.getPosition().y, point2.getPosition().x, point2.getPosition().y);

            //find the ratio of how far between the actual distance and the resting distance
            float ratioDiff = (restingDistance - distance) / distance;

            //an multiplier to make the adjustment more natural
            float stiffness = 1f;  
            float scalarP1 = (0.5f) * stiffness;  // mass1/(mass1+mass2) = 0.5, since for our task point have same mass
            float scalarP2 = stiffness - scalarP1;

            //move the point1 toward point2 to attain the resting distance constraint
            float newPosX = point1.getPosition().x + scalarP1 * ratioDiff * diffX;
            float newPosY = point1.getPosition().y + scalarP1 * ratioDiff * diffY;
            point1.setPosition(new Vector3(newPosX, newPosY));
            //move the point2 toward point1 to attain the resting distance constraint
            newPosX = point2.getPosition().x - scalarP2 * ratioDiff * diffX;
            newPosY = point2.getPosition().y - scalarP2 * ratioDiff * diffY;
            point2.setPosition(new Vector3(newPosX, newPosY));
        }


    }


    /*-------------------------------------------------------------------------*/

    //an inner class for the points which used as joints of the balloon shape
    public class Point
    {
        //for using Verlet ingegration, we have to keep track the previous position, current position and the acceleration
        //to calculate the next position of the Point
        private Vector3 previousPosition;
        private Vector3 currentPosition;
        private Vector3 acceleration;

        //store the invisible Links connected to this Point in an ArrayList
        ArrayList links = new ArrayList();

        public Point(Vector3 position)
        {
            this.currentPosition = position;
            this.previousPosition = this.currentPosition;
            this.acceleration = new Vector3(0, 0, 0);
        }


        //function to update the point position using Verlet integration
        public void updatePosition(float deltaTime)
        {
            Vector3 currentPos = this.currentPosition;
            //p'(next position) = p + (p - p*) + a*t
            this.currentPosition = this.currentPosition + (this.currentPosition - this.previousPosition) + this.acceleration * deltaTime;
            //change the previous position to the last current position
            this.previousPosition = currentPos;

            //reset the acceleration
            acceleration = new Vector3(0, 0, 0);
        }

        //getter function for the current position of the Point
        public Vector3 getPosition()
        {
            return this.currentPosition;
        }

        //setter function for the current position of the Point
        public void setPosition(Vector3 pos)
        {
            this.currentPosition.x = pos.x;
            this.currentPosition.y = pos.y;
        }

        //setter function for the acceleration of the Point
        public void setAcceleration(Vector3 acc)
        {
            this.acceleration = acc;
        }

        //function to add to the current acceleration
        public void addAcceleration(Vector3 acc)
        {
            this.acceleration += acc;
        }

        //this function will create a link between this Point and the Point pt with a resting distance between these 2 points
        //and add this link into the link arrayList of this point
        public void addLink(Point pt, float restingDistance)
        {
            links.Add(new Link(this, pt, restingDistance));
        }

        //this function will solve the constraints on the links connected to this point by adjust the Points position
        public void solveConstraints()
        {
            foreach(Link l in links)
            {
                l.solve();
            }
        }

        //this function will give the moving direction (with the speed) of this Point based on the position infomation
        public Vector3 getMovingDirection()
        {
            return new Vector3(this.currentPosition.x - this.previousPosition.x, this.currentPosition.y - this.previousPosition.y);
        }

    }
    /*-------------------------------------------------------------------------*/


    //using Line game object to present this balloon
    private GameObject balloonBody;
    private GameObject balloonString;
    private LineRenderer bodyLineRenderer;
    private LineRenderer stringLineRenderer;

    //using array to store the points that constructs the balloon shape
    private ArrayList balloonBodyPoints = new ArrayList();    //the points that construct the balloon body
    private ArrayList balloonStringPoints = new ArrayList();  //the points that construct the balloon string

    //set the top point of the balloon be the point that will be affected by rising accelerations,
    //other points will move accordingly due to the constraints between points
    private Point motorPoint;


    //using array to store the links between points
    private ArrayList linksOfBalloon = new ArrayList();


    //the Balloon class will take a Line gameObject and a spawnPosition for the balloon as parameter
    public Balloon(GameObject balloonBodyLine, GameObject balloonStringLine, Vector3 spawnPosition)
    {
        balloonBody = balloonBodyLine;
        balloonString = balloonStringLine;

        //our balloon will have six points for the body (one point is connecting to the string)
        //and 4 points for the string (one point is the body-string connecting point)
        //The spawnPosition will be the position of the top motorPoint of this balloon.
        //      w1=w2
        //       |
        //       v
        //        .
        // h1->  / \
        // h2-> |   |
        // h1->  \ /
        //        |
        //        |
        //        |
        //
        // The invisible Links between points to keep the balloon in shape are designed in a separate PDF file

        //Create Points:
        //define some distance for the balloon shape
        float h1 = 0.125f;  //0.0625f  0.125f  0.10525f
        float h2 = 0.25f;
        float w1 = 0.2105f;
        float strLen = 0.15f;
        //balloon body points
        Point topP = new Point(spawnPosition);
        motorPoint = topP;
        Point leftP1 = new Point(new Vector3(spawnPosition.x - w1, spawnPosition.y - h1));
        Point rightP1 = new Point(new Vector3(spawnPosition.x + w1, spawnPosition.y - h1));
        Point leftP2 = new Point(new Vector3(leftP1.getPosition().x, leftP1.getPosition().y - h2));
        Point rightP2 = new Point(new Vector3(rightP1.getPosition().x, rightP1.getPosition().y - h2));
        Point botP = new Point(new Vector3(spawnPosition.x, spawnPosition.y - 2*h1 - h2));
        //string points
        Point str1 = new Point(new Vector3(spawnPosition.x, botP.getPosition().y - strLen));
        Point str2 = new Point(new Vector3(spawnPosition.x, str1.getPosition().y - strLen));
        Point str3 = new Point(new Vector3(spawnPosition.x, str2.getPosition().y - strLen));
        Point str4 = new Point(new Vector3(spawnPosition.x, str3.getPosition().y - strLen));


        //Add all body points into array list in loop order, first element will connected with the last element later
        //the first element is the connecting point between balloon body and the string
        balloonBodyPoints.Add(botP);
        balloonBodyPoints.Add(leftP2);
        balloonBodyPoints.Add(leftP1);
        balloonBodyPoints.Add(topP);
        balloonBodyPoints.Add(rightP1);
        balloonBodyPoints.Add(rightP2);
        //Add all string points into array list, the first point in the arrayList will be connected to the first point in the
        //balloon body arrayList
        balloonStringPoints.Add(str1);
        balloonStringPoints.Add(str2);
        balloonStringPoints.Add(str3);
        balloonStringPoints.Add(str4);


        //Use Line object to draw the ballon shape:
        Vector3[] bodyPoints = new Vector3[6];
        for(int i = 0; i < balloonBodyPoints.Count; i++)
        {
            bodyPoints[i] = ((Point)balloonBodyPoints[i]).getPosition();
        }
        //we have to connect the last point with the fist point
        //bodyPoints[6] = ((Point)balloonBodyPoints[0]).getPosition();
        bodyLineRenderer = balloonBodyLine.GetComponent<LineRenderer>();
        bodyLineRenderer.positionCount = bodyPoints.Length;  //set the number of vertices this line have, including two end of the line
        bodyLineRenderer.numCornerVertices = 6;  //higher value will smooth the corner
        //draw the balloon body
        bodyLineRenderer.SetPositions(bodyPoints);

        //Use Line object to draw the string shape attached to the balloon:
        Vector3[] stringPoints = new Vector3[5];
        //the first point of the string is the connecting point to the balloon body
        stringPoints[0] = ((Point)balloonBodyPoints[0]).getPosition();
        for (int i = 1; i < stringPoints.Length; i++)
        {
            stringPoints[i] = ((Point)balloonStringPoints[i-1]).getPosition();
        }
        stringLineRenderer = balloonStringLine.GetComponent<LineRenderer>();
        stringLineRenderer.positionCount = stringPoints.Length;  //set the number of vertices this line have, including two end of the line
        stringLineRenderer.numCornerVertices = 6;  //higher value will smooth the corner
        //draw the string
        stringLineRenderer.SetPositions(stringPoints);


        //add constraints as links to keep the hexagon convex
        float crossLength = h2 / Mathf.Sin(30 * Mathf.PI / 180);  //the length between two opposite angles of this hexagon 
        leftP1.addLink(rightP2, crossLength);
        leftP2.addLink(rightP1, crossLength);
        topP.addLink(botP, crossLength);
        botP.addLink(topP, crossLength);
        rightP2.addLink(leftP1, crossLength);
        rightP1.addLink(leftP2, crossLength);
        
        //add constraints as links for the length between each vertices of the balloon (hexagon)
        topP.addLink(leftP1, h2);
        leftP1.addLink(topP, h2);
        leftP1.addLink(leftP2, h2);
        leftP2.addLink(leftP1, h2);
        leftP2.addLink(botP, h2);
        botP.addLink(leftP2, h2);
        botP.addLink(rightP2, h2);
        rightP2.addLink(botP, h2);
        rightP2.addLink(rightP1, h2);
        rightP1.addLink(rightP2, h2);
        rightP1.addLink(topP, h2);
        topP.addLink(rightP1, h2);


        //add constraints as Links for the points of this balloon string (constraints are shown in the separate PDF file)
        float str_balloonAngle = 120 * Mathf.PI / 180;
        //str1.addLink(leftP2, CollisionCalculation.cosineFormula(str1.getPosition(), botP.getPosition(), leftP2.getPosition(), str_balloonAngle));
        botP.addLink(str1, strLen);
        str1.addLink(botP, strLen);
        str1.addLink(str2, strLen);
        str2.addLink(str1, strLen);
        str2.addLink(str3, strLen);
        str3.addLink(str2, strLen);
        str3.addLink(str4, strLen);
        str4.addLink(str3, strLen);

        //botP.addLink(str2, 2*strLen);
        //str2.addLink(botP, 2*strLen);

        
        str2.addLink(leftP2, CollisionCalculation.cosineFormula(str2.getPosition(), botP.getPosition(), leftP2.getPosition(), str_balloonAngle));
        leftP2.addLink(str2, CollisionCalculation.cosineFormula(str2.getPosition(), botP.getPosition(), leftP2.getPosition(), str_balloonAngle));
        
        str2.addLink(rightP2, CollisionCalculation.cosineFormula(str2.getPosition(), botP.getPosition(), rightP2.getPosition(), str_balloonAngle));
        rightP2.addLink(str2, CollisionCalculation.cosineFormula(str2.getPosition(), botP.getPosition(), rightP2.getPosition(), str_balloonAngle));
        
    }

    //this function will update the position of every point belongs to this balloon using Verlet integration in Point class
    //and then update the linerenderer
    public void updatePosition(float deltaTime, Vector3 acceleration)
    {
        //update the position of the motor point, since this point is in a constraint system, changing the position
        //of this point will trigger the change of all points in the constraints system
        motorPoint.setAcceleration(acceleration);
        motorPoint.updatePosition(deltaTime);


        int boundOfIteration = 30;  //set a bound to the iteration for solving the constraints of this balloon (Verlet-integration)

        for(int i = 0; i < boundOfIteration; i++)
        {
            //solve the constraints point by point for all points of the balloon body
            foreach(Point p in balloonBodyPoints)
            {
                p.solveConstraints();

            }

            //solve the constraints point by point for all points of the balloon string
            foreach (Point p in balloonStringPoints)
            {
                p.solveConstraints();
            }
        }


        //update each balloon body Point's position
        foreach (Point p in balloonBodyPoints)
        {
            p.updatePosition(deltaTime);
        }

        //update each balloon string Point's position
        foreach (Point p in balloonStringPoints)
        {
            p.updatePosition(deltaTime);
        }
    }


    //this function will update the line renderers the this balloon based on the current position of points
    //i.e. this function will draw the current balloon on the scene
    public void updateRenderer()
    {
        //Use Line object to draw the ballon shape:
        Vector3[] bodyPoints = new Vector3[6];
        for (int i = 0; i < balloonBodyPoints.Count; i++)
        {
            bodyPoints[i] = ((Point)balloonBodyPoints[i]).getPosition();
        }

        bodyLineRenderer.positionCount = bodyPoints.Length;  //set the number of vertices this line have, including two end of the line
        //draw the balloon body
        bodyLineRenderer.SetPositions(bodyPoints);

        //Use Line object to draw the string shape attached to the balloon:
        Vector3[] stringPoints = new Vector3[5];
        //the first point of the string is the connecting point to the balloon body
        stringPoints[0] = ((Point)balloonBodyPoints[0]).getPosition();
        for (int i = 1; i < stringPoints.Length; i++)
        {
            stringPoints[i] = ((Point)balloonStringPoints[i - 1]).getPosition();
        }
        stringLineRenderer.positionCount = stringPoints.Length;  //set the number of vertices this line have, including two end of the line
        //draw the string
        stringLineRenderer.SetPositions(stringPoints);
    }


    //getter function for the top point of the balloon
    public Point getTopPoint()
    {
        return motorPoint;
    }


    //getter function for the position of the top motor point of this balloon
    public Vector3 getTopPosition()
    {
        return motorPoint.getPosition();
    }

    //getter function for the ballon body Line game objects
    public GameObject getBodyLine()
    {
        return this.balloonBody;
    }

    //getter function for the balloon string Line game objects
    public GameObject getStringLine()
    {
        return this.balloonString;
    }

    //getter function for the balloon body point positions (add the start point at the end of return array to make it a loop)
    public Vector3[] getBodyLinePositions()
    {
        Vector3[] positions = new Vector3[this.balloonBodyPoints.Count+1];
        for(int i = 0; i < balloonBodyPoints.Count; i++)
        {
            positions[i] = ((Point) balloonBodyPoints[i]).getPosition();
        }

        //add the start point at the end of return array to make it a loop of line segment
        positions[positions.Length-1] = ((Point)balloonBodyPoints[0]).getPosition();

        return positions;
    }


    //getter function for the balloon string point positions (add the connecting point to the return array)
    public Vector3[] getStringLinePositions()
    {
        Vector3[] positions = new Vector3[this.balloonStringPoints.Count + 1];

        //add the connecting point to the return array to make it as a part of line segment
        positions[0] = ((Point)balloonBodyPoints[0]).getPosition();

        for (int i = 1; i < balloonStringPoints.Count + 1; i++)
        {
            positions[i] = ((Point)balloonStringPoints[i-1]).getPosition();
        }

        return positions;
    }

    //getter function for the body Point array list
    public ArrayList getBodyPoints()
    {
        return this.balloonBodyPoints;
    }

    //getter function for the string Point array list
    public ArrayList getStringPoints()
    {
        return this.balloonStringPoints;
    }
}
