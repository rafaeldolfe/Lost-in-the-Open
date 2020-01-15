using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public const int SIZE_OF_ABILITY_BAR = 5;

    public enum STANCES {
        Default,
        Aggressive,
        Defensive,
        Ranged,
        Special,
        Fleeing
    }
}
