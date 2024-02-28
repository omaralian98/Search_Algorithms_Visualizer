
namespace Search_Algorithms.Games;

public class Shortest_Path : ISearchable
{
    public int X { get; set; }
    public int Y { get; set; }
    public Shortest_Path? Goal { get; set; } = null;
    public ISearchable? Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IEnumerable<ISearchable> GetAllPossibleStates()
    {
        throw new NotImplementedException();
        ;
    }

    public bool IsOver()
    {
        if (Goal is null) return true;
        return this == Goal;
    }
    public override string ToString()
    {
        return base.ToString();
    }
    public static bool operator ==(Shortest_Path a, Shortest_Path b)
    {
        return (a.X == b.X && a.Y == a.Y);
    }
    public static bool operator !=(Shortest_Path a, Shortest_Path b)
    {
        return !(a == b);
    }
}
