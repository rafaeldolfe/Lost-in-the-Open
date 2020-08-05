using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public abstract class Behaviour : MonoBehaviour
    {
        public abstract Analysis GetAnalysis(List<Ability> abilities);
    }
}