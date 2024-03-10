namespace Search_Algorithms.Algorithms;

public class Hill_Climbing
{
    public delegate int MyFunction(ISearchable game);
    public static async Task<SearchResult<ISearchable>> FindPath(ISearchable initial, MyFunction heuristic, int maxIterations = 1000, int delay = 0, CancellationToken token = default)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        ISearchable current = initial;

        ISearchable? result = await Task.Run(() =>
        {
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                Thread.Sleep(delay);
                if (heuristic(current) == 0)
                {
                    return current;
                }

                var neighbours = current.GetAllPossibleStates().ToArray();
                ISearchable? bestNeighbour = null;

                current.State = SearchState.Visited;
                VisitedNodes++;

                for (int i = 0; i < neighbours.Length; i++)
                {
                    if (neighbours[i].State == SearchState.Visited)
                    {
                        continue;
                    }
                    bestNeighbour ??= neighbours[i];

                    if (heuristic(neighbours[i]) < heuristic(bestNeighbour))
                    {
                        bestNeighbour = neighbours[i];
                    }
                    neighbours[i].State = SearchState.Discovered;
                    DiscoveredNodes++;
                }
                if (bestNeighbour is null) break;
                bestNeighbour.Parent = current;
                current = bestNeighbour;
            }
            return null;
        });

        return new SearchResult<ISearchable>
        {
            Steps = result?.ConstructPath() ?? [],
            DiscoveredNodes = DiscoveredNodes,
            VisitedNodes = VisitedNodes
        };
    }
}
