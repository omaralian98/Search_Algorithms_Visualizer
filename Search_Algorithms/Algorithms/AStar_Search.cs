using System.Collections.Generic;

namespace Search_Algorithms.Algorithms;
public class AStarSearch<TSearch> where TSearch : ISearchable
{
    public delegate int MyFunction(TSearch game);

    /// <summary>
    /// This function finds the shortest path for any game state using A*.
    /// </summary>
    /// <param name="initial">The initial state of the game</param>
    /// <param name="heuristic">the heuristic to be used</param>
    /// <returns>SearchResult</returns>
    /// <exception cref="OperationCanceledException"></exception>
    public async Task<SearchResult<TSearch>> FindPath(TSearch initial, MyFunction heuristic, CancellationToken token = default, int delay = 0)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        SortedList<int, TSearch> list = new(new DuplicateKeyComparer<int>());
        HashSet<string> visited = [];
        Dictionary<string, int> visitedWithCost = [];
        Dictionary<TSearch, int> g = [];

        list[0] = initial;
        visited.Add(initial.ToString());
        visitedWithCost[initial.ToString()] = 0;
        g[initial] = 0;
        initial.Parent = null;
        initial.IsVisited = true;
        Task del = Task.Delay(delay);
        TSearch? result = await Task.Run(() =>
        {
            while (list.Count > 0)
            {
                Thread.Sleep(delay);
                if (token.IsCancellationRequested) throw new TaskCanceledException("Operation was canceled.");
                VisitedNodes++;
                TSearch current = list.Values[0];
                current.IsVisited = true;
                if (current.IsOver())
                {
                    return Task.FromResult(current);
                }

                list.RemoveAt(0);

                foreach (TSearch next in current.GetAllPossibleStates())
                {
                    string newstr = next.ToString();
                    int newcost = heuristic(next) + g[current] + 1;
                    if ((visited.Contains(newstr) && visitedWithCost[newstr] > newcost) || !visited.Contains(newstr))
                    {
                        visitedWithCost[newstr] = newcost;
                        visited.Add(newstr);
                        list[newcost] = next;
                        g[next] = g[current] + 1;
                        DiscoveredNodes++;
                        next.Parent = current;
                    }
                }
            }
            return null;
        });
        return new SearchResult<TSearch>
        {
            Steps = ConstructPath(result),
            DiscoveredNodes = DiscoveredNodes,
            VisitedNodes = VisitedNodes
        };
    }

    private static List<TSearch> ConstructPath(TSearch init)
    {
        if (init is null) return [];
        var path = new List<TSearch>();
        while (init.Parent is not null)
        {
            path.Add(init);
            init = (TSearch)init.Parent;
        }
        path.Add(init);
        path.Reverse();
        return path;
    }
}

/// <summary>
/// Comparer for comparing two keys, handling equality as being greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1; // Handle equality as being greater. Note: this will break Remove(key) or
        else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            return result;
    }
    #endregion
}