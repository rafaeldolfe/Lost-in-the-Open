using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Encounter;
using ThreadedPathfinding;

public class PathfindingResolution
{
    public PathfindingResult Result;
    public List<PathNode> Path;
    public PathFound Callback;
    public int Id;
    public int FramesToLive;
}
