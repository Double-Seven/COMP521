using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: ZiQi Li
public class GameInitializer : MonoBehaviour
{
    // Public variable
    // Prefabs
    [Header("Prefabs")]
    public GameObject crateGameObjectPrefab;
    public GameObject rockGameObjectPrefab;
    public GameObject slimeGameObjectPrefab;
    // Grid system
    [Header("Grid Map")]
    public GameObject gridOriginGameObject;
    public float x_width = 0;
    public float z_width = 0;
    public int x_numCells = 0;
    public int z_numCells = 0;

    // ----------------------------

    // Private vairalbe
    private GridMap gridMap;
    private float groundLevel = 0;
    private int num_obstacle_x = 10;  // number of obstacles preventing player can directly go to the cave from entrance

    private ArrayList obstacleList;
    private ArrayList slimeList = new ArrayList();


    // ----------------------------




    // Start is called before the first frame update
    void Awake()
    {
        // construct a grid map for the map
        gridMap = new GridMap(gridOriginGameObject.transform.position, this.x_width, this.z_width, this.x_numCells, this.z_numCells);

        // generate obstacles
        this.obstacleList = generateRandomObstacles(this.gridMap, crateGameObjectPrefab, rockGameObjectPrefab);
        this.slimeList = generateSlimes(11, 40, 10, 40, this.slimeGameObjectPrefab);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Function to randomly generate obstacles on the map using grid map, and return the list of obstacle game object
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="cratePrefab"></param>
    /// <param name="rockPrefab"></param>
    /// <returns></returns>
    ArrayList generateRandomObstacles(GridMap grid, GameObject cratePrefab, GameObject rockPrefab)
    {
        ArrayList obsList = new ArrayList();
        int rand = 0;
        float rand1 = 0;
        float rand2 = 0;
        int zIndex = 2; // used for preventing player can directly go to the cave from entrance
        Vector3 pos;
        float probability_generation = 0.15f;  // probability of generate an obstacle on the map

        // generate obstacles preventing player can directly go to the cave from entrance
        for(int i = 0; i < this.num_obstacle_x; i++)
        {
            rand = Random.Range(zIndex, zIndex+2);  // generate obstacles between a cell index range of z direction (between 2 and 3 of index of cell in z direction )
            // get the cell at index [i][rand]
            pos = new Vector3(grid.getCellPosition(i, rand).x, groundLevel, grid.getCellPosition(i, rand).y);

            rand1 = Random.Range(0,1.0f);  // randomly choose crate or rock to generate
            if(rand1 < 0.1f)
            {
                // generate crate
                obsList.Add(Instantiate(crateGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
            }
            else
            {
                // generate rock
                obsList.Add(Instantiate(rockGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
            }
        }


        // generate obstacles randomly in the rest of regions (small rectangle region near the entrance)
        for (int i = 1; i < this.x_numCells; i++)
        {
            for (int k = 0; k < zIndex; k++)
            {
                rand2 = Random.Range(0, 1.0f);

                // generate an obstacle if the rand number < probability_generation
                if (rand2 < probability_generation - 0.08f)
                {
                    rand1 = Random.Range(0, 1.0f);  // randomly choose crate or rock to generate
                    if (rand1 < 0.3f)
                    {
                        // get the cell at index [i][k]
                        pos = new Vector3(grid.getCellPosition(i, k).x, groundLevel, grid.getCellPosition(i, k).y);
                        // generate crate
                        obsList.Add(Instantiate(crateGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
                    }
                    else
                    {
                        // get the cell at index [i][k]
                        pos = new Vector3(grid.getCellPosition(i, k).x, groundLevel, grid.getCellPosition(i, k).y);
                        // generate rock
                        obsList.Add(Instantiate(rockGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
                    }
                }

            }
        }


        // generate obstacles randomly in the rest of regions (large rectangle region)
        for (int i = 3; i < this.x_numCells-1; i++)
        {
            for(int k = zIndex + 2; k < this.z_numCells; k++)
            {
                rand2 = Random.Range(0, 1.0f);

                // generate an obstacle if the rand number < probability_generation
                if (rand2 < probability_generation - 0.05f)
                {
                    rand1 = Random.Range(0, 1.0f);  // randomly choose crate or rock to generate
                    if (rand1 < 0.3f)
                    {
                        // get the cell at index [i][k]
                        pos = new Vector3(grid.getCellPosition(i, k).x, groundLevel, grid.getCellPosition(i, k).y);
                        // generate crate
                        obsList.Add(Instantiate(crateGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
                    }
                    else
                    {
                        // get the cell at index [i][k]
                        pos = new Vector3(grid.getCellPosition(i, k).x, groundLevel, grid.getCellPosition(i, k).y);
                        // generate rock
                        obsList.Add(Instantiate(rockGameObjectPrefab, pos, this.transform.rotation).transform.GetChild(0).gameObject);
                    }
                }

            }
        }


        return obsList;
    }


    /// <summary>
    /// Function to randomly generate slimes on the free space of map
    /// </summary>
    /// <param name="minx"></param>
    /// <param name="maxx"></param>
    /// <param name="minz"></param>
    /// <param name="maxz"></param>
    /// <param name="slimePrefab"></param>
    /// <returns></returns>
    ArrayList generateSlimes(float minx, float maxx, float minz, float maxz, GameObject slimePrefab)
    {
        int maxNumOfSlimes = 6;  // num of slimes to generate
        ArrayList slimeList = new ArrayList();

        // generate a point inside the rectangle area (test collision using SphereCast (ray cast with radius in 3D space)
        // the origin of the ray is at 3m above the ground level, the direction is toward the ground and the radius is a little bit larger than the radius of an agent
        Vector3 rayOrigin = new Vector3(UnityEngine.Random.Range(minx, maxx), 3, UnityEngine.Random.Range(minz, maxz));
        Vector3 direction = Vector3.down;
        float radius = slimePrefab.transform.localScale.x + 0.5f;
        float maxDistance = 3f - 1;
        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = rayOrigin;
        ray.direction = direction;
        // keep picking new rand point inside the picked floor if the previous rand point will cause collision with other colliders
        while (maxNumOfSlimes > 0)
        {
            if(Physics.SphereCast(rayOrigin, radius, direction, out hit, maxDistance))
            {
                rayOrigin = new Vector3(UnityEngine.Random.Range(minx, maxx), 3, UnityEngine.Random.Range(minz, maxz));

                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 20f);
            }
            else
            {
                rayOrigin.y = -0.003f;
                slimeList.Add(Instantiate(slimePrefab, rayOrigin, this.transform.rotation));
                maxNumOfSlimes--;

                rayOrigin = new Vector3(UnityEngine.Random.Range(minx, maxx), 3, UnityEngine.Random.Range(minz, maxz));
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 20f);
                //Debug.Log(hit.transform.name);
            }
        }

        return slimeList;
    }


    /// <summary>
    /// Function to return the current obstacle list on the map
    /// </summary>
    /// <returns></returns>
    public ArrayList getObstacleList()
    {
        return this.obstacleList;
    }

    /// <summary>
    /// Function to return the current slime list on the map
    /// </summary>
    /// <returns></returns>
    public ArrayList getSlimeList()
    {
        return this.slimeList;
    }
}
