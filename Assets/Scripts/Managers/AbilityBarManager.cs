using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class AbilityBarManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private CameraManager cm;

    private List<SpriteRenderer> abilityImages;
    private List<Ability> abilities;
    private List<GameObject> abilityHighlights;


    void Awake()
    {
        gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
        cm = FindObjectOfType(typeof(CameraManager)) as CameraManager;
        if (gem == null || cm == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, cm };
            List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(CameraManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        CacheAbilityImages();
        CacheAbilityHighlights();
    }

    void Start()
    {
        cm.AttachToCamera(gameObject); 
        gem.StartListening("SetAbilityBar", SetAbilityBar);
        gem.StartListening("ResetAbilityBar", ResetAbilityBar);
        gem.StartListening("HighlightAbility", HighlightAbility);
    }
    void OnDestroy()
    {
        gem.StopListening("SetAbilityBar", SetAbilityBar);
        gem.StopListening("ResetAbilityBar", ResetAbilityBar);
        gem.StopListening("HighlightAbility", HighlightAbility);
    }

    private void CacheAbilityImages()
    {
        abilityImages = new List<SpriteRenderer>();
        Transform abilityBar = transform.GetChild(0);
        foreach (Transform child in abilityBar)
        {
            if (child.name.Contains("AbilityFrame"))
            {
                if (!child.GetChild(0).name.Contains("AbilityImage"))
                {
                    throw new System.Exception("Expected first child to contain substring AbilityImage, but it did not");
                }
                abilityImages.Add(child.GetChild(0).gameObject.GetComponent<SpriteRenderer>());
            }
        }
        if (abilityImages.Count != Constants.SIZE_OF_ABILITY_BAR)
        {
            throw new System.Exception("Number of AbilityFrame objects, and the constant SIZE_OF_ABILITY_BAR, are mismatched");
        }
    }
    private void CacheAbilityHighlights()
    {
        abilityHighlights = new List<GameObject>();
        Transform abilityBar = transform.GetChild(0);
        foreach (Transform child in abilityBar)
        {
            if (child.name.Contains("AbilityFrame"))
            {
                if (!child.GetChild(1).name.Contains("AbilityHighlight"))
                {
                    throw new System.Exception("Expected second child to contain substring AbilityHighlight, but it did not");
                }
                abilityHighlights.Add(child.GetChild(1).gameObject);
            }
        }
        if (abilityHighlights.Count != Constants.SIZE_OF_ABILITY_BAR)
        {
            throw new System.Exception("Number of AbilityFrame objects, and the constant SIZE_OF_ABILITY_BAR, are mismatched");
        }
    }
    private void SetAbilityBar(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        foreach(object ability in parameters)
        {
            if (!ability.GetType().IsSubclassOf(typeof(Ability)))
            {
                throw new System.Exception("Expected list of abilities, found something of type " + ability.GetType());
            }
        }
        if (parameters.Count > Constants.SIZE_OF_ABILITY_BAR)
        {
            throw new System.Exception("Expected fewer abilities in parameters than " + Constants.SIZE_OF_ABILITY_BAR + ", found " + parameters.Count);
        }
        ResetAbilityBar();
        ShowAbilityBar();
        abilities = parameters.Select(p => (Ability)p).ToList();
        for (int i = 0; i < abilities.Count; i++)
        {
            SpriteRenderer sRenderer = abilityImages[i];
            sRenderer.sprite = abilities[i].image;
        }
    }
    private void HideAbilityBar()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
            foreach (Transform secondchild in child.transform)
            {
                secondchild.gameObject.SetActive(false);
            }
        }
    }
    private void ShowAbilityBar()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            foreach (Transform secondchild in child.transform)
            {
                secondchild.gameObject.SetActive(true);
            }
        }
    }
    private void ResetAbilityBar()
    {
        foreach (SpriteRenderer sRend in abilityImages)
        {
            sRend.sprite = null;
        }
        foreach (GameObject go in abilityHighlights)
        {
            go.SetActive(false);
        }
        HideAbilityBar();
    }
    private void ResetAbilityBar(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz) { ResetAbilityBar(); }
    private void ResetHighlight()
    {
        foreach (GameObject abilityHighlight in abilityHighlights)
        {
            abilityHighlight.SetActive(false);
        }
    }
    private void HighlightAbility(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (gameObject)
        if (parameters.Count == 0)
        {
            throw new System.Exception("Expected list with 1 ability, found empty list");
        }
        if (parameters[0] == null)
        {
            ResetHighlight();
            return;
        }
        if (!parameters[0].GetType().IsSubclassOf(typeof(Ability)))
        {
            throw new System.Exception("Expected subclass of type Ability, found element of type " + parameters[0].GetType());
        }
        Ability ability = (Ability) parameters[0];

        for (int i = 0; i < abilities.Count; i++)
        {
            if (ability == abilities[i])
            {
                HighlightAbilitySlot(i);
                return;
            }
        }
    }
    public void HighlightAbilitySlot(int index)
    {
        foreach (GameObject abilityHighlight in abilityHighlights)
        {
            if (abilityHighlight.active)
            {
                abilityHighlight.SetActive(false);
            }
        }
        abilityHighlights[index].SetActive(true);
    }
    public GameObject GetAttachable()
    {
        return gameObject;
    }
}
