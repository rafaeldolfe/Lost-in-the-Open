using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utils;
using Newtonsoft.Json;

static class ExtensionMethodsClass
{

    private static string CullCloneText(string name)
    {
        if (name.Length > 8)
        {
            if (name.Substring(name.Length - 7, 7) == "(Clone)")
            {
                return name.Substring(0, name.Length - 7);
            }
        }
        return name;
    }
    private static string ToPlainClassJson(this GameObject unit)
    {
        List<MonoBehaviour> scripts = unit.GetComponents<MonoBehaviour>().ToList();

        StringBuilder gameObject = new StringBuilder("{");

        StringBuilder scriptList = new StringBuilder("\"scripts\":[");

        bool firstObject = true;

        PrefabMetaData prefabMetaData = (PrefabMetaData)scripts.Find(script => script.GetType() == typeof(PrefabMetaData));

        scripts.Remove(prefabMetaData);

        gameObject.Append($"\"prefabName\":\"{prefabMetaData.prefabName}\"");

        gameObject.Append(",");

        gameObject.Append($"\"folder\":\"{prefabMetaData.folder}\"");

        gameObject.Append(",");

        foreach (MonoBehaviour script in scripts)
        {
            if (!firstObject)
            {
                scriptList.Append(",");
            }
            StringBuilder monoJson = new StringBuilder("{");
            monoJson.Append($"\"type\":\"{script.GetType()}\",");
            StringBuilder monoKeys = new StringBuilder("\"_keys\":[");
            StringBuilder monoProperties = new StringBuilder("\"properties\":");
            string scriptProps = JsonUtility.ToJson(script, true);
            monoProperties.Append(scriptProps);
            monoJson.Append(monoProperties);
            monoJson.Append("}");
            scriptList.Append(monoJson);
            firstObject = false;
        }
        scriptList.Append("]");

        gameObject.Append(scriptList.ToString());

        gameObject.Append("}");

        return gameObject.ToString();
    }
    public static PlainMesh GetPlainClass(this Mesh mesh)
    {
        return new PlainMesh(mesh.vertices.ToList(), mesh.uv.ToList(), mesh.triangles.ToList());
    }
    public static PlainMaterial GetPlainClass(this Material material)
    {
        return new PlainMaterial(material.name, Constants.MATERIAL_RESOURCE_PATH);
    }
    public static PlainGameObject GetPlainClass(this GameObject unit)
    {
        string json = ToPlainClassJson(unit);
        return FromJson<PlainGameObject>(json);
    }
    public static List<PlainGameObject> GetPlainClasses(this IEnumerable<GameObject> units)
    {
        return units
            .Select(unit => unit.GetPlainClass())
            .ToList();
    }
    public static List<GameObject> InstantiateSelves(this IEnumerable<PlainGameObject> units, Func<GameObject, GameObject> instantiateFunc)
    {
        return units
            .Select(unit => unit.InstantiateSelf(instantiateFunc))
            .ToList();
    }
    private static string ToJson<T>(T data)
    {
        return JsonConvert.SerializeObject(data);
    }
    private static T FromJson<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
