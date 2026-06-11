using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;

namespace SettlersClone.Pathfinding
{
    public static class AStarPathfinder
    {
        private class Node
        {
            public HexCell Cell;
            public Node    Parent;
            public float   G, H; // cost from start, heuristic
            public float   F => G + H;
        }

        public static List<HexCell> FindPath(HexCell start, HexCell goal)
        {
            if (start == null || goal == null || !goal.IsPassable()) return null;

            var open   = new SortedList<float, Node>(new DuplicateKeyComparer());
            var closed = new HashSet<HexCell>();
            var nodeMap = new Dictionary<HexCell, Node>();

            var startNode = new Node { Cell = start, G = 0, H = Heuristic(start, goal) };
            open.Add(startNode.F, startNode);
            nodeMap[start] = startNode;

            while (open.Count > 0)
            {
                var current = open.Values[0];
                open.RemoveAt(0);

                if (current.Cell == goal)
                    return ReconstructPath(current);

                closed.Add(current.Cell);

                for (int d = 0; d < 6; d++)
                {
                    HexCell neighbour = current.Cell.GetNeighbour(d);
                    if (neighbour == null || closed.Contains(neighbour) || !neighbour.IsPassable())
                        continue;

                    float tentativeG = current.G + neighbour.MovementCost();

                    if (!nodeMap.TryGetValue(neighbour, out var nNode))
                    {
                        nNode = new Node { Cell = neighbour, G = float.MaxValue, H = Heuristic(neighbour, goal) };
                        nodeMap[neighbour] = nNode;
                    }

                    if (tentativeG < nNode.G)
                    {
                        nNode.Parent = current;
                        nNode.G = tentativeG;
                        if (!open.ContainsValue(nNode))
                            open.Add(nNode.F, nNode);
                    }
                }
            }
            return null; // no path found
        }

        private static float Heuristic(HexCell a, HexCell b) =>
            a.Coordinates.DistanceTo(b.Coordinates);

        private static List<HexCell> ReconstructPath(Node node)
        {
            var path = new List<HexCell>();
            while (node != null)
            {
                path.Add(node.Cell);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        private class DuplicateKeyComparer : IComparer<float>
        {
            public int Compare(float x, float y) => x <= y ? -1 : 1;
        }
    }
}
