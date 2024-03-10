namespace Search_Algorithms.Algorithms;

public class Greedy_Best_First_Search
{
    public delegate int MyFunction(ISearchable game);

    /// <summary>
    /// This function finds the shortest path for any game state using Greedy Best First Search.
    /// </summary>
    /// <param name="initial">The initial state of the game</param>
    /// <param name="heuristic">the heuristic to be used</param>
    /// <returns>SearchResult</returns>
    /// <exception cref="OperationCanceledException"></exception>
    public static async Task<SearchResult<ISearchable>> FindPath(ISearchable initial, MyFunction heuristic, int delay = 0, CancellationToken token = default)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        SortedList<int, ISearchable> list = new(new DuplicateKeyComparer<int>());
        HashSet<string> visited = [];
        Dictionary<string, int> visitedWithCost = [];

        list[0] = initial;
        visited.Add(initial.ToString());
        visitedWithCost[initial.ToString()] = 0;
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
                    int newcost = heuristic(next);
                    if ((visited.Contains(newstr) && visitedWithCost[newstr] > newcost) || !visited.Contains(newstr))
                    {
                        next.State = SearchState.Discovered;
                        visitedWithCost[newstr] = newcost;
                        visited.Add(newstr);
                        list[newcost] = next;
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