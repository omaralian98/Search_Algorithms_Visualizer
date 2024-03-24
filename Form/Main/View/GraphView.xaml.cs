using Search_Algorithms;
using Search_Algorithms.Algorithms;
using Search_Algorithms.Games.Graph;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Form.Main.View;

/*
* Fix AddLine boxes
* Delete Node in Adorner
* Delete Edge in Adorner
* Add Coordinates to nodes 
* Fix informed searches 
* 
* Make the graph scrollable
* button to display x and y axis 
*/
/// <summary>
/// Interaction logic for GraphView.xaml
/// </summary>
public partial class GraphView : UserControl
{
    public Graph<string> graph = new();
    public Dictionary<int, Button> NodesIndex = new();
    public Dictionary<KeyValuePair<Grid, char>, KeyValuePair<int, int>> EdgesIndex = new();
    public int StartNodeId = -1;
    public int TargetNodeId = -1;

    public static decimal Delay { get; set; } = 0.1m;
    public GraphView()
    {
        InitializeComponent();
    }

    /// <summary>N
    /// The Event of draging elements from the toolbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DragElement(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button button)
        {
            DragDrop.DoDragDrop(button, button.Content, DragDropEffects.Copy);
        }
    }

    private void Graph_DragEnter(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.StringFormat) || sender == e.Source)
        {
            e.Effects = DragDropEffects.Move;
        }
    }

    /// <summary>
    /// When we drop the element onto the canvas
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Graph_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.StringFormat))
        {
            string content = e.Data.GetData(DataFormats.StringFormat).ToString() ?? "";
            switch (content)
            {
                case "N":
                    AddNode(e);
                    break;
                case "L":
                    //AddLine(e);
                    break;
                case "WL":
                    //AddLine();
                    break;
            }
        }

    }

    /// <summary>
    /// Adds New Node to the graph
    /// </summary>
    /// <param name="e"></param>
    private void AddNode(DragEventArgs e)
    {
        void ApplyNodeStyleToButton(Button button)
        {
            if (button != null)
            {
                Style? nodeStyle = FindResource("NodeStyle") as Style;

                button.Style = nodeStyle;
            }
        }
        var node = graph.AddNode("N");
        Button newNode = new()
        {
            Content = $"N{node.Id + 1}",
            Tag = node.Id,
            Width = 80,
            Height = 80,
        };
        NodesIndex.Add(node.Id, newNode);

        SetNodeBackground(newNode, CellColour.Default);
        ApplyNodeStyleToButton(newNode);
        newNode.PreviewMouseDown += Node_PreviewMouseDown;
        newNode.PreviewMouseUp += Node_PreviewMouseUp;
        newNode.PreviewMouseMove += Node_PreviewMouseMove;
        node.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(node.State))
            {

                Dispatcher.Invoke(Delay == 0 ? DispatcherPriority.Background : DispatcherPriority.Normal, () =>
                {
                    switch (node.State)
                    {
                        case SearchState.Default:
                            SetNodeBackground(newNode, CellColour.Default);
                            break;
                        case SearchState.Discovered:
                            SetNodeBackground(newNode, CellColour.Dicovered);
                            break;
                        case SearchState.Visited:
                            SetNodeBackground(newNode, CellColour.Visited);
                            break;
                        case SearchState.Path:
                            SetNodeBackground(newNode, CellColour.Path);
                            break;
                    }
                });
            }
        };

        Point dropPosition = e.GetPosition(Graph);
        Canvas.SetLeft(newNode, dropPosition.X);
        Canvas.SetTop(newNode, dropPosition.Y);
        Graph.Children.Add(newNode);
    }

    private Button? adornerBtn;


    /// <summary>
    /// When Node is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Node_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        if (button == adornerBtn)
        {
            RemoveAdorner(adornerBtn);
            adornerBtn = null;
        }
        else if (adornerBtn is not null)
        {
            RemoveAdorner(adornerBtn);
            AddAdorner(button);
            adornerBtn = button;
        }
        else
        {
            AddAdorner(button);
            adornerBtn = button;
        }
    }

    private static void AddAdorner(Button? button, Adorner? adorner = null)
    {
        if (button is null) return;
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(button);
        adornerLayer?.Add(adorner ?? new ButtonCustomAdorner(button));
    }

    private static void RemoveAdorner(Button? button)
    {
        if (button is null) return;
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(button);
        Adorner[]? adorners = adornerLayer?.GetAdorners(button);

        if (adorners != null)
        {
            foreach (Adorner adorner in adorners)
            {
                adornerLayer?.Remove(adorner);
            }
        }
    }

    private bool isDragging = false;
    private Point originalPosition;
    private int sourceNodeId = -1;

    /// <summary>
    /// When Node is Clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Node_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        // If AddLine is Clicked then we draw a line and save the Id of this node to be the source of this line
        if (sender is Button button)
        {
            var nodeId = Convert.ToInt32(button.Tag);
            if (DeleteClicked)
            {
                DeleteNode(nodeId);
            }

            else if (AddLineClicked)
            {
                if (sourceNodeId == -1)
                {
                    sourceNodeId = nodeId;
                    AddAdorner(button, new BorderAdorner(button));
                    adornerBtn = button;
                }
                else
                {
                    string mode = AddLineMode;
                    var destinationNodeId = Convert.ToInt32(((Button)sender).Tag);
                    bool exist = EdgeExists(new(sourceNodeId, destinationNodeId));
                    bool inverse = EdgeExists(new(destinationNodeId, sourceNodeId));
                    if (exist)
                    {
                        RemoveAdorner(adornerBtn);
                        sourceNodeId = -1;
                        return;
                    }

                    else if (inverse)
                    {
                        mode = "C";
                    }
                    if (sourceNodeId == destinationNodeId)
                    {
                        ConnectNodeToItSelf(sourceNodeId);
                    }
                    else
                    {
                        ConnectTwoNodes(sourceNodeId, destinationNodeId, mode);
                    }
                    RemoveAdorner(adornerBtn);
                    sourceNodeId = -1;
                }
            }

            else if (e.RightButton == MouseButtonState.Pressed)
            {
                // If Start Node is not set
                if (StartNodeId == -1 && nodeId != TargetNodeId)
                {
                    StartNodeId = nodeId;
                    var btn = NodesIndex.GetValueOrDefault(StartNodeId);
                    SetNodeBackground(btn, CellColour.Start);
                }
                // If Target Node is not set
                else if (TargetNodeId == -1 && nodeId != StartNodeId)
                {
                    TargetNodeId = nodeId;
                    var btn = NodesIndex.GetValueOrDefault(TargetNodeId);
                    SetNodeBackground(btn, CellColour.Target);
                }
                //If the StartNode is set and it's clicked again = reset
                else if (nodeId == StartNodeId)
                {
                    var btn = NodesIndex.GetValueOrDefault(StartNodeId);
                    SetNodeBackground(btn, CellColour.Default);
                    StartNodeId = -1;
                }
                //If the TargetNode is set and it's clicked again = reset
                else if (nodeId == TargetNodeId)
                {
                    var btn = NodesIndex.GetValueOrDefault(TargetNodeId);
                    SetNodeBackground(btn, CellColour.Default);
                    TargetNodeId = -1;
                }
            }
            // If the left click is pressed start dragging
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                Node_Click(sender, e);
                isDragging = true;
                originalPosition = e.GetPosition(Graph);
                button.CaptureMouse();
            }
        }

    }

    private void Node_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging && sender is Button grid && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(Graph);
            double deltaX = currentPosition.X - originalPosition.X;
            double deltaY = currentPosition.Y - originalPosition.Y;

            double newX = Canvas.GetLeft(grid) + deltaX;
            double newY = Canvas.GetTop(grid) + deltaY;

            newX = Math.Max(0, Math.Min(Graph.ActualWidth - grid.ActualWidth, newX));
            newY = Math.Max(0, Math.Min(Graph.ActualHeight - grid.ActualHeight, newY));
            Canvas.SetLeft(grid, newX);
            Canvas.SetTop(grid, newY);
            originalPosition = currentPosition;
        }
    }

    private void Node_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button button && isDragging == true)
        {
            var nodeId = Convert.ToInt32(button.Tag);
            isDragging = false;
            button.ReleaseMouseCapture();
            ReConnectNode(nodeId);
        }
    }

    private void ReConnectNode(int nodeId)
    {
        var edges = GetEdges(source: nodeId, destenation: nodeId);
        foreach (var ((grid, mode), (sourceId, destinationId)) in edges)
        {
            RemoveEdge(sourceId, destinationId);
            if (sourceId == destinationId)
            {
                ConnectNodeToItSelf(sourceId);
            }
            else
            {
                ConnectTwoNodes(sourceId, destinationId, $"{mode}L");
            }
            Graph.Children.Remove(grid);
        }
    }

    public bool AddLineClicked = false;
    private string AddLineMode;

    private void CreateGraphConnection()
    {
        foreach (var co in EdgesIndex)
        {
            char mode = co.Key.Value;
            int sourceId = co.Value.Key;
            int targetId = co.Value.Value;
            if (mode == 'U')
            {
                graph.AddEdge(targetId, sourceId);
            }
            graph.AddEdge(sourceId, targetId);
        }
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        if ((StartNodeId == -1) || (TargetNodeId == -1) || graph.Nodes.Count == 0)
        {
            MessageBox.Show("You should add Nodes, add start point and Target first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        ClearGraph();
        CreateGraphConnection();
        graph.StartNode = graph.GetNode(StartNodeId);
        graph.TargetNode = graph.GetNode(TargetNodeId);
        try
        {
            SearchResult<ISearchable> res = new();
            switch (Algorithms.SelectedIndex)
            {
                case 0:
                    //res = await AStarSearch.FindPath(gra.StartNode, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 1:
                    //res = await Greedy_Best_First_Search.FindPath(gra.StartNode, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 2:
                    res = await Breadth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 3:
                    res = await Depth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 4:
                    //res = await Hill_Climbing.FindPath(gra.StartNode, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
            }
            MessageBox.Show(res.Steps.Count.ToString());
        }
        catch (OperationCanceledException)
        {
            ClearGraph();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearGraph()
    {
        var temp = Delay;
        Delay = 1;
        foreach (var node in graph.Nodes)
        {
            var btn = NodesIndex.GetValueOrDefault(node.Id);
            if (node.Id == StartNodeId)
            {
                SetNodeBackground(btn, CellColour.Start);
            }
            else if (node.Id == TargetNodeId)
            {
                SetNodeBackground(btn, CellColour.Target);
            }
            else
            {
                SetNodeBackground(btn, CellColour.Default);
            }
        }
        Delay = temp;
    }

    private void SetNodeBackground(Button btn, CellColour color)
    {
        switch (color)
        {
            case CellColour.Default:
                btn.Background = Brushes.White;
                break;
            case CellColour.Visited:
                btn.Background = Brushes.Green;
                break;
            case CellColour.Dicovered:
                btn.Background = Brushes.Aqua;
                break;
            case CellColour.Start:
                btn.Background = Brushes.Blue;
                break;
            case CellColour.Target:
                btn.Background = Brushes.MediumPurple;
                break;
            case CellColour.Obstacle:
                btn.Background = Brushes.Black;
                break;
            case CellColour.Path:
                Dispatcher.Invoke(DispatcherPriority.Background, () =>
                {
                    btn.Background = Brushes.Gold;
                });
                break;
            default:
                break;
        }
    }

    private void ConnectTwoNodes(int sourceId, int destinationId, string mode)
    {
        static double GetAngle(double x1, double y1, double x2, double y2) => Math.Atan2(y2 - y1, x2 - x1);
        Line DrawLine(Point startPoint, Point endPoint, SolidColorBrush color)
        {
            var line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = color,
                Tag = $"{sourceId},{destinationId}",
            };
            line.MouseDown += Edge_MouseDown;
            return line;
        }
        Polygon DrawArrow(Point startPoint, Point endPoint, SolidColorBrush color)
        {
            var angle = GetAngle(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);

            var arrowAngle = Math.PI / 6; // 30 degrees
            var arrowLength = 10;
            var arrowPoint1 = new Point(
                endPoint.X - arrowLength * Math.Cos(angle - arrowAngle),
                endPoint.Y - arrowLength * Math.Sin(angle - arrowAngle)
            );
            var arrowPoint2 = new Point(
                endPoint.X - arrowLength * Math.Cos(angle + arrowAngle),
                endPoint.Y - arrowLength * Math.Sin(angle + arrowAngle)
            );
            var arrowHead = new Polygon
            {
                Stroke = color,
                Fill = color,
                Points = [endPoint, arrowPoint1, arrowPoint2]
            };
            return arrowHead;
        }

        var first = NodesIndex.GetValueOrDefault(sourceId);
        var second = NodesIndex.GetValueOrDefault(destinationId);


        var radius1 = first.ActualWidth / 2;
        var radius2 = second.ActualWidth / 2;

        var x1 = Canvas.GetLeft(first) + radius1;
        var y1 = Canvas.GetTop(first) + radius1;
        var x2 = Canvas.GetLeft(second) + radius2;
        var y2 = Canvas.GetTop(second) + radius2;

        var startPoint = new Point(x1 + radius1 * Math.Cos(GetAngle(x1, y1, x2, y2)), y1 + radius1 * Math.Sin(GetAngle(x1, y1, x2, y2)));
        var endPoint = new Point(x2 + radius2 * Math.Cos(GetAngle(x2, y2, x1, y1)), y2 + radius2 * Math.Sin(GetAngle(x2, y2, x1, y1)));


        var grid = new Grid();

        switch (mode)
        {
            case "UL":
                grid.Children.Add(DrawLine(startPoint, endPoint, Brushes.Black));
                grid.Children.Add(DrawArrow(startPoint, endPoint, Brushes.Black));
                grid.Children.Add(DrawArrow(endPoint, startPoint, Brushes.Black));
                AddEdge(grid, 'U', sourceId, destinationId);
                break;
            case "DL":
                grid = new Grid();
                grid.Children.Add(DrawLine(startPoint, endPoint, Brushes.Black));
                grid.Children.Add(DrawArrow(startPoint, endPoint, Brushes.Black));
                AddEdge(grid, 'D', sourceId, destinationId);
                break;
            case "C":
                RemoveEdge(destinationId, sourceId);
                goto case "UL";
            default:
                break;
        }
    }

    private void Edge_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (DeleteClicked && sender is Line edge)
        {
            int sourceId = Convert.ToInt32(edge.Tag?.ToString()?.Split(',')[0]);
            int destination = Convert.ToInt32(edge.Tag?.ToString()?.Split(',')[1]);
            RemoveEdge(sourceId, destination);
        }
    }

    private void ConnectNodeToItSelf(int nodeId)
    {
        var node = NodesIndex.GetValueOrDefault(nodeId);
    }


    private bool EdgeExists(KeyValuePair<int, int> a) => EdgesIndex.ContainsValue(a);
    private void AddEdge(Grid grid, char mode, int sourceId, int destenationId)
    {
        EdgesIndex.Add(new KeyValuePair<Grid, char>(grid, mode), new KeyValuePair<int, int>(sourceId, destenationId));
        Graph.Children.Add(grid);
    }
    private void RemoveEdge(int sourceId, int destinationId, int nodeId = -1)
    {
        if (nodeId == -1)
        {
            var edge = EdgesIndex.FirstOrDefault(x => x.Value.Key == sourceId && x.Value.Value == destinationId);
            EdgesIndex.Remove(edge.Key);
            Graph.Children.Remove(edge.Key.Key);
        }
        else
        {
            var edge = EdgesIndex.Where(x => x.Value.Key == nodeId || x.Value.Value == nodeId);
            foreach (var ed in edge)
            {
                EdgesIndex.Remove(ed.Key);
                Graph.Children.Remove(ed.Key.Key);
            }
        }
    } 
    private void EditEdge(KeyValuePair<int, int> a)
    {
        var item = EdgesIndex.First(x => x.Value.Key == a.Value && x.Value.Value == a.Key);
        RemoveEdge(a.Key, a.Value);
        AddEdge(item.Key.Key, 'U', item.Value.Key, item.Value.Value);
    }
    private KeyValuePair<KeyValuePair<Grid, char>, KeyValuePair<int, int>>[] GetEdges(int source = -1, int destenation = -1)
    {
        if (source > -1 && destenation == -1)
        {
            return EdgesIndex.Where(x => x.Value.Key == source).ToArray();
        }
        else if (source == -1 && destenation > -1)
        {
            return EdgesIndex.Where(x => x.Value.Value == destenation).ToArray();
        }
        else return [.. EdgesIndex];
    }



    private void AddLine_Checked(object sender, RoutedEventArgs e)
    {
        AddLineClicked = !AddLineClicked;
        RemoveAdorner(adornerBtn);
        sourceNodeId = -1;
        AddLineMode = ((ToggleButton)sender).Tag.ToString() ?? "";
    }

    private bool DeleteClicked = false;

    private void DeleteEnable_Click(object sender, RoutedEventArgs e)
    {
        DeleteClicked = !DeleteClicked;
    }

    private void DeleteNode(int nodeId)
    {
        RemoveEdge(0, 0, nodeId);
        graph.DeleteEdge(nodeId);
        Graph.Children.Remove(NodesIndex.FirstOrDefault(x => x.Key == nodeId).Value);
    }
}
