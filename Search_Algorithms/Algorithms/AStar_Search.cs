namespace Search_Algorithms.Algorithms;

public class AStarSearch
{
    public delegate int MyFunction(ISearchable game);

    /// <summary>
    /// This function finds the shortest path for any game state using A*.
    /// </summary>
    /// <param name="initial">The initial state of the game</param>
    /// <param name="heuristic">the heuristic to be used</param>
    /// <returns>SearchResult</returns>
    /// <exception cref="OperationCanceledException"></exception>
    public async Task<SearchResult<ISearchable>> FindPath(ISearchable initial, MyFunction heuristic,int delay = 0, CancellationToken token = default)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        SortedList<int, ISearchable> list = new(new DuplicateKeyComparer<int>());
        HashSet<string> visited = [];
        Dictionary<string, int> visitedWithCost = [];
        Dictionary<ISearchable, int> g = [];

        list[0] = initial;
        visited.Add(initial.ToString());
        visitedWithCost[initial.ToString()] = 0;
        g[initial] = 0;
        initial.Parent = null;
        Task del = Task.Delay(delay, token);
        ISearchable result = await Task.Run(() =>
        {
            while (list.Count > 0)
            {
                Thread.Sleep(delay);
                token.ThrowIfCancellationRequested();
                VisitedNodes++;
                ISearchable current = list.Values[0];
                current.State = SearchState.Visited;
                if (current.IsOver())
                {
                    return Task.FromResult(current);
                }

                list.RemoveAt(0);

                foreach (ISearchable next in current.GetAllPossibleStates())
                {
                    string newstr = next.ToString();
                    int newcost = heuristic(next) + g[current] + 1;
                    if ((visited.Contains(newstr) && visitedWithCost[newstr] > newcost) || !visited.Contains(newstr))
                    {
                        next.State = SearchState.Discoverd;
                        visitedWithCost[newstr] = newcost;
                        visited.Add(newstr);
                        list[newcost] = next;
                        g[next] = g[current] + 1;
                        DiscoveredNodes++;
                        next.Parent = current;
                    }
                }
            }
            throw new Exception("Operation Failed\nCouldn't find the shortest path");
        });

        return new SearchResult<ISearchable>
        {
            Steps = result.ConstructPath(),
            DiscoveredNodes = DiscoveredNodes,
            VisitedNodes = VisitedNodes
        };
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

public static class Extension
{

    public static List<ISearchable> ConstructPath(this ISearchable init)
    {
        if (init is null) return [];
        var path = new List<ISearchable>();
        while (init.Parent is not null)
        {
            init.State = SearchState.Path;
            path.Add(init);
            init = init.Parent;
        }
        init.State = SearchState.Path;
        path.Add(init);
        path.Reverse();
        return path;
    }
}