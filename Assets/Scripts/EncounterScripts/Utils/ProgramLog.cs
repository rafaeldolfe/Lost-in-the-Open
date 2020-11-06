using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using Utils;

namespace Encounter
{
    public class ProgramLog : MonoBehaviour
    {
        private GlobalEventManager gem;

        private static List<Event> evlist = new List<Event>();

        void Awake()
        {
            gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
            if (gem == null)
            {
                List<MonoBehaviour> deps = new List<MonoBehaviour> { gem };
                List<Type> depTypes = new List<Type> { typeof(GlobalEventManager) };
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
        }
        void Start()
        {
            gem.StartListening("Move", LogMoveActor);
            gem.StartListening("Death", LogDeath);
            gem.StartListening("Attack", LogAttack);
            gem.StartListening("OfferEndTurn", LogOfferEndTurn);
            gem.StartListening("PlayerEndTurn", LogEndTurn);
            gem.StartListening("EnemyBeginTurn", LogBeginAITurn);
            gem.StartListening("EnemyEndTurn", LogEndAITurn);
        }
        void OnDestroy()
        {
            gem.StopListening("Move", LogMoveActor);
            gem.StopListening("Death", LogDeath);
            gem.StopListening("Attack", LogAttack);
            gem.StopListening("PlayerEndTurn", LogEndTurn);
            gem.StopListening("EnemyBeginTurn", LogBeginAITurn);
            gem.StopListening("EnemyEndTurn", LogEndAITurn);
        }

        public void LogMoveActor(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Event ev = new Event(invoker.name, String.Format("Move {0} from tile ({1},{2}) to ({3},{4})", invoker.name, x, y, tx, ty), parameters);

            Log(ev);
        }
        public void LogAttack(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Event ev = new Event(invoker.name, String.Format("Attack from tile ({0},{1}) to ({2},{3})", x, y, tx, ty), parameters);

            Log(ev);
        }
        public void LogDeath(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Event ev = new Event(invoker.name, String.Format("{0} at ({1},{2}) died", invoker.name, x, y), parameters);

            Log(ev);
        }
        public void LogOfferEndTurn(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Event ev = new Event(invoker.name, String.Format("Player prompted to end turn"), parameters);

            Log(ev);
        }
        public void LogEndTurn()
        {
            Event ev = new Event("GlobalEventManager", String.Format("Player ended turn"), new List<object>());

            Log(ev);
        }
        public void LogBeginAITurn(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Event ev = new Event(invoker.name, String.Format("AI began turn"), parameters);

            Log(ev);
        }
        public void LogEndAITurn()
        {
            Event ev = new Event("GlobalEventManager", String.Format("AI ended turn"), new List<object>());

            Log(ev);
        }

        public string GetLog()
        {
            StringBuilder sb = new StringBuilder("Begin game\n");
            foreach (Event ev in evlist)
            {
                sb.Append(ev.ToString());
            }
            return sb.ToString();
        }
        private void Log(Event ev)
        {
            if (GlobalDebugManager.debug)
            {
                Debug.Log(ev.ToString());
            }
            evlist.Add(ev);
        }

        public class Event
        {
            public string name;
            public string desc;
            public List<object> parameters;

            public Event(string name, string desc, List<object> parameters)
            {
                this.name = name;
                this.desc = desc;
                this.parameters = parameters;
            }

            public override string ToString()
            {
                return this.name + ": " + this.desc + "\n";
            }
        }
    }
}