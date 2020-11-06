using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Encounter
{
    public interface ILifecycleTile
    {
        /// <summary>
        /// Implements Awake and Start methods into tile. Order: TileAwake -> Awake -> TileStart -> Start
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tilemap"></param>
        void TileAwake(Vector3Int position, Tilemap tilemap);
        void TileStart(Vector3Int position, Tilemap tilemap);
    }
}