using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class Constants : MonoBehaviour
    {
        public const int SIZE_OF_ABILITY_BAR = 5;
        public const int DOMAIN_KNOWLEDGE_RADIUS = 5;
        public const int NEARBY_RADIUS = 5;
        public const float TILE_OFFSET = 0.5f;
        public static List<string> UNIT_TYPES = new List<string>
    {
        "Melee",
        "Ranged",
        "King"
    };

        public enum STANCES
        {
            Default,
            Aggressive,
            Defensive,
            Ranged,
            Special,
            Fleeing
        }
    }
}
