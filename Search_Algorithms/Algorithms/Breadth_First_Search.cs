using Search_Algorithms;

namespace SearchAlgorithms.Algorithms
{
    public class Breadth_First_Search<TSearch> where TSearch : ISearchable
    {
        /// <summary>
        /// This function finds the shortest path for any game state using BFS.
        /// </summary>
        /// <param name="initial">The initial state</param>
        /// <returns></returns>
        public (List<TSearch>, long, long) FindPath(TSearch initial)
        {
            long DiscoveredNodes = 1;
            long VisitedNodes = 1;
            Queue<(TSearch, List<TSearch>)> queue = new Queue<(TSearch, List<TSearch>)>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue((initial, new List<TSearch>()));
            visited.Add(initial.ToString());

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();

                if (current.IsOver()) return (path, DiscoveredNodes, VisitedNodes); // Return the path when the solution is found

                VisitedNodes++;

                foreach (var next in current.GetAllPossibleStates())
                {
                    if (!visited.Contains(next.ToString()))
                    {
                        visited.Add(next.ToString());

                        // Copies the previous path and adds next to the new path
                        List<TSearch> newPath = new List<TSearch>(path);
                        newPath.Add((TSearch)next);

                        queue.Enqueue(((TSearch, List<TSearch>))(next, newPath));
                        DiscoveredNodes++;
                    }
                }
            }

            return (new List<TSearch>(), DiscoveredNodes, VisitedNodes); // Return empty List if no solution is found
        }
    }
}