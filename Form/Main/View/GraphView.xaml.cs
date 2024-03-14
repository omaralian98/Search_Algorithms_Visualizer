using Search_Algorithms;
using Search_Algorithms.Algorithms;
using Search_Algorithms.Games.Graph;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Form.Main.View;

/*
* Resize Node: Done
* Resize Line
* 
* Check box fix: Done
* Connect Nodes to Lines
* Move Lines
* Create correct Graph
*/
/// <summary>
/// Interaction logic for GraphView.xaml
/// </summary>
public partial class GraphView : UserControl
{
    public Graph<string> graph = new();
    public Dictionary<int, int> NodesIndex = new();
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
        NodesIndex.Add(node.Id, Graph.Children.Count);
        Button newNode = new()
        {
            Content = $"N{node.Id}",
            Tag = node.Id,
            Width = 80,
            Height = 80,
        };

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
                break;
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

            if (IsAddLineClicked)
            {
                if (sourceNodeId == -1)
                {
                    sourceNodeId = nodeId;
                    AddAdorner(button, new BorderAdorner(button));
                }
                else
                {
                    var destinationNodeId = Convert.ToInt32(((Button)sender).Tag);
                    var sourceNode = (Button)Graph.Children[NodesIndex.GetValueOrDefault(sourceNodeId)];
                    if (sourceNodeId == destinationNodeId)
                    {
                        ConnectNodeToItSelf(sourceNode);
                    }
                    else
                    {
                        var destinationNode = (Button)Graph.Children[NodesIndex.GetValueOrDefault(destinationNodeId)];
                        ConnectTwoNodes(sourceNode, destinationNode);
                    }
                    RemoveAdorner(sourceNode);
                    sourceNodeId = -1;
                }
            }

            else if (e.RightButton == MouseButtonState.Pressed)
            {
                // If Start Node is not set
                if (StartNodeId == -1 && nodeId != TargetNodeId)
                {
                    StartNodeId = nodeId;
                    var index = NodesIndex.GetValueOrDefault(StartNodeId);
                    SetNodeBackground((Button)Graph.Children[index], CellColour.Start);
                }
                // If Target Node is not set
                else if (TargetNodeId == -1 && nodeId != StartNodeId)
                {
                    TargetNodeId = nodeId;
                    var index = NodesIndex.GetValueOrDefault(TargetNodeId);
                    SetNodeBackground((Button)Graph.Children[index], CellColour.Target);
                }
                //If the StartNode is set and it's clicked again = reset
                else if (nodeId == StartNodeId)
                {
                    var index = NodesIndex.GetValueOrDefault(StartNodeId);
                    SetNodeBackground((Button)Graph.Children[index], CellColour.Default);
                    StartNodeId = -1;
                }
                //If the TargetNode is set and it's clicked again = reset
                else if (nodeId == TargetNodeId)
                {
                    var index = NodesIndex.GetValueOrDefault(TargetNodeId);
                    SetNodeBackground((Button)Graph.Children[index], CellColour.Default);
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
        if (sender is Button button)
        {
            isDragging = false;
            button.ReleaseMouseCapture();
        }
    }

    public bool _IsAddLineClicked = false;
    private string AddLineMode;
    public bool IsAddLineClicked
    {
        get
        {
            return _IsAddLineClicked;
        }
        set
        {
            _IsAddLineClicked = value;
            RemoveAdorner(adornerBtn);
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
        graph.StartNode = graph.GetNode(StartNodeId);
        graph.TargetNode = graph.GetNode(TargetNodeId);
        try
        {
            SearchResult<ISearchable> res = new();
            switch (Algorithms.SelectedIndex)
            {
                case 0:
                    //res = await AStarSearch.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Button.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 1:
                    //res = await Greedy_Best_First_Search.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Button.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 2:
                    res = await Breadth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 3:
                    res = await Depth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 4:
                    //res = await Hill_Climbing.FindPath(graph.StartNode, Search_Algorithms.Games.Shortest_Path.Button.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
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
            var index = NodesIndex.GetValueOrDefault(node.Id);
            var graphNode = Graph.Children[index];
            if (node.Id == StartNodeId)
            {
                SetNodeBackground((Button)graphNode, CellColour.Start);
            }
            else if (node.Id == TargetNodeId)
            {
                SetNodeBackground((Button)graphNode, CellColour.Target);
            }
            else
            {
                SetNodeBackground((Button)graphNode, CellColour.Default);
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

    private void ConnectTwoNodes(Button first, Button second)
    {
        static double GetAngle(double x1, double y1, double x2, double y2) => Math.Atan2(y2 - y1, x2 - x1);
        void DrawArrow(Point startPoint, Point endPoint, SolidColorBrush color)
        {
            var line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y,
                Stroke = color
            };
            Graph.Children.Add(line);

            // Calculate angle of the line
            var angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);

            // Draw arrowhead
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
            Graph.Children.Add(arrowHead);
        }

        var radius1 = first.ActualWidth / 2;
        var radius2 = second.ActualWidth / 2;

        var x1 = Canvas.GetLeft(first) + radius1;
        var y1 = Canvas.GetTop(first) + radius1;
        var x2 = Canvas.GetLeft(second) + radius2;
        var y2 = Canvas.GetTop(second) + radius2;

        var startPoint = new Point(x1 + radius1 * Math.Cos(GetAngle(x1, y1, x2, y2)), y1 + radius1 * Math.Sin(GetAngle(x1, y1, x2, y2)));
        var endPoint = new Point(x2 + radius2 * Math.Cos(GetAngle(x2, y2, x1, y1)), y2 + radius2 * Math.Sin(GetAngle(x2, y2, x1, y1)));

        var destinationNodeId = Convert.ToInt32(second.Tag);



        switch (AddLineMode)
        {
            case "UL":
                DrawArrow(startPoint, endPoint, Brushes.Black);
                DrawArrow(endPoint, startPoint, Brushes.Black);
                graph.AddEdge(sourceNodeId, destinationNodeId);
                graph.AddEdge(destinationNodeId, sourceNodeId);
                break;
            case "DL":
                DrawArrow(startPoint, endPoint, Brushes.Black);
                graph.AddEdge(sourceNodeId, destinationNodeId);
                break;
            default:
                break;
        }
    }

    private void ConnectNodeToItSelf(Button node)
    {
        var nodeId = Convert.ToInt32(node.Tag);
        graph.AddEdge(nodeId, nodeId);
    }






    private void AddLine_Checked(object sender, RoutedEventArgs e)
    {
        AddLineMode = ((ToggleButton)sender).Tag.ToString() ?? "";
    }
}
