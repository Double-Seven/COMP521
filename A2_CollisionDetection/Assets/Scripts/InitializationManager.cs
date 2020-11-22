using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Author: ZiQi Li, for Comp521 A2, McGill University
public class InitializationManager : MonoBehaviour
{

    public GameObject terrainLinePrefab;   // prefab for the line used to draw terrain
    public GameObject terrainPointPrefab;  // prefab for the point used to draw terrain outline
    public GameObject waterLinePrefab;
    public GameObject collsionLinePrefab;  // prefab for the invisible line used for terrain collsion




    //Variables for Terrain generation:
    //set the random range for the y-value of points
    private float randomMin = -0.07f;
    private float randomMax = 0.07f;
    private int mapLength = 18;  //the length of map
    //the horizontal distance between each point for the terrain outline
    private float pointLap = 0.15f;
    //the y-value for ground level
    private float groundLevel = -2f;

    //store the collision line for collosion detection
    private GameObject terrainCollisionLine;
    private GameObject waterCollisionLine;


    // Awake is called before the Start() in other scripts, so used for initialization
    void Awake()
    {
        //initialize the outline points of terrain
        ArrayList terrainPoints = generateTerrainPoints(terrainPointPrefab, mapLength, groundLevel);

        //generate line for the terrain
        generateTerrainLine(terrainLinePrefab, terrainPoints, 40);

        //the water start point and end point of the terrain will be at the 37.8% and 62.2% index of all points
        ArrayList waterPoints = generateWaterPoints(terrainPointPrefab, terrainPoints[(int)(0.378 * mapLength / pointLap - 1)] as GameObject, terrainPoints[(int)(0.622 * mapLength / pointLap)] as GameObject);

        //generate line for water using the same function that we generate terrain line
        generateTerrainLine(waterLinePrefab, waterPoints, 40);

        //generate collision lines
        terrainCollisionLine = generateCollisionLineOnTerrain(collsionLinePrefab, terrainPoints);

        //generate water collision line
        waterCollisionLine = generateCollisionLineOnWater(collsionLinePrefab, terrainPoints);



    }

    // Update is called once per frame
    void Update()
    {

    }

