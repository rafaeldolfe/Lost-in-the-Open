﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Utils;

namespace Encounter
{
	[CreateAssetMenu]
	public class CameraStart : Tile, ILifecycleTile
	{
		private CameraManager cm;
		public void TileAwake(Vector3Int position, Tilemap tilemap)
		{
			List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
			List<MonoBehaviour> deps = new List<MonoBehaviour>
		{
			(cm = FindObjectOfType(typeof(CameraManager)) as CameraManager),
		};
			if (deps.Contains(null))
			{
				throw ProgramUtils.DependencyException(deps, depTypes);
			}
			cm.SetPosition(position.x, position.y);
		}
		public void TileStart(Vector3Int position, Tilemap tilemap)
		{

		}
	}
}