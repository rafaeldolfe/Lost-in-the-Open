using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JetBrains.Annotations;
using Utils;

namespace Global
{
	public class GlobalGameManager : MonoBehaviour
	{
        public GameObject playerKing;
        public List<GameObject> playerUnits;

        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;
        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        [UsedImplicitly]
		private void Awake()
		{
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
        }
		
		/// <summary>
		/// Start is called just before any of the Update methods is called the first time.
		/// </summary>
		[UsedImplicitly]
		private void Start()
		{
			
		}
	}
}