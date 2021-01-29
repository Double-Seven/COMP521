using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: ZiQi Li
public class InitializationManager : MonoBehaviour
{
    [Header("Map boundaries")]
    public float leftBoundary;
    public float rightBoundary;
    public float canyonBoundary;

    [Header("Path generator parameters")]
    public GameObject initialPathTile;
    public GameObject pathTilePrefab;
    public GameObject ammoPrefab;
    public int numberOfAmmo = 10;
    //keep track of the previous path tile type, 0:left tile, 1:forward tile, 2: right tile
    //the tile type positions are the children of Path prefab
    private ArrayList pathTiles = new ArrayList();
    private int totalNumberOfAmmo;

    [Header("Maze generator parameters")]
    //array to store the platform of maze, initialized in the inspector of Unity
    public GameObject[] mazePlatforms;
    //number of row and column of the maze
    public int numRow;
    public int numCol;
    //start and goal platform index in the platform array
    public int startIndex;
    public int goalIndex;
    //an array that store the solution of the maze
    private ArrayList mazeSolution = new ArrayList();


    //define a enum type for direction (NSWE)
    enum PlatformDirection
    {
        north = 0,
        south = 1,
        west = 2,
        east = 3
    }

    /*----------------------------------------------------*/
    /*define a data type "platform" for the maze generator*/
    private class Platform
    {
        public Platform[] neighbours = new Platform[4]; //using index with enum type PlatformDirection, defaultly elements are null
        public bool isVisited;
        public GameObject platformGameObject;

        public Platform(GameObject platform)
        {
            this.platformGameObject = platform;
            isVisited = false;

        }

        //add a neighbour to this platform based on the direction of the neighbour
        public void addNeighbour(Platform neighbourPlatform, PlatformDirection direction)
        {
            this.neighbours[(int) direction] = neighbourPlatform;
        }

        //destroy the wall between two adjacent platform
        public void destroyWall(Platform p2)
        {
            if(object.ReferenceEquals(this.neighbours[(int) PlatformDirection.north],p2))
            {
                //set the north wall of p1 to invisible
                this.platformGameObject.transform.GetChild((int)PlatformDirection.north).gameObject.SetActive(false);
                //set the south wall of p2 to invisible
                p2.platformGameObject.transform.GetChild((int)PlatformDirection.south).gameObject.SetActive(false);
            }
            else if(object.ReferenceEquals(this.neighbours[(int) PlatformDirection.south], p2))
            {
                //set the south wall of p1 to invisible
                this.platformGameObject.transform.GetChild((int)PlatformDirection.south).gameObject.SetActive(false);
                //set the north wall of p2 to invisible
                p2.platformGameObject.transform.GetChild((int)PlatformDirection.north).gameObject.SetActive(false);
            }
            else if (object.ReferenceEquals(this.neighbours[(int)PlatformDirection.west], p2))
            {
                //set the west wall of p1 to invisible
                this.platformGameObject.transform.GetChild((int)PlatformDirection.west).gameObject.SetActive(false);
                //set the east wall of p2 to invisible
                p2.platformGameObject.transform.GetChild((int)PlatformDirection.east).gameObject.SetActive(false);
            }
            else if (object.ReferenceEquals(this.neighbours[(int)PlatformDirection.east], p2))
            {
                //set the east wall of p1 to invisible
                this.platformGameObject.transform.GetChild((int)PlatformDirection.east).gameObject.SetActive(false);
                //set the west wall of p2 to invisible
                p2.platformGameObject.transform.GetChild((int)PlatformDirection.west).gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("These 2 platforms are not adjacent.");
            }

        }

        //determine if this platform has any unvisited neighbour
        public bool hasUnvisitedNeighbour()
        {
            bool result = true;
            foreach(Platform pl in this.neighbours)
            {
                //ignore if the neighbour is null
                if(pl == null)
                {
                    continue;
                }
                else
                {
                    //combine with the result, if any neighbour.isVisited == false, result will be false at the end
                    result = pl.isVisited && result;
                }
            }

            //return the opposite of the result, since the result represents "hasUnvisitedNeighbour"
            return !result;
        }

