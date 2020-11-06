using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class Decision
    {
        public Ability ability;
        public List<PathNode> path;
        private IEnumerator pathGen;

        public Decision(Ability ability, List<PathNode> path)
        {
            this.ability = ability;
            this.path = path;
        }
    }
}