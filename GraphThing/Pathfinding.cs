using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphThing
{
    public enum HeuristicMethod
    {
        /// <summary>
        /// Manhatten distance, more commonly used in games.
        /// </summary>
        Manhatten,
        /// <summary>
        /// Euclidean distance.
        /// </summary>
        Euclidean
    }

    public interface INode
    {
        int X { get; set; }
        int Y { get; set; }

        INode Parent { get; set; }
        IEnumerable<INode> Neighbours { get; set; }

        bool Checked { get; set; } // if the node has been checked
        bool Closed { get; set; } // if the node has been closed
        int ID { get; set; } // used for comparing nodes

        float H { get; set; } // Heuristic value.
        float G { get; set; } // Weight or move cost.
        float F { get; } // Sum of the heuristic and weight.
    }

    /// <summary>
    /// Pathfinding class implementing the A* pathfinding algorithm.
    /// </summary>
    /// <typeparam name="T">Class or struct implementing the INode interface</typeparam>
    public class Pathfinding<T>
        where T : INode
    {
        public HeuristicMethod HeuristicMethod { get; set; }
        public bool DiagonalMovement { get; set; }

        public Pathfinding()
            : this(HeuristicMethod.Euclidean, true)
        { }

        public Pathfinding(HeuristicMethod method, bool diagonal)
        {
            HeuristicMethod = method;
            DiagonalMovement = diagonal;
        }

        /// <summary>
        /// Calculates the path from a start node to an end node using a provided set of nodes.
        /// Returns null if no path is found.
        /// </summary>
        /// <param name="startNode">Startnode</param>
        /// <param name="targetNode">Endnode</param>
        /// <param name="nodes">Nodes to search</param>
        public IEnumerable<T> CalculatePath(T startNode, T targetNode, IEnumerable<T> nodes)
        {
            T currentNode = default(T);
            var openList = new List<T>();

            // Set the heuristic value for all the nodes
            nodes = SetHeuristics(HeuristicMethod, nodes, targetNode);

            // Initially add the startnode to the openlist
            openList.Add(startNode);

            // Continue as long as there is any nodes left
            while (openList.Any())
            {
                // find the current node with the lowest f cost
                currentNode = openList[LocateLowestFIndex(openList)];

                // if we find the target node we return the traceback
                if (currentNode.ID == targetNode.ID)
                    return TracebackPath(targetNode);

                // if the node is not closed, close it
                // and remove it from the open list
                if (!currentNode.Closed)
                {
                    openList.Remove(currentNode);
                    currentNode.Closed = true;
                }

                for (int i = 0; i < currentNode.Neighbours.Count(); i++)
                {
                    var neighbour = currentNode.Neighbours.ToArray()[i];

                    if (neighbour.Closed) continue;

                    // calculate a new g score using the distance betweent the neigbours
                    var ngscore = currentNode.G +
                        (HeuristicMethod == HeuristicMethod.Euclidean
                            ? Euclidean(currentNode, neighbour)
                            : Manhatten(currentNode, neighbour));

                    // if the neighbour has never been checked, or
                    // it's new g score is better, check it
                    if (!neighbour.Checked || ngscore < neighbour.G)
                    {
                        neighbour.Parent = currentNode;
                        neighbour.G = ngscore;

                        if (!neighbour.Checked)
                        {
                            // if it's not checked, add it to the openlist
                            neighbour.Checked = true;
                            openList.Add((T)neighbour);
                        }
                        else
                        {
                            // if the g score is better, update it
                            var index = openList.IndexOf((T)neighbour);
                            openList[index] = (T)neighbour;
                        }
                    }
                }
            }

            // No path was found
            return null;
        }

        private static List<T> SetHeuristics(HeuristicMethod method, IEnumerable<T> nodes, T targetNode)
        {
            var temp = nodes.ToList<T>();
            for (int i = 0; i < temp.Count; i++)
                switch (method)
                {
                    case HeuristicMethod.Euclidean:
                        temp[i].H = Euclidean(temp[i], targetNode);
                        break;
                    case HeuristicMethod.Manhatten:
                        temp[i].H = Manhatten(temp[i], targetNode);
                        break;
                }
            return temp;
        }

        private static float Euclidean(INode node, INode targetNode)
        {
            var dx = Math.Abs(node.X - targetNode.X);
            var dy = Math.Abs(node.Y - targetNode.Y);

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private static float Manhatten(INode node, INode targetNode)
        {
            var dx = Math.Abs(node.X - targetNode.X);
            var dy = Math.Abs(node.Y - targetNode.Y);

            return (float)(dx + dy);
        }

        private static List<T> TracebackPath(T node)
        {
            // it's important to add the node to the list
            // before the loop begins
            var path = new List<T>() { node };

            // for as long as there is a parent, follow the path
            while (node.Parent != null)
            {
                var n = node.Parent;
                path.Add((T)node.Parent);
                node = (T)n;
            }
            return path;
        }

        private static int LocateLowestFIndex(IEnumerable<T> list)
        {
            // find the lowest f cost in the given list
            INode lowest = (INode)list.First();

            foreach (var node in list)
            {
                if (node.F < lowest.F) lowest = (T)node;
            }

            // we only return the index
            var index = list.ToList().IndexOf((T)lowest);
            return index;
        }
    }
}
