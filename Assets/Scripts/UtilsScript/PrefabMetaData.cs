using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class PrefabMetaData : MonoBehaviour
    {
        public string prefabName;
        public string folder;

        public void Start()
        {
            gameObject.name = prefabName;
        }
    }
}
