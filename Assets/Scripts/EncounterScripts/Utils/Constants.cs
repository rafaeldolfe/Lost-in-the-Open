using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class Constants : MonoBehaviour
    {
        public static float MOVE_STRAIGHT_COST = 1.0f;
        public static float MOVE_DIAGONAL_COST = 1.41421356237f;
        public const int SIZE_OF_ABILITY_BAR = 5;
        public const int DOMAIN_KNOWLEDGE_RADIUS = 10;
        public const int NEARBY_RADIUS = 10;
        public const float TILE_OFFSET = 0.5f;
        public static List<string> UNIT_TYPES = new List<string>
    {
        "Melee",
        "Ranged",
        "King"
    };
        public static Color WITHIN_RANGE_HIGHLIGHT_COLOR = new Color(0, 0.46f, 0.71f, 0.8f);

        public static float WITHIN_RANGE_ALPHA = 0.8f;

        public enum STANCES
        {
            Default,
            Aggressive,
            Defensive,
            Ranged,
            Special,
            Fleeing
        }

        public static float ActorProximityFunc(float dist)
        {
            if (dist < 2)
            {
                return 1.00f - Mathf.Pow((dist - 1), 0.5f) * 0.3f;
            }
            if (dist < 3)
            {
                return 0.70f - Mathf.Pow((dist - 2), 0.5f) * 0.25f;
            }
            if (dist < 4)
            {
                return 0.45f - Mathf.Pow((dist - 3), 0.5f) * 0.20f;
            }
            if (dist < 5)
            {
                return 0.25f - Mathf.Pow((dist - 4), 0.5f) * 0.15f;
            }
            if (dist < 6)
            {
                return 0.10f - Mathf.Pow((dist - 5), 0.5f) * 0.10f;
            }
            if (dist >= 6)
            {
                return 0.10f;
            }
            return 0;
        }
    }
}
