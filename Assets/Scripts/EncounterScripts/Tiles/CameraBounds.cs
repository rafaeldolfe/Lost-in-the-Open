using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Utils;

namespace Encounter
{
	[CreateAssetMenu]
	public class CameraBounds : Tile, ILifecycleTile
	{
		private CameraManager cm;
		private MapManager mm;

		public void TileAwake(Vector3Int position, Tilemap tilemap)
		{
			List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
			List<MonoBehaviour> deps = new List<MonoBehaviour>
			{
				(cm = FindObjectOfType(typeof(CameraManager)) as CameraManager),
				(mm = FindObjectOfType(typeof(MapManager)) as MapManager),
			};
			if (deps.Contains(null))
			{
				throw ProgramUtils.DependencyException(deps, depTypes);
			}
			cm.AddBounds(new Vector2Int(position.x, position.y));
			mm.AddBounds(new Vector2Int(position.x, position.y));
		}
		public void TileStart(Vector3Int position, Tilemap tilemap)
		{

		}
	}
}