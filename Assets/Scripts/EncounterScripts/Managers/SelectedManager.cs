using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Encounter
{
    public class SelectedManager : MonoBehaviour
    {
        public enum HighlightType
        {
            TilesWithinRange,
            TargetTiles,
        }

        private GlobalEventManager gem;
        private FactionManager fm;

        public GameObject tileHighlight;
        public GameObject tileHollowHighlight;

        public Selected selected;

        private List<Highlight> highlights = new List<Highlight>();
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

            gem.StartListening("Death", DeathHandler);
            gem.StartListening("ActorIsDone", ActorIsDoneHandler);
            gem.StartListening("PlayerEndTurn", EndTurnHandler);
            gem.StartListening("EnemyEndTurn", EndAITurnHandler);
        }

        void OnDestroy()
        {
            gem.StopListening("Death", DeathHandler);
            gem.StopListening("ActorIsDone", ActorIsDoneHandler);
            gem.StopListening("PlayerEndTurn", EndTurnHandler);
            gem.StopListening("EnemyEndTurn", EndAITurnHandler);
        }
        private void Update()
        {
            if (selected == null || selected.ah.GetStatus() == "Busy" || selected.ah.IsCurrentAbilityDone())
            {
                UnhighlightPath(HighlightType.TilesWithinRange);
                UnhighlightPath(HighlightType.TargetTiles);
                return;
            }
            Color abilityColor = selected.ah.GetHighlightColor();
            Color color = MixColors(abilityColor, Constants.WITHIN_RANGE_HIGHLIGHT_COLOR);
            List<GridContainer> tiles = GetGridContainersFromPathNodes(selected.ah.GetTilesWithinRange());
            HighlightTiles(tiles, color, HighlightType.TilesWithinRange, tileHollowHighlight);
        }
        private Color MixColors(Color a, Color b, float alpha = 1)
        {
            return new Color((a.r + b.r) / 2, (a.g + b.g) / 2, (a.b + b.b) / 2);
        }
        private void DeathHandler(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (selected != null && selected.go == null)
            {
                Unselect();
            }
        }
        private void ActorIsDoneHandler(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            SelectFirstAvailablePlayerActor();
        }
        private void EndTurnHandler()
        {
            Unselect();
        }
        private void EndAITurnHandler()
        {
            SelectFirstAvailablePlayerActor();
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
        public void HighlightTargetTile(GridContainer current)
        {
            if (highlights.Where(hl => hl.gc == current).Any())
            {
                HighlightTiles(selected.ah.GetTargetTiles(current.pn.x, current.pn.y), selected.ah.GetHighlightColor(), HighlightType.TargetTiles, tileHighlight);
            }
            else
            {
                UnhighlightPath(HighlightType.TargetTiles);
            }
        }
        public void HighlightTiles(List<GridContainer> newTiles, Color highlightColor, HighlightType newHighlightType, GameObject highlightPrefab)
        {

            if (CompareGridContainerLists(newTiles, highlightColor, newHighlightType))
            {
                return;
            }

            UnhighlightPath(newHighlightType);

            foreach (GridContainer gc in newTiles)
            {
                float z = newHighlightType == HighlightType.TargetTiles ? 0.1f : 0;
                GameObject tileHighlightInstance = Instantiate(highlightPrefab, highlightPrefab.transform.position + new Vector3(gc.x, gc.y, z), highlightPrefab.transform.rotation);
                TileHighlightScript script = tileHighlightInstance.GetComponent<TileHighlightScript>();
                if (script == null)
                {
                    throw ProgramUtils.MissingComponentException(typeof(TileHighlightScript));
                }
                script.SetColor(highlightColor);
                gc.AddGameObject(tileHighlightInstance);
                highlights.Add(new Highlight(gc, tileHighlightInstance, newHighlightType, highlightColor));
            }
        }
        public void UnhighlightAll()
        {
            if (highlights.Count == 0)
            {
                return;
            }
            foreach (Highlight highlight in highlights)
            {
                highlight.gc.RemoveGameObject(highlight.go);
                Destroy(highlight.go);
            }
            highlights = new List<Highlight>();
        }
        public void UnhighlightPath(HighlightType highlightType)
        {
            if (highlights.Count == 0)
            {
                return;
            }
            var filtered = highlights.Where(h => h.type == highlightType);
            foreach (Highlight highlight in filtered)
            {
                highlight.gc.RemoveGameObject(highlight.go);
                Destroy(highlight.go);
            }
            highlights = highlights.Where(h => h.type != highlightType).ToList();
        }
        public void UseAbility(int x, int y)
        {
            if (selected != null && selected.go.GetComponent<Faction>().faction == "Player")
            {
                StartCoroutine(selected.ah.UseAbility(x, y));
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
        private void TriggerSetAbilityBar(GameObject actor)
        {
            if (actor.GetComponent<AbilitiesHandler>() == null)
            {
                return;
            }
            List<object> abilityBarParams = actor.GetComponent<AbilitiesHandler>().GetActiveAbilities().ConvertAll(p => (object)p);
            gem.TriggerEvent("SetAbilityBar", actor, abilityBarParams);
        }
        private void TriggerSetPortrait(GameObject actor)
        {
            Actor actorComp = actor.GetComponent<Actor>();
            Health healthComp = actor.GetComponent<Health>();
            List<object> portraitParams = new List<object>();

            portraitParams.Add(actorComp.name);
            portraitParams.Add(actorComp.GetPortrait());
            if (healthComp != null)
            {
                portraitParams.Add(healthComp.maxHealth);
                portraitParams.Add(healthComp.health);
            }
            gem.TriggerEvent("SetPortrait", actor, portraitParams);
        }
        private bool CompareGridContainerLists(List<GridContainer> newTiles, Color color, HighlightType newHighlightType)
        {
            List<GridContainer> filtered = highlights.Where(h => h.color == color && h.type == newHighlightType).ToList().ConvertAll(f => f.gc);
            if (newTiles.Count != filtered.Count)
                return false;
            foreach (GridContainer gc in newTiles)
            {
                if (!filtered.Contains(gc))
                {
                    return false;
                }
            }
            return true;
        }
        private List<GridContainer> GetGridContainersFromPathNodes(List<PathNode> path)
        {
            return path.ConvertAll<GridContainer>(p => p.parent);
        }
        private class Highlight
        {
            public GridContainer gc;
            public GameObject go;
            public HighlightType type;
            public Color color;

            public Highlight(GridContainer gc, GameObject go, HighlightType type, Color color)
            {
                this.gc = gc;
                this.go = go;
                this.type = type;
                this.color = color;
            }
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
}