using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //For getting inputs

//Adam McWilliams
//contact adammcw01@gmail.com

public class Maze : MonoBehaviour
{
    public GameObject Wall; //the wall is the compent part of the maze, i.e. a maze is built of walls
    public GameObject MazeFloor;//The base that the maze is built on

    public int width = 5;//the size of the width of the maze, 5 is a default value
    public int height = 5;//the size of the height of the maze, 5 is a default value

    public Slider X_Slider;
    public Slider Y_Slider;

    private IndividualCell[,] grid;
    private int currentX = 0;
    private int currentY = 0;

    private bool isFinished;

    //public methods
    //regenerate the maze on button click
    public void Regenerate()
    {
        //destory all child objects of maze
        foreach (Transform gameobj in transform)
        {
            Destroy(gameobj.gameObject);
        }

        //reinitalise a new grid
        initialiseGrid();

        //reset the algorithms starting position
        currentX = 0;
        currentY = 0;
        isFinished = false;
        //create a new maze
        mazeAlgorithm();

    }

    //change width using slider
    public void SetWidth(float newWidth)
    {
        //cast float as int
        width = (int)newWidth;
    }

    //change height using slider
    public void SetHeight(float newHeight)
    {
        //cast float as int
        height = (int)newHeight;
    }

    // Initialise the maze for the given size.
    void initialiseGrid() {

        //Set the size of each cell to be based on the number of cells
        MazeFloor.transform.localScale = new Vector3(550 / width, 550 / height);
        //Adjust wall size
        Wall.transform.localScale = new Vector3(4, 550 / height);
        float wallSize = Wall.transform.localScale.y;//A varaible that holds the size of the wall object, this allows us to move each wall by this amount to prevent overlapping.
        float floorWidthSize = MazeFloor.transform.localScale.x;
        int gridOffset = 550 / height * 3 / 4;//The amount to move the maze by on the screen

        //Translate every object by this amount to solve issues with camera
        int xOffSet = 580 + 550 / width * 3 / 4;
        int yOffSet = 320 + 550 / height * 3 / 4;

        //create a 2D array of cell obejcts
        grid = new IndividualCell[width, height];

        //for every row and collum of the maze create the objects to build the maze
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Populate the grid with Cell obejcts
                grid[i, j] = new IndividualCell();

                //Debug.Log("i = " + i + " j = " + j);

                //every cell has a floor, the size is determined by the number of cells around it so that the entire maze is always the same size
                GameObject floor = Instantiate(MazeFloor, new Vector3(i * floorWidthSize + xOffSet, j * wallSize + yOffSet, -1), Quaternion.identity);
                floor.name = "Floor(" + j + "," + i + ")";

                //every cell can have a floor and up to 4 walls around it, the walls are created in the middle of the object and the offset by half of the size of the floor
                GameObject leftWall = Instantiate(Wall, new Vector3(i * floorWidthSize + xOffSet - (((550 / width) - 2) / 2), j * wallSize + yOffSet, -2), Quaternion.identity);
                leftWall.name = "leftWall(" + j + "," + i + ")";

                GameObject rightWall = Instantiate(Wall, new Vector3(i * floorWidthSize + xOffSet + (((550 / width) - 2) / 2), j * wallSize + yOffSet, -2), Quaternion.identity);
                rightWall.name = "rightWall(" + j + "," + i + ")";

                //the top and bottom walls are made the same, however they are also rotated 90 degrees along the Z axis
                GameObject topWall = Instantiate(Wall, new Vector3(i * floorWidthSize + xOffSet, j * wallSize + yOffSet + (((550 / height) - 2) / 2), -2), Quaternion.Euler(0, 0, 90));
                topWall.name = "topWall(" + j + "," + i + ")";
                topWall.transform.localScale = new Vector3(4, 550 / width);

                GameObject bottomWall = Instantiate(Wall, new Vector3(i * floorWidthSize + xOffSet, j * wallSize + yOffSet - (((550 / height) - 2) / 2), -2), Quaternion.Euler(0, 0, 90));
                bottomWall.name = "bottomWall(" + j + "," + i + ")";
                bottomWall.transform.localScale = new Vector3(4, 550 / width);

                //create a cell object by using the walls
                grid[i, j].leftWallCell = leftWall;
                grid[i, j].rightWallCell = rightWall;
                grid[i, j].topWallCell = topWall;
                grid[i, j].bottomWallCell = bottomWall;

                //All of the maze components are grouped with the maze object to avoid clutter
                floor.transform.parent = transform;
                leftWall.transform.parent = transform;
                rightWall.transform.parent = transform;
                topWall.transform.parent = transform;
                bottomWall.transform.parent = transform;

            }
        }
    }

    void mazeAlgorithm(){

        //set starting cell to visited
        grid[currentX, currentY].hasVitied = true;

        System.Random randInt = new System.Random();

        //perform walk algorithm until dead-end is reached
        //find new starting cell and repeat until every cell is visited

        do
        {
            walk(randInt);
            findNewStart(randInt);
        } while (!isFinished);

    }

    //When algorithm reaches a dead end find a new cell to start from, an unvisited cell with a visited neighbour
    private void findNewStart(System.Random randInt)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!grid[i,j].hasVitied && hasAVisitedNeighbour(i,j))
                {
                    currentX = i;
                    currentY = j;
                    grid[i, j].hasVitied = true;
                    destroyNeighourWall(randInt);
                    return;
                }
            }
        }
        //code only reaches here if all cells have been visited
        //therefore algorithm is finished
        isFinished = true;
        return;
    }

    //destory the wall beetween the new start and the visited cell
    private void destroyNeighourWall(System.Random randInt)
    {
        //true when the connecting wall has been destoryed
        bool destroyed = false;

        do
        {
            int direction = randInt.Next(4);
            switch (direction)
            {
                //check up
                case 0:
                    if (currentY < height -1 && grid[currentX, currentY+1].hasVitied)
                    {
                        //destory the two walls between the visited cell and new cell
                        tryDestroyCellWall(currentX, currentY + 1, 3);
                        tryDestroyCellWall(currentX, currentY, 2);
                        destroyed = true;
                    }
                    break;
                //check down
                case 1:
                    if (currentY > 0 && grid[currentX, currentY - 1].hasVitied)
                    {
                        tryDestroyCellWall(currentX, currentY -1, 2);
                        tryDestroyCellWall(currentX, currentY, 3);
                        destroyed = true;
                    }
                    break;
                //check left
                case 2:
                    if (currentX > 0 && grid[currentX-1, currentY].hasVitied)
                    {
                        tryDestroyCellWall(currentX-1, currentY, 1);
                        tryDestroyCellWall(currentX, currentY, 0);
                        destroyed = true;
                    }
                    break;
                //check right
                case 3:
                    if (currentX < width - 1 && grid[currentX+1, currentY].hasVitied)
                    {
                        tryDestroyCellWall(currentX+1, currentY, 0);
                        tryDestroyCellWall(currentX, currentY, 1);
                        destroyed = true;
                    }
                    break;
                default:
                    break;
            }
        } while (!destroyed);

    }

    private bool hasAVisitedNeighbour(int x, int y)
    {
        //check up
        if (y < height -1 && grid[x,y+1].hasVitied)
        {
            return true;
        }

        //check down
        if (y > 0 && grid[x,y-1].hasVitied)
        {
            return true;
        }

        //check left
        if (x > 0 && grid[x-1,y].hasVitied)
        {
            return true;
        }

        //check right
        if (x < width-1 && grid[x+1,y].hasVitied)
        {
            return true;
        }

        return false;
    }

    private void walk(System.Random randInt)
    {
       //int errorHandle = 0;
        do
        {
            /*errorHandle++;
            if (errorHandle == 2000)
            {
                Debug.Log("Broken by infinite loop in walk" + errorHandle);
                break;
            }*/

            //create a random number to decide what cell to visit next
            int nextCellDir = randInt.Next(4);

            switch (nextCellDir)
            {
                //check left direction
                case 0:
                    //Debug.Log("check left" + currentX + currentY);
                    if (currentX > 0 && !grid[currentX - 1, currentY].hasVitied)
                    {
                        tryDestroyCellWall(currentX, currentY, 0);

                        currentX--;
                        grid[currentX, currentY].hasVitied = true;

                        tryDestroyCellWall(currentX, currentY, 1);

                    }
                    break;

                //check right direction
                case 1:
                    //Debug.Log("check right" + currentX + currentY);
                    if (currentX < width - 1 && !grid[currentX + 1, currentY].hasVitied)
                    {
                        tryDestroyCellWall(currentX, currentY, 1);

                        currentX++;
                        grid[currentX, currentY].hasVitied = true;

                        tryDestroyCellWall(currentX, currentY, 0);
                    }
                    break;

                //check up direction
                case 2:
                    //Debug.Log("check up" + currentX + currentY);
                    if (currentY < height - 1 && !grid[currentX, currentY + 1].hasVitied)
                    {
                        tryDestroyCellWall(currentX, currentY, 2);

                        currentY++;
                        grid[currentX, currentY].hasVitied = true;

                        tryDestroyCellWall(currentX, currentY, 3);
                    }
                    break;

                //check down direction
                case 3:
                    //Debug.Log("check down" + currentX + currentY);
                    if (currentY > 0 && !grid[currentX, currentY - 1].hasVitied)
                    {
                        tryDestroyCellWall(currentX, currentY, 3);

                        currentY--;
                        grid[currentX, currentY].hasVitied = true;

                        tryDestroyCellWall(currentX, currentY, 2);
                    }
                    break;
            }
        } while (!allNeighboursVisited());
    }

    private bool allNeighboursVisited()
    {
        int x = currentX;
        int y = currentY;
        int count = 0;
        //check up
        if (isVisited(currentX, currentY + 1))
        {
            count++;
        }

        //check down
        if (isVisited(currentX, currentY -1))
        {
            count++;
        }

        //check left
        if (isVisited(currentX -1, currentY))
        {
            count++;
        }

        //check right
        if (isVisited(currentX +1, currentY))
        {
            count++;
        }

        //Debug.Log(count);

        //Check for middle cells
        if (count == 4)
        {
            return true;
        }
        //check for edge cells
        else if (count == 3 && (x == 0 || y == 0 || x == width -1 || y == height -1)) 
        {
            return true;
        }

        //check for corner cells
        //count is 2
        //and x,y = 0,0 or (x == 0 && y == 0)
        //width - 1, height -1 or (x = width -1 && y == height -1)
        //0, height -1 or (x = 0 && y == height -1)
        //width -1, 0 (x = width -1 && y == 0)
        else if (count == 2 && (((x == 0)&&(y == 0))||((x == width-1)&&(y == height -1))||((x == 0)&&(y == height-1))||(x == width -1)&&(y == 0)))
        {
            return true;
        }

        return false;
    }

    private bool isVisited(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height && grid[x,y].hasVitied)
        {
            return true;
        }
        return false;
    }

    //check if wall exists and destroy it
    //0,1,2,3 -> left, right, top, bottom
    void tryDestroyCellWall(int x, int y, int direction) {
        switch (direction) {
            case 0:
                if (grid[x, y].leftWallCell)
                {
                    Destroy(grid[x, y].leftWallCell);
                }
                break;
            case 1:
                if (grid[x, y].rightWallCell)
                {
                    Destroy(grid[x, y].rightWallCell);
                }
                break;
            case 2:
                if (grid[x, y].topWallCell)
                {
                    Destroy(grid[x, y].topWallCell);
                }
                break;
            case 3:
                if (grid[x, y].bottomWallCell)
                {
                    Destroy(grid[x, y].bottomWallCell);
                }
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Main
    void Start()
    {
        //call method to create the grid
        initialiseGrid();
        //run the hunt and kill algorithm, documentation for algorithm in spreadsheet
        mazeAlgorithm();

    }
}
