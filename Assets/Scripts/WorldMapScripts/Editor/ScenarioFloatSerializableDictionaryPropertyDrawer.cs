using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WorldMap
{
    [CustomPropertyDrawer(typeof(ScenarioFloatDictionary))]
    public class ScenarioFloatSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
}