        //choose randomly an unvisited neighbour of this platform
        public Platform chooseRandomlyUnvisitedNeighbour()
        {
            ArrayList unvisitedPlatforms = new ArrayList();
            //iterate through the neighbour array and eliminate null value,
            //put the non-null and unvisited platform into an array
            foreach (Platform pl in this.neighbours)
            {
                //ignore if the neighbour is null
                if (pl == null)
                {
                    continue;
                }
                else
                {
                    //if a neighbour is unvisited, add it to the unvisited list
                    if(pl.isVisited == false)
                    {
                        unvisitedPlatforms.Add(pl);
                    }
                }
            }

            //randomly find a platform from the unvisitedPlatforms list and return it
            int randomIndex = Random.Range(0, unvisitedPlatforms.Count);
            return (Platform) unvisitedPlatforms[randomIndex];

        }

        //convert an ArrayList containing Platform type to an ArrayList containing GameObject type of platform
        public static ArrayList convertToGameObjects(ArrayList platformList)
        {
            ArrayList gameObjectList = new ArrayList();
            foreach(Platform pl in platformList)
            {
                gameObjectList.Add(pl.platformGameObject);
            }
            return gameObjectList;
        }

    }
    /*----------------------------------------------------*/

    /*----------------------------------------------------*/
    /*define a data type "tile" for the path generator*/
    private class Tile
    {
        public GameObject thisTile;
        public int thisTileType;

        public Tile(GameObject tile)
        {
            this.thisTile = tile;
            thisTileType = 1;  //range is 0,1,2 so we set it initially to 1 (forward)
        }
    }
    /*----------------------------------------------------*/




