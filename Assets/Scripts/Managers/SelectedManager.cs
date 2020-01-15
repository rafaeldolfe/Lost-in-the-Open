using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SelectedManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private FactionManager fm;

    public GameObject tileHighlight;

    public Selected selected;

    private List<GridContainer> highlightedTiles;
    private Color prevColor;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(fm = FindObjectOfType(typeof(FactionManager)) as FactionManager);
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        this.highlightedTiles = new List<GridContainer>();
    }

    void Start()
    {
        gem.StartListening("Death", DeathHandler);
        gem.StartListening("ActorIsDone", ActorIsDoneHandler);
        gem.StartListening("EndTurn", EndTurnHandler);
    }
    void OnDestroy()
    {
        gem.StopListening("Death", DeathHandler);
        gem.StopListening("ActorIsDone", ActorIsDoneHandler);
        gem.StopListening("EndTurn", EndTurnHandler);
    }

    public void HandleSelectedActor()
    {
        if (selected == null || selected.ah.GetStatus() == "Busy" || selected.ah.IsCurrentAbilityDone())
        {
            UnhighlightPath();
            return;
        }
        Color color = selected.ah.GetHighlightColor();
        List<GridContainer> path = GetGridContainersFromPathNodes(selected.ah.GetTilesWithinRange());
        HighlightPath(path, color);
    }
    public void Select(GameObject actor)
    {
        Unselect();
        TriggerSetAbilityBar(actor);
        TriggerSetPortrait(actor);

        selected = new Selected(actor);
        selected.ah.SetFirstAvailableAbility();
    }
    public void Unselect()
    {
        if (selected != null)
        {
            gem.TriggerEvent("ResetAbilityBar", selected.go);
            gem.TriggerEvent("ResetPortrait", selected.go);
            selected.RemoveHighlight();
            selected = null;
        }
    }
    public void HighlightPath(List<GridContainer> newTiles, Color highlightColor)
    {

        if (CompareGridContainerLists(newTiles, highlightedTiles) && prevColor.Equals(highlightColor))
        {
            return;
        }

        UnhighlightPath();

        foreach (GridContainer gc in newTiles)
        {
            GameObject tileHighlightInstance = UnityEngine.Object.Instantiate(tileHighlight, tileHighlight.transform.position + new Vector3(gc.x, 0, gc.z), Quaternion.identity);
            tileHighlightInstance.GetComponent<Renderer>().GetComponent<Renderer>().material.color = highlightColor;
            gc.AddGameObject(tileHighlightInstance);
        }
        highlightedTiles = newTiles;
        prevColor = highlightColor;
    }
    public void UnhighlightPath()
    {
        if (highlightedTiles.Count == 0)
        {
            return;
        }
        foreach (GridContainer gcon in highlightedTiles)
        {
            foreach (GameObject go in gcon.gos)
            {
                if (go.tag == "Highlight")
                {
                    UnityEngine.Object.Destroy(go);
                    gcon.RemoveGameObject(go);
                    break;
                }
            }
        }
        highlightedTiles = new List<GridContainer>();
    }
    public void UseAbility(int x, int z)
    {
        if (selected != null && selected.go.GetComponent<Faction>().faction == "Player")
        {
            selected.ah.UseAbility(x, z);
        }
    }
    public void SetAbility(int index)
    {
        if (selected != null)
        {
            selected.ah.SetAbility(index);
        }
    }
    private void SelectFirstAvailablePlayerActor()
    {
        List<GameObject> factionMembers = fm.GetFaction("Player");
        foreach (GameObject actor in factionMembers)
        {
            if (actor.GetComponent<AbilitiesHandler>().IsAnyAbilityLeft() == true)
            {
                Select(actor);
                selected.ah.SetFirstAvailableAbility();
                return;
            }
        }
        gem.TriggerEvent("OfferEndTurn", gameObject);
        Unselect();
    }
    private void DeathHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (selected != null && selected.go == null)
        {
            Unselect();
        }
    }
    private void ActorIsDoneHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        SelectFirstAvailablePlayerActor();
    }
    private void EndTurnHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        SelectFirstAvailablePlayerActor();
    }
    private void TriggerSetAbilityBar(GameObject actor)
    {
        if (actor.GetComponent<AbilitiesHandler>() == null)
        {
            return;
        }
        List<object> abilityBarParams = actor.GetComponent<AbilitiesHandler>().GetAbilities().Select(p => (object)p).ToList();
        gem.TriggerEvent("SetAbilityBar", actor, abilityBarParams);
    }
    private void TriggerSetPortrait(GameObject actor)
    {
        Actor actorComp = actor.GetComponent<Actor>();
        Health healthComp = actor.GetComponent<Health>();
        PrimaryResource primaryResComp = actor.GetComponent<PrimaryResource>();
        List<object> portraitParams = new List<object>();

        portraitParams.Add(actorComp.name);
        portraitParams.Add(actorComp.portrait);
        if (healthComp != null)
        {
            portraitParams.Add(healthComp.maxHealth);
            portraitParams.Add(healthComp.health);
        }
        if (primaryResComp != null)
        {
            portraitParams.Add(primaryResComp.color);
            portraitParams.Add(primaryResComp.maxResource);
            portraitParams.Add(primaryResComp.resource);
        }
        gem.TriggerEvent("SetPortrait", actor, portraitParams);
    }
    private bool CompareGridContainerLists(List<GridContainer> prev, List<GridContainer> curr)
    {
        if (prev.Count != curr.Count)
            return false;
        for (int i = 0; i < prev.Count; i++)
        {
            if (prev[i] != curr[i])
                return false;
        }
        return true;
    }
    private List<GridContainer> GetGridContainersFromPathNodes(List<PathNode> path)
    {
        return path.ConvertAll<GridContainer>(p => p.parent);
    }
}

public class Selected
{
    private const float highlightAmount = 0.2f;

    public GameObject go;
    public AbilitiesHandler ah;

    private Color originalColor;
    private Color newColor;


    public Selected(GameObject go)
    {
        this.go = go;
        this.originalColor = go.transform.GetComponent<Renderer>().material.color;
        this.newColor = go.GetComponent<Renderer>().material.color;
        this.ah = go.GetComponent<AbilitiesHandler>();

        ApplyHighlight();
    }
    public void RemoveHighlight()
    {
        if (go == null)
        {
            return;
        }
        this.go.transform.GetComponent<Renderer>().material.color = originalColor;
    }
    private void ApplyHighlight()
    {
        this.newColor.r = newColor.r + highlightAmount;
        this.newColor.g = newColor.g + highlightAmount;
        this.newColor.b = newColor.b + highlightAmount;
        this.go.transform.GetComponent<Renderer>().material.color = newColor;
    }
}
