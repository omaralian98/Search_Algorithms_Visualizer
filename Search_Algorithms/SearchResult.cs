namespace Search_Algorithms;

public class SearchResult<T>
{
    public List<T> Steps { get; set; } = [];
    public long VisitedNodes { get; set; }
    public long DiscoveredNodes { get; set; }
}
