using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadScript : MonoBehaviour
{
    private static GameObject thisInstance;
    void Awake()
    {
        if (thisInstance == null)
        {
            thisInstance = gameObject;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
    }
}
