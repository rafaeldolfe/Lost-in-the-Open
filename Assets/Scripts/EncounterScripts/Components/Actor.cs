using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Utils;
using Sirenix.OdinInspector;

[Serializable]
public class PlainSprite
{
    /// <summary>
    /// This exact sprite's name
    /// </summary>
    public string spriteName;
    /// <summary>
    /// The path to this prefab's location in the resource folder
    /// </summary>
    public string folder;

    public PlainSprite() {}

    public PlainSprite(string spriteName, string folder)
    {
        this.spriteName = spriteName;
        this.folder = folder;
    }

    /// <summary>
    /// Converts this PlainMesh class into its Unity Mesh equivalent
    /// </summary>
    /// <returns></returns>
    public Sprite GetUnityClass()
    {
        return Resources.Load($"{folder}/{spriteName}") as Sprite;
    }

    public override string ToString()
    {
        return $"path: {folder}/{spriteName}";
    }
}

namespace Encounter
{
    [RequireComponent(typeof(Position))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Faction))]
    [RequireComponent(typeof(AbilitiesHandler))]
    public class Actor : MonoBehaviour
    {
        public new string name;
        [SerializeField]
        private Sprite portrait = null;
        [ValidateInput("PlainSpriteMustLoadCorrectSprite", "The name and folder of the PlainSprite must correspond to the associated Sprite.")]
        [HideIf("PlainSpriteMustLoadCorrectSprite")]
        public PlainSprite portraitData;

        private GlobalEventManager gem;
        private Position pos;
        private Health hp;
        private Faction fac;
        private AbilitiesHandler ah;

        private bool PlainSpriteMustLoadCorrectSprite(PlainSprite plainSprite)
        {
            Sprite testSprite = (Sprite)AssetDatabase.LoadAssetAtPath($"Assets/Resources/{plainSprite.folder}/{plainSprite.spriteName}", typeof(Sprite));
            if (testSprite != portrait)
            {
                return false;
            }
            return true;
        }
        void Start()
        {
            gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
            pos = GetComponent<Position>();
            hp = GetComponent<Health>();
            fac = GetComponent<Faction>();
            ah = GetComponent<AbilitiesHandler>();
            if (gem == null || pos == null || hp == null || fac == null || ah == null)
            {
                List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, pos, hp, fac, ah };
                List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(Position), typeof(Health), typeof(Faction), typeof(AbilitiesHandler) };
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("Death", Death);
        }

        public void OnDestroy()
        {
            gem.StopListening("Death", Death);
        }

        public void Death(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (invoker != gameObject)
            {
                return;
            }
            // implement death animation thing
            Destroy(invoker);
        }
        public Sprite GetPortrait()
        {
            return portrait;
        }
    }
}