using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class PauseService
{
    public static int TEXT_PAUSE = 4;
    public static int MENU_PAUSE = 5;

    readonly static List<int> activatedPauseLevels = new List<int>();
    public static void Unpause()
    {
        activatedPauseLevels.Clear();
    }
    public static int GetPauseLevel()
    {
        if (activatedPauseLevels.Count == 0)
        {
            return 0;
        }
        return activatedPauseLevels.Max();
    }
    public static void AddPauseLevel(int pauseLevel)
    {
        if (activatedPauseLevels.Contains(pauseLevel))
        {
            return;
        }
        activatedPauseLevels.Add(pauseLevel);
    }
    public static void RemovePauseLevel(int pauseLevel)
    {
        activatedPauseLevels.Remove(pauseLevel);
    }
    public static bool IsLevelPaused(int pauseLevel)
    {
        return GetPauseLevel() >= pauseLevel;
    }



    public static void _LogAllPauseLevels()
    {
        foreach(int level in activatedPauseLevels)
        {
            Debug.Log($"level: {level}");
        }
    }
}
