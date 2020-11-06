using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounter;
using ThreadedPathfinding;

public class Linker : MonoBehaviour
{
    // The same MyMap object from earlier. Replace this with your own class or system.
    public MapGrid Map;

    // Called once when the scene is loaded.
    // Important to use Awake and not Start so that no pathfinding requests are made before we set the provider.
    void Awake()
    {
        // Get a reference to the PathfindingManager object.
        PathfindingManager manager = PathfindingManager.Instance;

        // Now set the provider object. Replace MyCustomProvider with whatever you named your provider.
        manager.Provider = new MyCustomTileProvider(Map);
    }
}
