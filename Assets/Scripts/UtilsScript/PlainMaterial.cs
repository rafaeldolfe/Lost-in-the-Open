using UnityEngine;
using System.Collections;

public class PlainMaterial
{
    /// <summary>
    /// This exact prefab's name
    /// </summary>
    public string materialName;
    /// <summary>
    /// The path to this prefab's location in the resource folder
    /// </summary>
    public string folder;

    public PlainMaterial(string materialName, string folder)
    {
        this.materialName = materialName;
        this.folder = folder;
    }

    /// <summary>
    /// Converts this PlainMesh class into its Unity Mesh equivalent
    /// </summary>
    /// <returns></returns>
    public Material GetUnityClass()
    {
        return Resources.Load($"{folder}/{materialName}") as Material;
    }

    public override string ToString()
    {
        return $"path: {folder}/{materialName}";
    }
}
