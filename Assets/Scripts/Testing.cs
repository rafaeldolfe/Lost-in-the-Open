using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing : MonoBehaviour
{
    private GridScript grid;

    private void Start()
    {
        grid = new GridScript(4, 2, 16f, 40, new Vector3(2f, 2f, 2f));
        new GridScript(6, 2, 20f, 40, new Vector3(-8f, -28f, 2f));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.SetValue(UtilsClass.GetMouseWorldPosition(), 56);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(grid.GetValue(UtilsClass.GetMouseWorldPosition()));
        }
    }
}
