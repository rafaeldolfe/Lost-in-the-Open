using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System.Linq;
using System;

public class GenerateTest : MonoBehaviour
{
    public Material transparentMat;
    public GameObject prefab;
    public TextAsset file;

    private int[,] boatWalkingGrid;
    private GridScript grid;
    private GameObject playerBoatPosition;
    private float x;
    private float y;

    private float SizeX;
    private float SizeY;
    void Start()
    {
        playerBoatPosition = GameObject.Find("PlayerBoatPosition");
        x = playerBoatPosition.transform.position.x;
        y = playerBoatPosition.transform.position.y;

        Instantiate(prefab, new Vector3(x, y, -1f), Quaternion.identity);// playerBoatPosition.transform.rotation);

        Vector3 max = prefab.GetComponent<SpriteRenderer>().bounds.max;
        Vector3 min = prefab.GetComponent<SpriteRenderer>().bounds.min;

        Debug.Log(prefab.GetComponent<SpriteRenderer>().bounds.size);
        Debug.Log(prefab.GetComponent<SpriteRenderer>().bounds.max);
        Debug.Log(prefab.GetComponent<SpriteRenderer>().bounds.min);

        Debug.DrawLine(max, min, Color.white, 100f);
        Debug.DrawLine(new Vector3(max.x, 0, -1), new Vector3(min.x, 0, -1), Color.blue, 100f);
        Debug.DrawLine(new Vector3(0, max.y, -1), new Vector3(0, min.y, -1), Color.red, 100f);
        Debug.DrawLine(new Vector3(0, 0, -1), new Vector3(0, 0, -1), Color.green, 100f);

        grid = new GridScript(18, 6, 0.18f + -0.005f, 1, min + new Vector3(0.1f,0.1f,-1f) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0, 0.01f, 0));
        boatWalkingGrid = new int[6, 18];

        string[] gridSetup = file.text.Split(new string[] { "\n" }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

        for (int x = 0; x < 6; x++)
        {
            int[] grids = gridSetup[x].Split(new string[] { "," }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries).Select(c => Int32.Parse(c)).ToArray();
            for (int y = 0; y < 18; y++)
            {
                boatWalkingGrid[x, y] = grids[x];
                if (grids[x] == -1)
                {
                    Debug.Log(grids[x]);
                    Debug.Log(x + ", " + y);
                    grid.SetColor(new Vector3(x,y), transparentMat, Color.red);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(UtilsClass.GetMouseWorldPosition(), 2);
            grid.SetColor(UtilsClass.GetMouseWorldPosition(), transparentMat, Color.cyan);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.GetValue(UtilsClass.GetMouseWorldPosition()));
        }
    }
}