    // Start is called before the first frame update
    void Start()
    {
        totalNumberOfAmmo = numberOfAmmo;

        //generate the path leading to the maze
        generatePath(pathTilePrefab, initialPathTile, leftBoundary, rightBoundary, canyonBoundary);

        //generate ammo randomly
        generateAmmo(ammoPrefab, pathTiles);

        //add platform into the platforms array
        GameObject parentPlatform = GameObject.FindGameObjectWithTag("Platforms");
        for(int i = 0; i < numRow*numCol; i++)
        {
            //get the child platforms from the tagged parentPlatform
            mazePlatforms[i] = parentPlatform.transform.GetChild(i).gameObject;
        }

        //initialized the platforms in an Arraylist containing Platform type platforms
        ArrayList platformsList = initializeMaze(mazePlatforms);

        //string test = ((Platform)platformsList[24]).neighbours[0].platformGameObject.name;
        
        //generate the random maze, the return value is the solution of this maze. And update the private field.
        mazeSolution = Platform.convertToGameObjects(generateMaze(platformsList));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        

    //generate path based on modified version of DFS
    void generatePath(GameObject tilePrefab, GameObject initialTile, float leftBoundary, float rightBoundary, float forwardBoundary)
    {
        //use stack
        Stack tiles = new Stack();
        //create tile object for the initial tile
        Tile currentTile = new Tile(initialTile);
        currentTile.thisTileType = 1;  //the initial tile is forward type

        //create the first tile
        GameObject currentTileObject = Instantiate(tilePrefab, currentTile.thisTile.transform.GetChild(1).position, currentTile.thisTile.transform.GetChild(1).rotation);
        currentTile = new Tile(currentTileObject);
        currentTile.thisTileType = 1;

        //push the initial tile into stack
        tiles.Push(currentTile);
        //put the current tile into array list for ammo generation needs
        pathTiles.Add(currentTile.thisTile);

        //randomly define the position of the next path tile (left/forward/right)
        // [0,1):left tile, [1,2):forward tile, [2,3): right tile
        // float -> int will round down automatically, so 0:left tile, 1:forward tile, 2: right tile
        int randomIndex;
        
        //keeping generating path tiles until reaching the forward boundary
        while (currentTile.thisTile.transform.position.z < forwardBoundary)
        {
            //when the previous tile is "left" type we can't generate the next tile at the right,
            //since it will overlap with the previous tile, vice-versa. So, modify the random number range for each situation.
            //When the next tile may be out of map, we should modify the random number.
            if (currentTile.thisTile.transform.position.x >= leftBoundary && currentTile.thisTile.transform.position.x <= rightBoundary)  //the current tile is in the range
            {

                if (currentTile.thisTileType == 0) //when the previous tile is "left" type
                {
                    randomIndex = Random.Range(0, 2);
                }
                else if (currentTile.thisTileType == 2)  //when the previous tile is "right" type
                {
                    randomIndex = Random.Range(1, 3);
                }
                else
                {
                    randomIndex = Random.Range(0, 3);
                }

                //generate 2 tiles at a time for better path shape
                for(int i = 0; i < 2; i++)
                {
                    //create the tile object for the current tile
                    currentTileObject = Instantiate(tilePrefab, currentTile.thisTile.transform.GetChild(randomIndex).position, currentTile.thisTile.transform.GetChild(randomIndex).rotation);
                    currentTile = new Tile(currentTileObject);
                    currentTile.thisTileType = randomIndex;

                    //push the current tile into stack
                    tiles.Push(currentTile);
                    //put the current tile into array list for ammo generation needs
                    pathTiles.Add(currentTile.thisTile);
                }


            }
            else
            {
                currentTile.thisTile.SetActive(false);
                //back track to the not out of boundary tile
                tiles.Pop();
                currentTile = (Tile) tiles.Peek();
                pathTiles.RemoveAt(pathTiles.Count-1);
            }

        }

        //if the last tile is not close to the middle of maze, generate a horizontal path leading to there
        GameObject lastTile = (GameObject) pathTiles[pathTiles.Count - 1];
        if (lastTile.transform.position.x < 32f)
        {
            while (lastTile.transform.position.x < 32f)
            {
                //generate a tile to the right of the current tile
                pathTiles.Add(Instantiate(pathTilePrefab, lastTile.transform.GetChild(2).position, lastTile.transform.GetChild(2).rotation));
                lastTile = (GameObject)pathTiles[pathTiles.Count - 1];

            }
        }
        else if (lastTile.transform.position.x > 37f)
        {
            while (lastTile.transform.position.x > 37f)
            {
                //generate a tile to the right of the current tile
                pathTiles.Add(Instantiate(pathTilePrefab, lastTile.transform.GetChild(0).position, lastTile.transform.GetChild(0).rotation));
                lastTile = (GameObject)pathTiles[pathTiles.Count - 1];

            }
        }

    }

    //this function will randomly generate "n" Ammo based on the positon of path tiles in an Arraylist
    void generateAmmo(GameObject ammoPref, ArrayList tilesList)
    {
        GameObject tile;
        Vector3 position;

        //guarantee to add n ammo, n = numberOfAmmo
        while (numberOfAmmo > 0)
        {
            //add some randomness to the generation
            int randIndex = Random.Range(0, tilesList.Count);
            tile = (GameObject)tilesList[randIndex];
            tilesList.RemoveAt(randIndex);

            //set the y-position of the ammo +0.5 (above ground)
            position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z);

            //generate an ammo based on the random index
            Instantiate(ammoPref, position, tile.transform.rotation);
            numberOfAmmo--;
        }
        
    }

