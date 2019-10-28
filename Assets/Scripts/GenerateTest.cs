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
    private GridScript<PathNode> grid;
    private GameObject playerBoatPosition;
    private float x;
    private float y;

    private Vector3 highlightedNodePos = new Vector3(int.MinValue, int.MinValue, int.MinValue);
    private Vector4 cyanColor = new Vector4(0, 1, 1, 0.5f);

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

        //grid = new GridScript<int>(18, 6, 0.18f + -0.005f, min + new Vector3(0.1f, 0.1f, -1f) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0, 0.01f, 0), (GridScript<int> g, int x, int y) => 0);
        grid = new GridScript<PathNode>(18, 6, 0.18f + -0.005f, min + new Vector3(0.12f, 0.11f, -1), (GridScript<PathNode> g, int x, int y) => new PathNode(g, x, y));


        boatWalkingGrid = new int[18, 6];

        string[] gridSetup = file.text.Split(new string[] { "\n" }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);

        for (int s = 0; s < 6; s++)
        {
            int[] grids = gridSetup[s].Split(new string[] { "," }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries).Select(c => Int32.Parse(c)).ToArray();
            for (int l = 0; l < 18; l++)
            {
                boatWalkingGrid[l, s] = grids[l];
                if (grids[l] == -1)
                {
                    transparentMat.SetColor("_Color", Color.red);
                    grid.GetValue(l, s).SetStatus(PathNode.STATUS.UNAVAILABLE);
                    grid.SetColor(l, s, transparentMat);
                }
            }
        }
    }

    private void Update()
    {
        PathNode current = grid.GetValue(UtilsClass.GetMouseWorldPosition());
        PathNode previous;
        if (current == null)
        {
            previous = grid.GetValue(highlightedNodePos);
            if (previous != null)
            {
                grid.RemoveColor(highlightedNodePos);
                previous.SetStatus(PathNode.STATUS.NORMAL);
                resetHighlight();
            }
            return;
        }
        PathNode.STATUS caseSwitch = current.GetStatus();
        switch (caseSwitch)
        {
            case PathNode.STATUS.UNAVAILABLE:
                previous = grid.GetValue(highlightedNodePos);
                if (previous != null)
                {
                    grid.RemoveColor(highlightedNodePos);
                    previous.SetStatus(PathNode.STATUS.NORMAL);
                    resetHighlight();
                }
                return;
            case PathNode.STATUS.HIGHLIGHTED:
                return;
            case PathNode.STATUS.NORMAL:
                previous = grid.GetValue(highlightedNodePos);
                if (previous != null)
                {
                    grid.RemoveColor(highlightedNodePos);
                    previous.SetStatus(PathNode.STATUS.NORMAL);
                    resetHighlight();
                }
                grid.SetColor(UtilsClass.GetMouseWorldPosition(), transparentMat, cyanColor);
                current.SetStatus(PathNode.STATUS.HIGHLIGHTED);
                highlightedNodePos = UtilsClass.GetMouseWorldPosition();
                return;
        }
    }

    private void resetHighlight()
    {
        highlightedNodePos = new Vector3(int.MinValue, int.MinValue, int.MinValue);
    }
}
