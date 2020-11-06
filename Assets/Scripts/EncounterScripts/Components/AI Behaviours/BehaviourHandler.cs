using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class Analysis
    {
        public Behaviour behaviour;
        public List<List<Decision>> coursesOfAction;

        public Analysis(Behaviour behaviour, List<List<Decision>> coursesOfAction)
        {
            this.behaviour = behaviour;
            this.coursesOfAction = coursesOfAction;
        }
    }

    [RequireComponent(typeof(AbilitiesHandler))]
    [RequireComponent(typeof(Position))]
    public class BehaviourHandler : MonoBehaviour
    {
        private FactionManager fm;
        private Pathfinding pf;
        private AbilitiesHandler ah;
        private Position pos;

        public List<string> behaviourNames;
        private List<Behaviour> behaviours;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (ah = gameObject.GetComponent<AbilitiesHandler>()),
            (pos = gameObject.GetComponent<Position>()),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            CacheBehaviours();
            if (behaviours == null || behaviours.Count == 0)
            {
                throw new Exception("Expected Behaviour component(s) to be attached to gameObject, found none");
            }
        }

        public List<Behaviour> GetBehaviours()
        {
            return behaviours;
        }


        public List<Analysis> GetAnalyses()
        {
            List<Analysis> analyses = behaviours
                .Select(behaviour => behaviour.GetAnalysis(ah.GetAbilities()))
                .ToList();
            return analyses;
        }
        public IEnumerator Execute(List<Decision> decisions)
        {
            foreach (Decision decision in decisions)
            {
                yield return decision.ability.UseAbility(decision.path);
                while (decision.ability.Status() == "Busy")
                    yield return new WaitForSeconds(0.25f);
            }
        }
        private void CacheBehaviours()
        {
            behaviours = behaviourNames.ConvertAll<Behaviour>(be => (Behaviour)gameObject.GetComponent(be));
        }
        /// <summary>
        /// Returns the range of the first (primary) movement ability.
        /// </summary>
        /// <returns></returns>
        public int GetMoveRange()
        {
            foreach (Ability ability in ah.GetAbilities())
            {
                if (ability is IMovement)
                {
                    return (int) Math.Ceiling(ability.range);
                }
            }
            return 5;
        }
        public int GetAttackRange()
        {
            foreach (Ability ability in ah.GetAbilities())
            {
                if (ability is IOffensive)
                {
                    return (int)Math.Ceiling(ability.range);
                }
            }
            return 5;
        }

        internal List<(List<Decision>, float)> _GetEvaluationsOfFirstBehaviour()
        {
            Behaviour behaviour = behaviours[0];
            return behaviour._GetEvaluations();
        }
    }
}