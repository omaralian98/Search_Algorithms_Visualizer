using Search_Algorithms;

namespace SearchAlgorithms.Algorithms;

public class Depth_First_Search<TSearch> where TSearch : ISearchable
{
    /// <summary>
    /// This function find the shortest path of any game using DFS.
    /// </summary>
    /// <param name="initial">The initial state</param>
    /// <returns>
    /// Tuple contains 3 items.
    /// <br></br>
    /// List of TSearch containing the moves of the shortest path.
    /// <br></br>
    /// The number of discovered nodes.
    /// <br></br>
    /// The number of visited nodes.
    /// </returns>
    public static (List<TSearch>, long, long) FindPath(TSearch initial)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        Stack<(TSearch, List<TSearch>)> queue = [];
        HashSet<string> visited = [];

        queue.Push((initial, []));
        visited.Add(initial.ToString());
        while (queue.Count > 0)
        {
            var (current, path) = queue.Pop();
            if (current.IsOver()) return (path, DiscoveredNodes, VisitedNodes); // Return the path when the solution is found
            VisitedNodes++;
            foreach (var next in current.GetAllPossibleStates())
            {
                if (!visited.Contains(next.ToString()))
                {
                    visited.Add(next.ToString());
                    //Copies the previous path and adds next to the new path
                    List<TSearch> newPath = [.. path, (TSearch)next];
                    queue.Push(((TSearch, List<TSearch>))(next, newPath));
                    DiscoveredNodes++;
                }
            }
        }
        return ([], DiscoveredNodes, VisitedNodes); //Return empty List if no solution is found
    }
}