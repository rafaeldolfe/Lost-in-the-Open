/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Pathfinding : MonoBehaviour
{
    private MapGridManager mgm;

    private List<Vector2Int> directions;

    private MapGrid grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    void Awake()
    {
        mgm = FindObjectOfType(typeof(MapGridManager)) as MapGridManager;
        if (mgm == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { mgm };
            List<Type> depTypes = new List<Type> { typeof(MapGridManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }

        directions = new List<Vector2Int>();
        directions.Add(new Vector2Int(1, 0));
        directions.Add(new Vector2Int(0, 1));
        directions.Add(new Vector2Int(0, -1));
        directions.Add(new Vector2Int(-1, 0));
    }

    void Start()
    {
        grid = mgm.grid;
    }

    public List<PathNode> FindPath(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
    {
        if (pfconfig == null)
        {
            pfconfig = new PathfindingConfig();
        }
        if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
        {
            return null;
        }

        PathNode startNode = grid.GetGridObject(startX, startY).pn;
        PathNode endNode = grid.GetGridObject(endX, endY).pn;

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetHeight(); z++)
            {
                PathNode pathNode = grid.GetGridObject(x, z).pn;
                pathNode.gCost = 99999999;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            List<PathNode> neighbours;


            neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

            foreach (PathNode neighbourNode in neighbours)
            {
                if (closedList.Contains(neighbourNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        // Out of nodes on the openList
        return null;
    }
    public List<PathNode> FindPathNodesWithinRange(int startX, int startY, int range, PathfindingConfig pfconfig = null)
    {
        if (pfconfig == null)
        {
            pfconfig = new PathfindingConfig();
        }
        List<PathNode> openList = new List<PathNode> { GetNode(startX, startY) };
        List<PathNode> removeList = new List<PathNode>();
        List<PathNode> addList = new List<PathNode>();
        List<PathNode> finalList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();
        int counter = 0;

        while(counter < range)
        {
            foreach(PathNode node in openList)
            {
                closedList.Add(node);
                removeList.Add(node);
                List<PathNode> neighbours;

                neighbours = GetNeighbourListFiltered(node, pfconfig);

                foreach (PathNode neighbour in neighbours)
                {
                    if (closedList.Contains(neighbour)) continue;
                    if (openList.Contains(neighbour)) continue;
                    if (addList.Contains(neighbour)) continue;

                    finalList.Add(neighbour);
                    addList.Add(neighbour);
                }
            }
            foreach (PathNode node in removeList)
            {
                openList.Remove(node);
            }
            foreach (PathNode node in addList)
            {
                openList.Add(node);
            }
            removeList = new List<PathNode>();
            addList = new List<PathNode>();
            counter++;
        }
        return finalList;
    }
    
    public PathNode GetNode(int x, int z)
    {
        return grid.GetGridObject(x, z).pn;
    }
    public bool DoesPathNodeHavePlayerActor(PathNode p)
    {
        if (p.parent.actor == null)
        {
            return false;
        }
        if (p.parent.actor.GetComponent<Faction>() == null)
        {
            return false;
        }
        if (p.parent.actor.GetComponent<Faction>().faction != "Player")
        {
            return false;
        }
        return true;
    }
    public List<PathNode> GetPathToClosestActor(int fromX, int fromZ, List<GameObject> actors)
    {
        List<PathNode> closestPath = new List<PathNode>();
        int minDistance = int.MaxValue;
        foreach (GameObject actor in actors)
        {
            Position actorPos = actor.GetComponent<Position>();
            List<PathNode> path = FindPath(fromX, fromZ, actorPos.x, actorPos.z);
            if (path.Count - 1 < minDistance)
            {
                closestPath = path;
                minDistance = closestPath.Count - 1;
            }
        }
        return closestPath;
    }
    public PathNode GetClosestPlayerActorPosition(List<GameObject> actors, int x, int z)
    {
        return GetPathToClosestActor(x, z, actors).Last();
    }
    private List<PathNode> GetNeighbourListFiltered(PathNode currentNode, PathfindingConfig pfconfig)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        foreach (Vector2Int dir in directions)
        {
            if (currentNode.x + dir.x >= 0 && currentNode.x + dir.x < grid.GetWidth() &&
                currentNode.z + dir.y >= 0 && currentNode.z + dir.y < grid.GetHeight())
            {
                PathNode neighbour = GetNode(currentNode.x + dir.x, currentNode.z + dir.y);
                if (neighbour.CheckConditions(pfconfig))
                {
                    neighbourList.Add(neighbour);
                }
            }
        }

        return neighbourList;
    }
    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;
    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int zDistance = Mathf.Abs(a.z - b.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }
    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

}