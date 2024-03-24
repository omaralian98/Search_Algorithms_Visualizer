namespace Search_Algorithms.Games.Graph;

public class Graph<T>
{
    public List<Node<T>> Nodes { get; }
    public Node<T> StartNode { get; set; }
    public Node<T> TargetNode { get; set; }

    private int IdCounters = 0;

    public Graph()
    {
        Nodes = [];
    }

    public Node<T> AddNode(T data)
    {
        Node<T> newNode = new(this, IdCounters++, data);
        Nodes.Add(newNode);
        return newNode;
    }
    public void AddNode(Node<T> node) => Nodes.Add(node);

    public void AddEdge(int sourceId, int destinationId, int weight = 1)
    {
        Node<T>? sourceNode = Nodes.Find(node => node.Id == sourceId);
        Node<T>? destinationNode = Nodes.Find(node => node.Id == destinationId);

        if (sourceNode == null || destinationNode == null)
        {
            throw new ArgumentException("Source or destination node not found in the graph.");
        }

        sourceNode.AddNeighbor(destinationNode, weight);
    }

    public void DeleteEdge(int nodeId)
    {
        var node = GetNode(nodeId);
        if (node is not null)
        {
            Nodes.Remove(node);
        }
    }

    public Node<T>? GetNode(int id) => Nodes.FirstOrDefault(x => x.Id == id);
}