    //this function will intialized the 5x5 maze platforms (convert to Platform type and assign neighbours)
    //The order in the platforms array is start from the left bottom platform and end at the right top platform
    //And return an ArrayList containing Platform type objects
    ArrayList initializeMaze(GameObject[] platforms)
    {
        ArrayList platformsList = new ArrayList();

        //convert all platform objects (25) to Platform type
        for(int i = 0; i<25;i++)
        {
            platformsList.Add(new Platform(platforms[i]));
        }

        //add neighbours for each Platform object
        //Note: the index for neighbours will be: i-1 (west). i+1 (east), i-numCol (south), i+numCol (north)
        // and we have to handle the spacial case that the platfoms are at the boundary of maze. So,
        // we use one loop for adding the neighbours at each direction
        Platform pl;
        //#1: add west neighbour
        for (int i = 0; i < numRow*numCol; i++)
        {
            //if i % numCol == 0, then the platform is at the west boundary,
            //so no west neighbour for such a platform
            if (i % numCol == 0)
            {
                continue;
            }
            else  //add the platform at (i-1) in the list to the neighbour list of platform i, since i-1 is the west of i based on our initialization
            {
                pl = (Platform) platformsList[i];
                pl.addNeighbour((Platform) platformsList[i-1], PlatformDirection.west);
            }
        }
        //#2: add east neighbour
        for (int i = 0; i < numRow * numCol; i++)
        {
            //if i % numCol == numCol-1, then the platform is at the east boundary,
            //so no east neighbour for such a platform
            if (i % numCol == numCol-1)
            {
                continue;
            }
            else  //add the platform at (i+1) in the list to the neighbour list of platform i, since i+1 is the east of i based on our initialization
            {
                pl = (Platform) platformsList[i];
                pl.addNeighbour((Platform)platformsList[i+1], PlatformDirection.east);
            }
        }
        //#3: add south neighbour
        for (int i = 0; i < numRow * numCol; i++)
        {
            //if i < numRow, then the platform is at the south boundary,
            //so no east neighbour for such a platform
            if (i < numRow)
            {
                continue;
            }
            else  //add the platform at (i-numCol) in the list to the neighbour list of platform i, since i-numCol is the south of i based on our initialization
            {
                pl = (Platform) platformsList[i];
                pl.addNeighbour((Platform)platformsList[i-numCol], PlatformDirection.south);
            }
        }
        //#4: add north neighbour
        for (int i = 0; i < numRow * numCol; i++)
        {
            //if i > (numRow-1)*numCol, then the platform is at the north boundary,
            //so no north neighbour for such a platform
            if (i >= (numRow - 1) * numCol)
            {
                continue;
            }
            else  //add the platform at (i+numCol) in the list to the neighbour list of platform i, since i+numCol is the north of i based on our initialization
            {
                pl = (Platform) platformsList[i];
                pl.addNeighbour((Platform)platformsList[i+numCol], PlatformDirection.north);
            }
        }

        return platformsList;
    }


    //this function will generate randomly the maze using Randomized depth-first search
    //and return the solution of this maze as an ArrayList
    //Pseudo-code reference: https://en.wikipedia.org/wiki/Maze_generation_algorithm
    ArrayList generateMaze(ArrayList platformList)
    {
        //create a stack for the random BFS
        Stack solutionStack = new Stack();
        //Start from the startPlatform, mark it as visited and push it into stack
        Platform currentPlat = (Platform) platformList[startIndex];
        Platform neighbour;
        currentPlat.isVisited = true;
        solutionStack.Push(currentPlat);

        ArrayList solutionArray = new ArrayList();

        //while the stack is not empty
        while (solutionStack.Count > 0)
        {
            //pop a platform from the stack and make it current platform
            currentPlat = (Platform) solutionStack.Pop();
            
            //if the current platform is the goalPlatform, convert this solution stack to an array list
            if(object.ReferenceEquals(currentPlat.platformGameObject, mazePlatforms[goalIndex]))
            {
                solutionArray.Add(currentPlat);
                Stack clone = (Stack) solutionStack.Clone();
                
                while(clone.Count > 0)
                {
                    solutionArray.Add(clone.Pop());
                }
            }

            //if the current platform has any neighbours which have not been visited
            if(currentPlat.hasUnvisitedNeighbour())
            {
                //push back the current platform to the stack
                solutionStack.Push(currentPlat);

                //choose randomly an unvisited neighbour of the current platform
                neighbour = currentPlat.chooseRandomlyUnvisitedNeighbour();

                //remove the wall between the current platform and the unvisited neighbour
                ((Platform)currentPlat).destroyWall(neighbour);

                //mark the neighbour as visited and push it to the stack
                neighbour.isVisited = true;
                solutionStack.Push(neighbour);
            }
            else
            {
                continue;
            }

        }

        //return the solution as an array list
        return solutionArray;
    }


    //getter method for the maze solution array list
    public ArrayList getMazeSolution()
    {
        return mazeSolution;
    }

    //getter method for number of ammo putting in this scene
    public int getTotalAmmo()
    {
        return this.totalNumberOfAmmo;
    }


}
