/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class MapGrid
    {
        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;
        public List<GridContainer> tiles;

        public MapGrid(int width, int height, float cellSize, Vector3 originPosition, Func<MapGrid, int, int, GridContainer> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            tiles = new List<GridContainer>(width * height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tiles.Add(createGridObject(this, x, y));
                }
            }
        }

        /// <summary>
        /// Get a rectangular area from the grid centered around point (x,y). 
        /// Note, only odd widths and heights are supported atm.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public List<GridContainer> GetRectangularArea(int x, int y, int width, int height)
        {
            List<GridContainer> rectangle = new List<GridContainer>();
            int currentx = x;
            int currenty = y;
            for (int xi = currentx - width; xi < currentx + width; xi++)
            {
                int gridWidth = GetWidth();
                int gridHeight = GetWidth();
                if (xi < 0 || xi > gridWidth) continue;
                for (int yi = currenty - height; yi < currenty + height; yi++)
                {
                    if (yi < 0 || yi > gridHeight) continue;
                    rectangle.Add(tiles[xi + yi * gridHeight]);
                }
            }
            return rectangle;
        }
        public int GetWidth()
        {
            return width;
        }
        public int GetHeight()
        {
            return height;
        }
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * cellSize + originPosition;
        }
        public void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }
        public void SetGridObject(int x, int y, GridContainer value)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                tiles[y * width + x] = value;
            }
        }
        public GridContainer GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                return tiles[y * width + x];
            }
            else
            {
                return default(GridContainer);
            }
        }
    }
}