using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    private static GameObject thisInstance;
    public bool InstanceExists()
    {
        if (thisInstance == null)
        {
            thisInstance = gameObject;
        }
        else
        {
            Destroy(gameObject);
            return true;
        }

        DontDestroyOnLoad(this);
        return false;
    }
}
