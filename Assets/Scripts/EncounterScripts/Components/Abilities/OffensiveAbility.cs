using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public abstract class OffensiveAbility : Ability
    {
        public abstract float GetDamage();
    }
}
