using Search_Algorithms.Algorithms;
using Search_Algorithms.Games.Shortest_Path;
using Search_Algorithms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Form.MainWindow;
using System.Windows.Threading;

namespace Form.Main.View;


public enum CellColour
{
    Default,
    Visited,
    Dicovered,
    Start,
    Target,
    Obstacle,
    Path
}


/// <summary>
/// Interaction logic for Grid.xaml
/// </summary>
public partial class GridView : UserControl
{
            /*
        Create ViewModel for Path finding
        Create ViewModel for Grid Viewer 
        Add Swarm Algorithm and it's variants
        Add Hill climbing Algorithm
    */
    public static int Rows { get; set; } = 10;
    public static int Columns { get; set; } = 10;
    public static decimal Delay { get; set; } = 0.1m;
    public static Coordinates Start { get; set; } = new Coordinates(-1, -1);
    public static Coordinates Target { get; set; } = new Coordinates(-1, -1);
    private bool IsGridInitialized = false;
    private bool IsStartSet = false;
    private bool IsTargetSet = false;
    public static Search_Algorithms.Games.Shortest_Path.Grid grid;
    public GridView()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void ResetGrid()
    {
        IsGridInitialized = true;
        IsStartSet = false;
        IsTargetSet = false;
        Start = new Coordinates(-1, -1);
        Target = new Coordinates(-1, -1);
        Grid.Children.Clear();
    }

