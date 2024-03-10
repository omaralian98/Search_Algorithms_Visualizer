
using System.ComponentModel;

namespace Search_Algorithms.Games.Graph;

public class Node<T>(Graph<T> graph, int id, T data) : ISearchable, INotifyPropertyChanged
{
    public int Id { get; } = id;
    public Coordinates Coordinates { get; set; }
    public T Data { get; } = data;
    public List<Tuple<Node<T>, int>> Neighbors { get; } = [];
    public Graph<T> Graph { get; set; } = graph;
    private SearchState _State { get; set; } = SearchState.Default;
    public SearchState State
    {
        get { return _State; }
        set
        {
            if (value != _State)
            {
                _State = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

    private ISearchable? _Parent;
    public ISearchable? Parent { get => _Parent; set => _Parent = value; }


    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public void AddNeighbor(Node<T> neighbor, int weight = 1)
    {
        Neighbors.Add(new (neighbor, weight));
    }

    public IEnumerable<ISearchable> GetAllPossibleStates()
    {
        foreach (var neighbor in Neighbors)
        {
            if (neighbor.Item1.Id != Id)
            {
                yield return neighbor.Item1;
            }
        }
    }

    public bool IsOver()
    {
        return Graph.TargetNode.Id == Id;
    }
    public override string ToString() => $"{Id}";
}
