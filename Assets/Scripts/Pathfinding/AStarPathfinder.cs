using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    private class Node
    {
        public Vector2Int Cell;
        public Node Parent;
        public int G;
        public int H;
        public int F => G + H;
    }

    private static readonly Vector2Int[] neighborBuffer = new Vector2Int[4];

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, bool avoidOccupants = false)
    {
        GridSystem grid = GridSystem.Instance;
        if (grid == null) return null;
        if (grid.GetTileType(goal) == TileType.Blocked) return null;
        if (avoidOccupants && grid.TryGetOccupant(goal, out _)) return null;
        if (start == goal) return new List<Vector2Int> { start };

        Dictionary<Vector2Int, Node> openMap = new();
        HashSet<Vector2Int> closed = new();

        Node startNode = new() { Cell = start, G = 0, H = Manhattan(start, goal) };
        openMap[start] = startNode;

        while (openMap.Count > 0)
        {
            Node current = LowestF(openMap);

            if (current.Cell == goal)
                return BuildPath(current);

            openMap.Remove(current.Cell);
            closed.Add(current.Cell);

            int neighborCount = grid.GetWalkableNeighborsNonAlloc(current.Cell, neighborBuffer, avoidOccupants);
            for (int i = 0; i < neighborCount; i++)
            {
                Vector2Int neighborCell = neighborBuffer[i];
                if (closed.Contains(neighborCell)) continue;

                int tentativeG = current.G + 1;

                if (openMap.TryGetValue(neighborCell, out Node existing))
                {
                    if (tentativeG < existing.G)
                    {
                        existing.G = tentativeG;
                        existing.Parent = current;
                    }
                }
                else
                {
                    openMap[neighborCell] = new Node
                    {
                        Cell = neighborCell,
                        Parent = current,
                        G = tentativeG,
                        H = Manhattan(neighborCell, goal)
                    };
                }
            }
        }

        return null;
    }

    private static Node LowestF(Dictionary<Vector2Int, Node> map)
    {
        Node best = null;
        foreach (Node n in map.Values)
        {
            if (best == null || n.F < best.F || (n.F == best.F && n.H < best.H))
                best = n;
        }
        return best;
    }

    private static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static List<Vector2Int> BuildPath(Node goalNode)
    {
        List<Vector2Int> path = new();
        Node current = goalNode;
        while (current != null)
        {
            path.Add(current.Cell);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }
}
