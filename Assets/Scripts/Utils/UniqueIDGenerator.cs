using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueIDGenerator
{
    private static int id = 0;

    public static int GenerateID()
    {
        int temp = id;
        id++;
        return temp;
    }
}
