using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position : MonoBehaviour
{
    public int x { get; private set; }
    public int z { get; private set; }

    public void init(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    void Update()
    {
        if (ProgramDebug.debug)
        {
            if (gameObject.GetComponent<Move>() != null && gameObject.GetComponent<Move>().Status() == "Busy")
            {
                return;
            }
            int gx = (int) gameObject.transform.position.x;
            int gz = (int) gameObject.transform.position.z;
            if (x != gx)
            {
                Debug.Log(string.Format("Position x and gameObject x are mismatched: ({0} != {1})", x, gx));
            }
            if (z != gz)
            {
                Debug.Log(string.Format("Position z and gameObject z are mismatched: ({0} != {1})", z, gz));
            }
        }
    }

    public void MoveTo(int tx, int tz)
    {
        this.x = tx;
        this.z = tz;
    }
}
