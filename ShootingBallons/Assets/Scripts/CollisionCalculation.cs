using System;
using UnityEngine;


//Author: ZiQi Li, for Comp521 A2, McGill University
//this class will contain some functions using for our own collision detection and resolution
public class CollisionCalculation
{
    public CollisionCalculation()
    {
    }


    //function to calculate the distance between two point (x1,y1) and (x2,y2)
    public static float getDistance(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x2 - x1), 2) + Mathf.Pow(Mathf.Abs(y2 - y1), 2));
    }


    //detect if a point (px,py) is on the line ((x1,y1),(x2,y2))
    public static bool isPointOnLine(float px, float py, float x1, float y1, float x2, float y2)
    {
        // get distance from the point to the two ends of the line
        float d1 = getDistance(px, py, x1, y1);
        float d2 = getDistance(px, py, x2, y2);
        
        // get the length of the line
        float lineLength = getDistance(x1, y1, x2, y2);

        //add a range for the acceptable error since we are using float to compare
        float epsilon = 0.01f;   

        // if the sum of two distances are equal to the line's length, the point is on the line
        if (d1 + d2 >= lineLength - epsilon && d1 + d2 <= lineLength + epsilon)
        {
            return true;
        }

        //otherwise return false
        return false;
    }

    //detect if a point is in a circle with center point (cx,cy) with a given radius
    public static bool isPointInCircle(float px, float py, float cx, float cy, float radius)
    {

        // get distance between the point and the circle's center
        float distance = getDistance(cx, cy, px, py);

        // if the distance is less than the circle's radius the point is inside the circle
        if (distance <= radius)
        {
            return true;
        }

        //otherwise return false
        return false;
    }

    // detect if a circle with center point (cx,cy) and a given radius is colliding with a given line ((x1,y1),(x2,y2))
    //The idea is the modified version of collision between two circle we saw in class
    public static bool isCircleCollidesLine(float x1, float y1, float x2, float y2, float cx, float cy, float radius)
    {
        // if either end of line is inside the circle, return true
        bool end1 = isPointInCircle(x1, y1, cx, cy, radius);
        bool end2 = isPointInCircle(x2, y2, cx, cy, radius);
        if (end1 || end2) return true;

        // get length of the line
        float lineLength = getDistance(x1, y1, x2, y2);

        // get closest point on the line to the circle's center using the following formula
        // same as getting the dot product of the line and circle
        float dot = (((cx - x1) * (x2 - x1)) + ((cy - y1) * (y2 - y1))) / Mathf.Pow(lineLength, 2);
        // find the closest point on the line
        float closestX = x1 + (dot * (x2 - x1));
        float closestY = y1 + (dot * (y2 - y1));

        // if the closest point on the line from the circle is not on the line, return false
        bool onLine = isPointOnLine(closestX, closestY, x1, y1, x2, y2);
        if (!onLine) return false;

        // get distance to closest point from the center of circle
        float distanceToClosestPoint = getDistance(closestX, closestY, cx, cy);

        //if the distance to closest point from the center of circle <= the radius of circle,
        //the circle is collided with the line, return true;
        if (distanceToClosestPoint <= radius)
        {
            return true;
        }
        return false;
    }

    //function that return the unit normal of a line given by two points
    public static Vector2 getNormal(float x1, float y1, float x2, float y2)
    {
        float dx = x2 - x1;
        float dy = y2 - y1;
        //if we define dx=x2-x1 and dy=y2-y1, then the normals are (-dy, dx), and divide it by the vector length to get unit normal vector
        return new Vector2(-dy, dx) / Mathf.Sqrt(Mathf.Pow(dx,2) + Mathf.Pow(dy, 2));
    }

    //function to calculate the length between a and c given the vector (a,b), (b,c) and angle(in rad) abc as parameters
    public static float cosineFormula(Vector3 a, Vector3 b, Vector3 c, float angle)
    {
        float restingDistance;
        //the length lenA is the length between a and b, the length lenB is the length between b and c
        float lenA = getDistance(a.x, a.y, b.x, b.y);
        float lenB = getDistance(b.x, b.y, c.x, c.y);
        //apply the cosine formula
        restingDistance = Mathf.Sqrt(lenA * lenA + lenB * lenB - 2 * lenA * lenB * Mathf.Cos(angle));

        return restingDistance;
    }

}
