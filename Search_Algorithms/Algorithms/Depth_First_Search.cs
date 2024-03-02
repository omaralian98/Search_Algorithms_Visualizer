﻿using Search_Algorithms;
using Search_Algorithms.Algorithms;

namespace SearchAlgorithms.Algorithms;

public class Depth_First_Search
{
    /// <summary>
    /// This function find the shortest path of any game using DFS.
    /// </summary>
    /// <param name="initial">The initial state</param>
    /// <returns></returns>
    public async Task<SearchResult<ISearchable>> FindPath(ISearchable initial, int delay = 0, CancellationToken token = default)
    {
        long DiscoveredNodes = 1;
        long VisitedNodes = 1;
        Stack<ISearchable> queue = [];
        HashSet<string> visited = [];

        queue.Push(initial);
        visited.Add(initial.ToString());

        ISearchable? result = await Task.Run(() => 
        {
            while (queue.Count > 0)
            {
                Thread.Sleep(delay);
                var current= queue.Pop();
                current.State = SearchState.Visited;

                if (current.IsOver())
                {
                    return Task.FromResult(current);
                }
                VisitedNodes++;
                foreach (var next in current.GetAllPossibleStates())
                {
                    if (!visited.Contains(next.ToString()))
                    {
                        next.State = SearchState.Discoverd;
                        visited.Add(next.ToString());
                        queue.Push(next);
                        next.Parent = current;
                        DiscoveredNodes++;
                    }
                }
            }
            return null;
        });

        return new SearchResult<ISearchable>
        {
            Steps = result.ConstructPath(),
            DiscoveredNodes = DiscoveredNodes,
            VisitedNodes = VisitedNodes
        };
    }
}