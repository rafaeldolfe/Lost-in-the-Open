using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Encounter
{
    public interface ILifecycleTile
    {
        void TileAwake(Vector3Int position, Tilemap tilemap);
        void TileStart(Vector3Int position, Tilemap tilemap);
    }
}