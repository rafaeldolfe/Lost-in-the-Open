using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace Encounter
{
    public class Position : MonoBehaviour
    {
        [SerializeField]
        private int _x;
        [SerializeField]
        private int _y;
        public int x
        {
            get { return _x; }
            private set { _x = value; }
        }
        public int y
        {
            get { return _y; }
            private set { _y = value; }
        }

        public void init(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int GetX()
        {
            return _x;
        }
        public int GetY()
        {
            return _y;
        }

        void Update()
        {
            if (ProgramDebug.debug)
            {
                if (gameObject.GetComponent<Move>() != null && gameObject.GetComponent<Move>().Status() == "Busy")
                {
                    return;
                }
                int gx = (int)gameObject.transform.position.x;
                int gy = (int)gameObject.transform.position.y;
                if (x != gx)
                {
                    Debug.Log(string.Format("Position x and gameObject x are mismatched: ({0} != {1})", x, gx));
                }
                if (y != gy)
                {
                    Debug.Log(string.Format("Position y and gameObject y are mismatched: ({0} != {1})", y, gy));
                }
            }
        }

        public void MoveTo(int tx, int ty)
        {
            this.x = tx;
            this.y = ty;
        }

        public override string ToString()
        {
            return $"Position: ({_x},{_y})";
        }
    }
}