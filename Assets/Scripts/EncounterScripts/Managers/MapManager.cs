using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class MapManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        public MapGrid grid { get; set; }

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            grid = new MapGrid(50, 50, 1, new Vector3(0, 0, 0), (MapGrid g, int x, int y) => new GridContainer(g, x, y));

            gem.StartListening("Move", MoveActor);
            gem.StartListening("Death", RemoveActor);
            gem.StartListening("RegisterUnit", RegisterUnit);
        }
        void OnDestroy()
        {
            gem.StopListening("Move", MoveActor);
            gem.StopListening("Death", RemoveActor);
            gem.StopListening("RegisterUnit", RegisterUnit);
        }
        public void SetWalkability(int x, int y, bool walkability)
        {
            grid.GetGridObject(x, y).pn.isWalkable = walkability;
        }
        private void RegisterUnit(GameObject instantiatedGameObject, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected list with 1 Vector2Int, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(Vector2Int))
            {
                throw new Exception(string.Format("Expected 1st item to be Vector2Int, found ", parameters[0].GetType()));
            }
            Vector2Int position = (Vector2Int)parameters[0];
            instantiatedGameObject.transform.position = new Vector3(position.x, position.y);
            grid.GetGridObject(position.x, position.y).SetActor(instantiatedGameObject);
            instantiatedGameObject.GetComponent<Position>().init(position.x, position.y);
        }
        private void MoveActor(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (grid.GetGridObject(x, y) == null)
            {
                throw new Exception("Invalid grid coordinates");
            }
            if (grid.GetGridObject(tx, ty) == null)
            {
                throw new Exception("Invalid target grid coordinates");
            }
            if (grid.GetGridObject(x, y).actor == null)
            {
                throw new Exception(string.Format("Expected actor at position ({0}, {1}), but found null", x, y));
            }
            if (grid.GetGridObject(tx, ty).actor != null)
            {
                throw new Exception(string.Format("Expected empty position at ({0}, {1}), but found an actor", tx, ty));
            }
            grid.GetGridObject(x, y).RemoveActor();
            grid.GetGridObject(tx, ty).SetActor(invoker);
        }
        private void RemoveActor(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (grid.GetGridObject(x, y) == null)
            {
                throw new Exception("Invalid grid coordinates");
            }
            if (grid.GetGridObject(x, y).actor == null)
            {
                throw new Exception(string.Format("Expected actor at position ({0}, {1}), but found null", x, y));
            }
            grid.GetGridObject(x, y).RemoveActor();
        }
    }
}