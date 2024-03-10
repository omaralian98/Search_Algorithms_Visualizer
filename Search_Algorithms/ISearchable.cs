namespace Search_Algorithms;


public enum SearchState
{
    Default,
    Discovered,
    Visited,
    Path
}


public interface ISearchable
{
    public ISearchable? Parent { get; set; }
    public bool IsOver();
    public IEnumerable<ISearchable> GetAllPossibleStates();
    public SearchState State { get; set; }
    public string ToString();
}
