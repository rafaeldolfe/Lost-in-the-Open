using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Utils;

namespace Encounter
{
	[CreateAssetMenu]
	public class NonWalkable : Tile, ILifecycleTile
	{
		private MapManager mm;
		public string type;

		public void TileAwake(Vector3Int position, Tilemap tilemap)
		{

		}
		public void TileStart(Vector3Int position, Tilemap tilemap)
		{
			List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
			List<MonoBehaviour> deps = new List<MonoBehaviour>
		{
			(mm = FindObjectOfType(typeof(MapManager)) as MapManager),
		};
			if (deps.Contains(null))
			{
				throw ProgramUtils.DependencyException(deps, depTypes);
			}
			mm.SetWalkability(position.x, position.y, false);
		}
	}
}