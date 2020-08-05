using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Encounter
{
    public class TilemapLifecycleRunner : MonoBehaviour
    {
        private Tilemap tilemap;

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Tilemap));
            }

            foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(position) is ILifecycleTile tile)
                {
                    tile.TileAwake(position, tilemap);
                }
            }
        }
        private void Start()
        {
            foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(position) is ILifecycleTile tile)
                {
                    tile.TileStart(position, tilemap);
                }
            }
        }
    }
}