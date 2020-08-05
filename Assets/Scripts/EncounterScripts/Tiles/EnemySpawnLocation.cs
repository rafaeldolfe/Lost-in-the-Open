using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Utils;

namespace Encounter
{
	[CreateAssetMenu]
	public class EnemySpawnLocation : Tile, ILifecycleTile
	{
		private SpawnManager spm;
		public string type;

		public void TileAwake(Vector3Int position, Tilemap tilemap)
		{
			if (!Constants.UNIT_TYPES.Contains(type))
			{
				throw new Exception(string.Format("Invalid property: player spawn location type {0} is not one of the valid ones", type));
			}
			List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
			List<MonoBehaviour> deps = new List<MonoBehaviour>
		{
			(spm = FindObjectOfType(typeof(SpawnManager)) as SpawnManager),
		};
			if (deps.Contains(null))
			{
				throw ProgramUtils.DependencyException(deps, depTypes);
			}
			spm.AddEnemySpawnLocation(new Vector2Int(position.x, position.y), type);
		}
		public void TileStart(Vector3Int position, Tilemap tilemap)
		{

		}
	}
}