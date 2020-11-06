using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHighlightScript : MonoBehaviour
{
    public List<MeshRenderer> renderers;
    public void SetColor(Color color)
    {
        foreach (MeshRenderer meshr in renderers)
        {
            meshr.material.color = color;
        }
    }
}
