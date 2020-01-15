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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid
{

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    public GridContainer[,] gridArray;

    public MapGrid(int width, int height, float cellSize, Vector3 originPosition, Func<MapGrid, int, int, GridContainer> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new GridContainer[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int z = 0; z < gridArray.GetLength(1); z++)
            {
                gridArray[x, z] = createGridObject(this, x, z);
            }
        }

        bool showDebug = false;
        if (showDebug)
        {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int z = 0; z < gridArray.GetLength(1); z++)
                {
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
            };
        }
    }

    public int GetWidth()
    {
        return width;
    }
    public int GetHeight()
    {
        return height;
    }
    public float GetCellSize()
    {
        return cellSize;
    }
    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, z) * cellSize + originPosition;
    }
    public void GetXY(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }
    public void SetGridObject(int x, int z, GridContainer value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            gridArray[x, z] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, z = z });
        }
    }
    public void TriggerGridObjectChanged(int x, int z)
    {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }
    public void SetGridObject(Vector3 worldPosition, GridContainer value)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);
        SetGridObject(x, z, value);
    }
    public GridContainer GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            return gridArray[x, z];
        }
        else
        {
            return default(GridContainer);
        }
    }
    public GridContainer GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        GetXY(worldPosition, out x, out z);
        return GetGridObject(x, z);
    }
    public void MoveGridObject(GameObject gameObject, int prevx, int prevz, int newx, int newz)
    {
        GridContainer prev = gridArray[prevx, prevz];
        GridContainer target = gridArray[newx, newz];

        for (int i = 0; i < prev.gos.Count; i++)
        {
            if (prev.gos[i] == gameObject)
            {
                prev.gos.RemoveAt(i);
                target.gos.Add(gameObject);
                break;
            }
        }
    }
    public void MoveGridUnit(GameObject gameObject, int prevx, int prevz, int newx, int newz)
    {
        GridContainer prev = gridArray[prevx, prevz];
        GridContainer target = gridArray[newx, newz];

        prev.RemoveActor();
        target.SetActor(gameObject);
    }

}