    public void InitializeGrid(object sender, RoutedEventArgs e)
    {
        ResetGrid();
        Grid.Rows = Rows;
        Grid.Columns = Columns;
        grid = new Search_Algorithms.Games.Shortest_Path.Grid(Rows, Columns);
        foreach (var cell in grid.GridStates)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, () =>
            {
                Label label = new()
                {
                    Name = $"D{cell.X}D{cell.Y}",
                    BorderThickness = new Thickness(1),
                };
                label.MouseDown += SetCells_Click;
                label.MouseEnter += MouseDrag;
                cell.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(cell.State))
                    {
                        label.Dispatcher.Invoke(Delay == 0 ? DispatcherPriority.Background : DispatcherPriority.Normal, () =>
                        {
                            switch (cell.State)
                            {
                                case SearchState.Default:
                                    SetCellBackground(label, CellColour.Default);
                                    break;
                                case SearchState.Discovered:
                                    SetCellBackground(label, CellColour.Dicovered);
                                    break;
                                case SearchState.Visited:
                                    SetCellBackground(label, CellColour.Visited);
                                    break;
                                case SearchState.Path:
                                    SetCellBackground(label, CellColour.Path);
                                    break;
                            }
                        });
                    }
                    if (e.PropertyName == nameof(cell.IsObstacle))
                    {
                        label.Dispatcher.Invoke(() =>
                        {
                            switch (cell.IsObstacle)
                            {
                                case true:
                                    SetCellBackground(label, CellColour.Obstacle);
                                    break;
                                case false:
                                    SetCellBackground(label, CellColour.Default);
                                    break;
                            }
                        });
                    }
                };
                SetCellBackground(label, CellColour.Default);
                Grid.Children.Add(label);
            });

        }

        SetCells_Click(Grid.Children[0], new RoutedEventArgs());
        SetCells_Click(Grid.Children[^1], new RoutedEventArgs());
    }

    private void SetCellBackground(Label label, CellColour color)
    {
        switch (color)
        {
            case CellColour.Default:
                label.Background = Brushes.White;
                break;
            case CellColour.Visited:
                label.Background = Brushes.Green;
                break;
            case CellColour.Dicovered:
                label.Background = Brushes.DarkCyan;
                break;
            case CellColour.Start:
                label.Background = Brushes.Blue;
                break;
            case CellColour.Target:
                label.Background = Brushes.Purple;
                break;
            case CellColour.Obstacle:
                label.Background = Brushes.Black;
                break;
            case CellColour.Path:
                Dispatcher.Invoke(DispatcherPriority.Background, () =>
                {
                    label.Background = Brushes.Gold;
                });
                break;
            default:
                break;
        }
    }
    private CancellationTokenSource? cancellationTokenSource;

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        if (!IsGridInitialized || !IsStartSet || !IsTargetSet)
        {
            MessageBox.Show("You should initialize the grid, add start point and Target first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            ((Button)sender).Content = "Search";
            return;
        }
        ClearGrid(KeepObstacles: true);
        ((Button)sender).Content = "Cancel";
        cancellationTokenSource = new CancellationTokenSource();
        grid.Start = grid.GridStates[Start.X, Start.Y];
        grid.End = new GridState(Target.X, Target.Y, grid);

        try
        {
            SearchResult<ISearchable> res = new();
            switch (Algorithms.SelectedIndex)
            {
                case 0:
                    res = await AStarSearch.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 1:
                    res = await Greedy_Best_First_Search.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 2:
                    res = await Breadth_First_Search.FindPath(grid.Start, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 3:
                    res = await Depth_First_Search.FindPath(grid.Start, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
                case 4:
                    res = await Hill_Climbing.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
                    break;
            }
            MessageBox.Show(res.Steps.Count.ToString());
        }
        catch (OperationCanceledException)
        {
            ClearGrid(KeepObstacles: true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        cancellationTokenSource = null;
        ((Button)sender).Content = "Search";
    }

    private void AddRandomObstacles(object sender, RoutedEventArgs e)
    {
        if (!IsGridInitialized || !IsStartSet || !IsTargetSet)
        {
            MessageBox.Show("You should initialize the grid, add start point and Target first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        ClearGrid();

        Random random = new Random();
        int obstacleCount = 0; // Number of obstacles you want to add
        int maxObstacleCount = grid.GridStates.GetLength(0) * grid.GridStates.GetLength(1) / 4; // Maximum number of obstacles

        // Add obstacles until desired obstacle count is reached
        while (obstacleCount < maxObstacleCount)
        {
            int randomRow = random.Next(0, grid.GridStates.GetLength(0));
            int randomCol = random.Next(0, grid.GridStates.GetLength(1));
            var co = new Coordinates(randomRow, randomCol);
            // Check if the cell is already an obstacle
            if (!grid.GridStates[randomRow, randomCol].IsObstacle && Start != co && Target != co)
            {
                SetCells_Click(Grid.Children[(randomRow * Grid.Columns) + randomCol], new RoutedEventArgs());
                obstacleCount++;
            }
        }
    }


    private void ClearGrid(bool KeepObstacles = false)
    {
        var temp = Delay;
        Delay = 1;
        foreach (var cell in grid.GridStates)
        {
            if (cell.X == Start.X && cell.Y == Start.Y)
            {
                SetCellBackground((Label)Grid.Children[cell.X * Columns + cell.Y], CellColour.Start);

            }
            else if (cell.X == Target.X && cell.Y == Target.Y)
            {
                SetCellBackground((Label)Grid.Children[cell.X * Columns + cell.Y], CellColour.Target);
            }
            else
            {
                if (!KeepObstacles) cell.IsObstacle = false;
                cell.State = SearchState.Default;
            }
        }
        Delay = temp;
    }



    private void MouseDrag(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            SetCells_Click(sender, new RoutedEventArgs());
        }
    }

    private void ResetStart_Click(object sender, RoutedEventArgs e)
    {
        Start = new(-1, -1);
        var lab = (Label)sender;
        SetCellBackground(lab, CellColour.Default);
        lab.MouseDown -= ResetStart_Click;
        lab.MouseDown += SetCells_Click;
        IsStartSet = false;
    }

    private void ResetTarget_Click(object sender, RoutedEventArgs e)
    {
        Target = new(-1, -1);
        var lab = (Label)sender;
        SetCellBackground(lab, CellColour.Default);
        lab.MouseDown -= ResetTarget_Click;
        lab.MouseDown += SetCells_Click;
        IsTargetSet = false;
    }

    private void SetTarget_Click(int x, int y)
    {
        Coordinates temp = new(x, y);
        var label = (Label)Grid.Children[(x * Grid.Columns) + y];
        if (temp == Target)
        {
            Target = new Coordinates(-1, -1);
            IsTargetSet = false;
            SetCellBackground(label, CellColour.Default);
            label.MouseDown += SetCells_Click;
            label.MouseDown -= ResetTarget_Click;
        }
        else
        {
            Target = temp;
            IsTargetSet = true;
            SetCellBackground(label, CellColour.Target);
            label.MouseDown -= SetCells_Click;
            label.MouseDown += ResetTarget_Click;
        }
    }

    private void SetStart_Click(int x, int y)
    {
        Coordinates temp = new(x, y);
        var label = (Label)Grid.Children[(x * Grid.Columns) + y];
        if (temp == Start)
        {
            Start = new Coordinates(-1, -1);
            IsStartSet = false;
            SetCellBackground(label, CellColour.Default);
            label.MouseDown += SetCells_Click;
            label.MouseDown -= ResetStart_Click;
        }
        else
        {
            Start = temp;
            IsStartSet = true;
            SetCellBackground(label, CellColour.Start);
            label.MouseDown -= SetCells_Click;
            label.MouseDown += ResetStart_Click;
        }
    }


    private void SetCells_Click(object sender, RoutedEventArgs e)
    {
        var lab = (Label)sender;
        var coordinate = GetCoordinates(lab.Name);

        if (!IsStartSet)
        {
            SetStart_Click(coordinate.X, coordinate.Y);
        }
        else if (!IsTargetSet)
        {
            SetTarget_Click(coordinate.X, coordinate.Y);
        }
        else if (coordinate != Start && coordinate != Target)
        {
            if (grid.GridStates[coordinate.X, coordinate.Y].IsObstacle)
            {
                grid.GridStates[coordinate.X, coordinate.Y].IsObstacle = false;
            }
            else
            {
                grid.GridStates[coordinate.X, coordinate.Y].State = SearchState.Default;
                grid.GridStates[coordinate.X, coordinate.Y].IsObstacle = true;
            }
        }
    }

    private static Coordinates GetCoordinates(string a)
    {
        var str = a[1..].Split('D');
        return new Coordinates(int.Parse(str[0]), int.Parse(str[1]));
    }
}
public struct Coordinates(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    public static bool operator ==(Coordinates a, Coordinates b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(Coordinates a, Coordinates b) => !(a == b);
}
