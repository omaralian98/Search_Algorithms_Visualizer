using Search_Algorithms;
using Search_Algorithms.Algorithms;
using Search_Algorithms.Games.Graph;
using Search_Algorithms.Games.Shortest_Path;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

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

    /// <summary>
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

    private static void AddAdorner(Button button)
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(button);
        adornerLayer?.Add(new ButtonCustomAdorner(button));
    }

    private static void RemoveAdorner(Button button)
    {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(button);
        Adorner[]? adorners = adornerLayer?.GetAdorners(button);

        if (adorners != null)
        {
            foreach (Adorner adorner in adorners)
            {
                if (adorner is ButtonCustomAdorner)
                {
                    adornerLayer?.Remove(adorner);
                    break;
                }
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
        if (IsAddLineClicked)
        {
            sourceNodeId = Convert.ToInt32(((Button)sender).Tag);
            Graph_MouseDown(sender, e);
            return;
        } 
        else if (sender is Button button)
        {
            var nodeId = Convert.ToInt32(button.Tag);
            if (e.RightButton == MouseButtonState.Pressed)
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
        if (isDragging && sender is Button button && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(Graph);
            double deltaX = currentPosition.X - originalPosition.X;
            double deltaY = currentPosition.Y - originalPosition.Y;

            double newX = Canvas.GetLeft(button) + deltaX;
            double newY = Canvas.GetTop(button) + deltaY;

            newX = Math.Max(0, Math.Min(Graph.ActualWidth - button.ActualWidth, newX));
            newY = Math.Max(0, Math.Min(Graph.ActualHeight - button.ActualHeight, newY));

            Canvas.SetLeft(button, newX);
            Canvas.SetTop(button, newY);

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
    public bool IsAddLineClicked
    {
        get
        {
            return _IsAddLineClicked;
        }
        set
        {
            _IsAddLineClicked = value;
            CheckBox_Checked();
        }
    }
    private Point startPoint;
    private Line currentLine;

    private void Graph_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsAddLineClicked)
        {
            Point position = e.GetPosition(Graph);

            if (double.IsNaN(startPoint.X))
            {
                startPoint = position;
            }
            else
            {
                CheckBox_Checked();
            }
            if (sender is not Button)
            {
                Point hitPoint = e.GetPosition(Graph);
                VisualTreeHelper.HitTest(Graph, null, new HitTestResultCallback((result) =>
                {
                    if (result.VisualHit is Ellipse ellipse)
                    {
                        var destinationNodeId = Convert.ToInt32(ellipse.Tag);
                        if (sourceNodeId != -1)
                        {
                            graph.AddEdge(sourceNodeId, destinationNodeId);
                        }
                        return HitTestResultBehavior.Stop;
                    }
                    return HitTestResultBehavior.Continue;
                }), new PointHitTestParameters(hitPoint));

            }
        }
    }
    private void Graph_MouseMove(object sender, MouseEventArgs e)
    {
        if (IsAddLineClicked)
        {
            // Get the mouse position relative to the canvas
            Point position = e.GetPosition(Graph);

            // If startPoint is set, update the line's endpoint
            if (!double.IsNaN(startPoint.X))
            {
                currentLine.X1 = startPoint.X;
                currentLine.Y1 = startPoint.Y;
                currentLine.X2 = position.X;
                currentLine.Y2 = position.Y;
            }
        }
    }

    private void CheckBox_Checked()
    {
        if (IsAddLineClicked)
        {
            startPoint = new Point(double.NaN, double.NaN);
            currentLine = new Line
            {
                Stroke = Brushes.Black
            };
            Graph.Children.Add(currentLine);
        }

    }

    private void TriggerHolders()
    {
        if (IsAddLineClicked)
        {
            foreach (var nodeIndex in NodesIndex)
            {

            }
        }
        else
        {

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
                    //res = await AStarSearch.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 1:
                    //res = await Greedy_Best_First_Search.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 2:
                    res = await Breadth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 3:
                    res = await Depth_First_Search.FindPath(graph.StartNode, delay: Convert.ToInt32(Delay * 1000));
                    break;
                case 4:
                    //res = await Hill_Climbing.FindPath(graph.StartNode, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
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
}
