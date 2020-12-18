using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Author: ZiQi Li
// A class for construct a grid system for the map
public class GridMap 
{
    private float x_cellWidth = 0;  // width of a cell on x direction
    private float z_cellWidth = 0;  // width of a cell on z direction
    private int x_numCells = 0;  // total number of cells on x direction
    private int z_numCells = 0;  // total number of cells on z direction
    private Vector2[,] grid;  // a 2D array to store the position of cells (midpoint)


    public GridMap(Vector3 origin, float x_width, float z_width, int x_numCells, int z_numCells)
    {
        this.x_cellWidth = x_width;
        this.z_cellWidth = z_width;
        this.x_numCells = x_numCells;
        this.z_numCells = z_numCells;
        grid = new Vector2[this.x_numCells, this.z_numCells];  // construct the 2D grid array
        constructGrid(origin.x, origin.z);
    }


    /// <summary>
    /// Function to construct the 2D grid system using a origin point (x,z)
    /// </summary>
    /// <param name="x_origin"></param>
    /// <param name="z_origin"></param>
    private void constructGrid(float x_origin, float z_origin)
    {
        for (int i = 0; i < this.x_numCells; i++)
        {
            for(int k = 0; k < this.z_numCells; k++)
            {
                this.grid[i, k] = new Vector2(i*this.x_cellWidth + x_origin, k * this.z_cellWidth + z_origin);
            }
        }
    }

    /// <summary>
    /// Funtion to get the cell position with index [xth][zth] in the grid 2D array
    /// </summary>
    /// <param name="xth"></param>
    /// <param name="zth"></param>
    /// <returns>Position of (x,z) as Vector2</returns>
    public Vector2 getCellPosition(int xth, int zth)
    {
        return this.grid[xth, zth];
    }

}
