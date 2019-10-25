using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class GridScript
{
    private GameObject[,] highlightArray;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private int[,] gridArray;
    private Color[,] colorArray;
    private TextMesh[,] debugTextArray;

    public GridScript(int width, int height, float cellSize, int textSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new int[width, height];
        highlightArray = new GameObject[width, height];
        debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, textSize, Color.white, TextAnchor.MiddleCenter);
                //Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                //Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
            }
        }
        //Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        //Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

        SetValue(2, 1, 56);
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetValue(int x, int y, int value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            debugTextArray[x, y].text = gridArray[x, y].ToString();
        }
    }

    public void SetValue(Vector3 worldPosition, int value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    public void SetColor(int x, int y, Material transparentMat)
    {
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
        Debug.Log("x and y conversion: " + x + ", " + y);
        transparentMat.SetColor("_Color", color);
        SetColor(x, y, transparentMat);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return -1;
        }
    }

    public int GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }
}