    //function that generates the outline of terrain using terrainPointPrefab and return an array containing those points
    //Note that this function will add some randomness (Noise) to the shape while keeping the general shape of game terrain
    ArrayList generateTerrainPoints(GameObject pointPrefab, int mapLength, float horizonLineLevel)
    {
        ArrayList points = new ArrayList();

        //total number of point we have to generate
        int numPoints = (int)(mapLength / pointLap);

        //let's define the general shape of whole terrain relative to the mapLength (%):
        //
        //            -^-                     -^- 
        //           /   \                   /   \
        //          /     \                 /     \  
        //---------/       *               *       \----------
        //                  \             /  
        //                   \___________/   
        //   24%   8%     8%      20%      8%    8%     24%
        //
        //"*" "*", the water start point and end point of the terrain will be at the 37.8% and 62.2% index of all points
        float groundPortion = 0.24f;
        float moutainSlopPortion = 0.08f;
        float waterPortion = 0.2f;

        float upslopeIncrement = 0.3f;
        float downslopeIncrement = -0.45f;


        float yPosition = horizonLineLevel;  //set it initially to the ground level
        Vector3 position;
        //Vector3 rotation = new Vector3(0,0,0);
        Quaternion rotation = new Quaternion(0, 0, 0, 1);

        //At different portion of length, we will add an slope value to y-values of the
        //points in order to keep the general shape be the same as shown above
        for (int i = 0; i < numPoints; i++)
        {
            //left ground part
            if (i <= groundPortion * numPoints)
            {
                //add some randomness to the yPosition
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //first mountain upslope
            else if (i <= (groundPortion + moutainSlopPortion) * numPoints)
            {
                yPosition += upslopeIncrement;
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //first mountain downslope
            else if (i <= (groundPortion + moutainSlopPortion + moutainSlopPortion) * numPoints)
            {
                yPosition += downslopeIncrement;
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //water part
            else if (i <= (groundPortion + moutainSlopPortion + moutainSlopPortion + waterPortion) * numPoints)
            {
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //second mountain upslope
            else if (i <= (groundPortion + moutainSlopPortion + moutainSlopPortion + waterPortion + moutainSlopPortion) * numPoints)
            {
                yPosition -= downslopeIncrement;
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //second mountain downslope
            else if (i <= (groundPortion + moutainSlopPortion + moutainSlopPortion + waterPortion + moutainSlopPortion + moutainSlopPortion) * numPoints)
            {
                yPosition -= upslopeIncrement;
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
            //right ground
            else
            {
                position = new Vector3(i * pointLap, yPosition + Random.Range(randomMin, randomMax));
                points.Add(Instantiate(pointPrefab, position, rotation));
            }
        }

        return points;
    }


    //function that generates points for the water line using terrainPointPrefab, two points where the water line starts and ends
    //and return an array containing those points
    //Note that this function will add some randomness (Noise) to the shape
    ArrayList generateWaterPoints(GameObject pointPrefab, GameObject startPoint, GameObject endPoint)
    {
        ArrayList points = new ArrayList();
        //add the start point into the water points array
        points.Add(startPoint);

        //total number of point we have to generate
        int numPoints = (int)((endPoint.transform.position.x - startPoint.transform.position.x) / pointLap);

        Vector3 position;
        Quaternion rotation = new Quaternion(0, 0, 0, 1);
        float yPosition = startPoint.transform.position.y;  //set it initially to the water line level

        //create water points
        for (int i = 1; i < numPoints; i++)
        {
            position = new Vector3(startPoint.transform.position.x + i * pointLap, yPosition + Random.Range(randomMin, randomMax));
            points.Add(Instantiate(pointPrefab, position, rotation));
        }

        //add the end point into the water points array
        points.Add(endPoint);

        return points;
    }



    //this function will be a smooth function for Perlin Noise
    //using function 3t^2-2t^3.
    //Note: the range of t should be [0,1] and the interval between a and b is 1
    //the return value will also be in range [0,1]. So we have to rescale when calling this function
    float perlinSmooth(float a, float b, float t)
    {
        float tRemaped = t * t * (3 - 2 * t);
        return Mathf.Lerp(a, b, tRemaped);
    }


    //this function will generate terrain line using Perlin smooth function based on the terrains points generated
    //numOft is the number of intermediate points between two outline points (we will draw straight line between intermediate
    //points, more we increase the numberOft, more smooth line we will get)
    void generateTerrainLine(GameObject linePrefab, ArrayList pointsList, int numOft)
    {
        GameObject pointA;
        GameObject pointB;
        float y_A; //the y position of pointA
        float y_B;
        float interLap = pointLap / numOft;  //the lap between each t

        float previousX = 0f; //store value of x during iteration
        float nextX = 0f;
        float t = 0f;  // t = x - xMin (xMin is the x of pointA which is the outline point)
        float smoothYofT = 0f; //the y-value corresponds to T after applying smooth function

        GameObject line;
        LineRenderer lineRenderer;
        //iterate the points list and add intermediate points using Perlin smooth function, then draw lines between those points
        for (int i = 0; i < pointsList.Count - 1; i++)
        {
            pointA = (GameObject)pointsList[i];
            pointB = (GameObject)pointsList[i + 1];
            y_A = pointA.transform.position.y;
            y_B = pointB.transform.position.y;
            //draw lines between intermediate points
            for (int j = 0; j < numOft; j++)
            {
                //connect the point A with the first intermediate point
                if (j == 0)
                {
                    previousX = pointA.transform.position.x;
                    nextX = previousX + interLap;
                    t = nextX - pointA.transform.position.x;
                    smoothYofT = perlinSmooth(y_A, y_B, t * (1 / pointLap));  //* (1/pointLap) since we have to map the scale to [0,1] for t
                    //create a line object and using it to align the two points
                    line = Instantiate(linePrefab, new Vector3(0f, 0f, 0f), this.transform.rotation);
                    lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, pointA.transform.position);
                    lineRenderer.SetPosition(1, new Vector3(nextX, smoothYofT));

                }
                //connect the last intermediate point with point B
                else if (j == numOft - 1)
                {
                    //update the T variables
                    previousX = nextX;
                    nextX = pointB.transform.position.x;
                    //create a line object and using it to align the two points
                    line = Instantiate(linePrefab, new Vector3(0f, 0f, 0f), this.transform.rotation);
                    lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, new Vector3(previousX, smoothYofT));
                    //smoothYofT = perlinSmooth(y_A, y_B, nextT * (1 / pointLap));
                    lineRenderer.SetPosition(1, pointB.transform.position);
                }
                else  //connect two intermediate points 
                {
                    //update the T variables
                    previousX = nextX;
                    nextX = previousX + interLap;
                    t = nextX - pointA.transform.position.x;
                    //create a line object and using it to align the two points
                    line = Instantiate(linePrefab, new Vector3(0f, 0f, 0f), this.transform.rotation);
                    lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, new Vector3(previousX, smoothYofT));
                    smoothYofT = perlinSmooth(y_A, y_B, t * (1 / pointLap));
                    lineRenderer.SetPosition(1, new Vector3(nextX, smoothYofT));
                }
            }


        }
    }



    //this function will draw collision detection line on the terrain, and return that line
    GameObject generateCollisionLineOnTerrain(GameObject linePrefab, ArrayList outlinePoints)
    {
        //draw the collsion line base on the general terrain shape not the exact rendered shape (ground line and mountain line)
        //for simplififying the collision detection complecity

        GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        Vector3[] lineVertices = new Vector3[8];  //we need 8 vertices to draw the terrain line in one line
        lineRenderer.positionCount = 8;  //set the number of vertices this line have, including two end of the line

        //setting the vertices of the line
        //Left ground start and end:
        lineVertices[0] = new Vector3(0f, groundLevel + randomMin + 0.05f, 0.0f);
        lineVertices[1] = new Vector3(((GameObject)outlinePoints[(int)(0.2416 * mapLength / pointLap)]).transform.position.x, groundLevel + randomMin + 0.05f, 0.0f);

        //Left mountain top
        lineVertices[2] = new Vector3(((GameObject)outlinePoints[(int)(0.325 * mapLength / pointLap)]).transform.position.x, groundLevel + 10 * 0.3f, 0.0f);

        //Left mountain end of downslope
        lineVertices[3] = new Vector3(((GameObject)outlinePoints[(int)(0.4 * mapLength / pointLap)]).transform.position.x, groundLevel + 10 * 0.3f + 9 * -0.45f, 0.0f);

        //Right mountain start of upslope
        lineVertices[4] = new Vector3(((GameObject)outlinePoints[(int)(0.6 * mapLength / pointLap)]).transform.position.x, groundLevel + 10 * 0.3f + 9 * -0.45f, 0.0f);

        //Right mountain top
        lineVertices[5] = new Vector3(((GameObject)outlinePoints[(int)(0.675 * mapLength / pointLap)]).transform.position.x, groundLevel + 10 * 0.3f, 0.0f);

        //Right mountain end of down slope
        lineVertices[6] = new Vector3(((GameObject)outlinePoints[(int)(0.7583 * mapLength / pointLap)]).transform.position.x, groundLevel, 0.0f);

        //End point
        lineVertices[7] = new Vector3(mapLength, -2.0f, 0.0f);

        lineRenderer.SetPositions(lineVertices);  //draw the line

        return line;
    }

    //this function will draw collision detection line on the water, and return that line
    GameObject generateCollisionLineOnWater(GameObject linePrefab, ArrayList outlinePoints)
    {
        //draw the collsion line base on the general water shape not the exact rendered shape
        //for simplififying the collision detection complecity

        GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1));
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        Vector3[] lineVertices = new Vector3[2];  //we need 2 vertices to draw the terrain line in one line
        lineRenderer.positionCount = 2;  //set the number of vertices this line have, including two end of the line

        //water line start point
        lineVertices[0] = new Vector3(((GameObject)outlinePoints[(int)(0.37 * mapLength / pointLap)]).transform.position.x, groundLevel + 10 * 0.3f + 6 * -0.45f, 0f);
        //water line end point
        lineVertices[1] = new Vector3(((GameObject)outlinePoints[(int)(0.63 * mapLength / pointLap) - 1]).transform.position.x, groundLevel + 10 * 0.3f + 6 * -0.45f, 0f);

        lineRenderer.SetPositions(lineVertices);  //draw the line

        return line;
    }


    //getter function for terrainCollisionLine
    public GameObject getTerrainCollisionLine()
    {
        return this.terrainCollisionLine;
    }

    //getter function for waterCollisionLine
    public GameObject getWaterCollisionLine()
    {
        return this.waterCollisionLine;
    }
}