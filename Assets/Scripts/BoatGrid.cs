using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System.Linq;
using System;

public class BoatGrid<TGridObject>
{
    public const float DEFAULT_Z = -1f;

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;

    private GameObject[,] highlightArray;
    private TGridObject[,] gridArray;
    private GameObject[,] units;
    private Color[,] colorArray;

    public BoatGrid(int width, int height, float cellSize, Vector3 originPosition, Func<BoatGrid<TGridObject>, int, int, TGridObject> initObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];
        highlightArray = new GameObject[width, height];
        units = new GameObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = initObject(this, x, y);
            }
        }

        if (DebugStore.debugMode)
        {
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
        }
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 pos = new Vector3(x, y) * cellSize + originPosition;
        pos.z = DEFAULT_Z;
        return pos;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetValue(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
        }
    }

    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    private void RemoveColor(int x, int y)
    {
        if (x > gridArray.GetLength(0) || x < 0 || y > gridArray.GetLength(1) || y < 0)
        {
            return;
        }
        UnityEngine.Object.Destroy(highlightArray[x, y]);
        highlightArray[x, y] = null;
    }

    public void RemoveColor(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        RemoveColor(x, y);
    }

    public void SetColor(int x, int y, Material transparentMat)
    {
        if (x > gridArray.GetLength(0) || x < 0 || y > gridArray.GetLength(1) || y < 0)
        {
            return;
        }
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.layer = 8;
            cube.GetComponent<MeshRenderer>().material = new Material(transparentMat);
            cube.transform.position = GetWorldPosition(x, y) - new Vector3(-cellSize, -cellSize) / 2;
            cube.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
            highlightArray[x, y] = cube;
        }
    }

    public void SetColor(Vector3 worldPosition, Material transparentMat, Color color)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        transparentMat.SetColor("_Color", color);
        SetColor(x, y, transparentMat);
    }

    public TGridObject GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    public void AddUnit(int x, int y, GameObject unit)
    {
        units[x, y] = unit;
        UnityEngine.Object.Instantiate(unit, GetWorldPosition(x,y), Quaternion.identity);
    }

    public void AddUnit(Vector3 worldPosition, GameObject unit)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        AddUnit(x, y, unit);
    }
}
