using System.ComponentModel;

namespace Search_Algorithms.Games.Shortest_Path;


public class GridState : ISearchable, INotifyPropertyChanged
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsObstacle { get; set; }
    public Grid Grid { get; set; }

    public GridState(int x, int y, Grid grid)
    {
        X = x;
        Y = y;
        Grid = grid;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private bool isVisited;
    public bool IsVisited
    {
        get { return isVisited; }
        set
        {
            if (value != isVisited)
            {
                isVisited = value;
                OnPropertyChanged(nameof(IsVisited));
            }
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task VisitStateAsync()
    {
        await Task.Delay(100); // Simulating an asynchronous operation

        // Update the IsVisited property to true
        IsVisited = true;
    }


    private ISearchable? _parent;
    public ISearchable? Parent { get => _parent; set => _parent = value; }
    public IEnumerable<ISearchable> GetAllPossibleStates()
    {
        int[] xs = [0, -1, 0, 1];
        int[] ys = [-1, 0, 1, 0];

        for (int i = 0; i < 4; i++)
        {
            int x = xs[i] + X;
            int y = ys[i] + Y;
            if (!CanMove(x, y)) continue;
            if (!Grid.GridStates[x, y].IsObstacle)
            {
                yield return Grid.GridStates[x, y];
            }
        }
    }

    public bool CanMove(int x, int y)
    {
        return (x >= 0 && x < Grid.Rows) && (y >= 0 && y < Grid.Columns);
    }

    public bool IsOver()
    {
        return Grid.End.X == X && Grid.End.Y == Y;
    }

    public override string ToString() => $"{(X * Grid.Columns) + Y + 1}";
}
