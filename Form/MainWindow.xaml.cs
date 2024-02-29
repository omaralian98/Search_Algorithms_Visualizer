using Search_Algorithms.Games.Shortest_Path;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Search_Algorithms.Algorithms;
using System.Globalization;
using System.Drawing;
using System.Reflection;

namespace Form;
public enum CellColor
{
    Default,
    Visited,
    Start,
    Target,
    Obstacle,
    Path
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static int Rows { get; set; } = 10;
    public static int Columns { get; set; } = 10;
    public static decimal Delay { get; set; } = 0.1m;
    public static Coordinates Start { get; set; } = new Coordinates(-1, -1);
    public static Coordinates Target {  get; set; } = new Coordinates(-1, -1);
    private bool IsGridInitialized = false;
    private bool IsStartSet = false;
    private bool IsTargetSet = false;
    public static Search_Algorithms.Games.Shortest_Path.Grid grid;
    public MainWindow()
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

    public async void InitializeGrid(object sender, RoutedEventArgs e)
    {
        ResetGrid();
        Grid.Rows = Rows;
        Grid.Columns = Columns;
        grid = new Search_Algorithms.Games.Shortest_Path.Grid(Rows, Columns);
        foreach (var cell in grid.GridStates)
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
                if (e.PropertyName == nameof(cell.IsVisited))
                {
                    label.Dispatcher.Invoke(() =>
                    {
                        SetCellBackground(label, cell.IsVisited ? CellColor.Visited : CellColor.Default);
                    });
                }
            };
            SetCellBackground(label, CellColor.Default);
            Grid.Children.Add(label);
            await Task.Delay(0);
        }
        SetStart_Click(0, 0);
        SetTarget_Click(Rows - 1, Columns - 1);
    }

    private void SetTarget_Click(int x, int y)
    {
        Coordinates temp = new(x, y);
        var label = (Label)Grid.Children[(x * Grid.Columns) + y];
        if (temp == Target)
        {
            Target = new Coordinates(-1, -1);
            IsTargetSet = false;
            SetCellBackground(label, CellColor.Default);
        }
        else
        {
            Target = temp;
            IsTargetSet = true;
            SetCellBackground(label, CellColor.Target);
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
            SetCellBackground(label, CellColor.Default);
        }
        else
        {
            Start = temp;
            IsStartSet = true;
            SetCellBackground(label, CellColor.Start);
        }
    }

    private void SetCellBackground(Label label, CellColor color)
    {
        switch (color)
        {
            case CellColor.Default:
                label.Background = Brushes.Red;
                break;
            case CellColor.Visited:
                label.Background = Brushes.Green;
                break;
            case CellColor.Start:
                label.Background = Brushes.Blue;
                break;
            case CellColor.Target:
                label.Background = Brushes.Purple;
                break;
            case CellColor.Obstacle:
                label.Background = Brushes.Black;
                break;
            case CellColor.Path:
                label.Background = Brushes.Gold;
                break;
            default:
                break;
        }
    }
    private CancellationTokenSource cancellationTokenSource;

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        if (!IsGridInitialized)
        {
            MessageBox.Show("You should create a grid first", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            ((Button)sender).Content = "Search";
            return;
        }
        ((Button)sender).Content = "Cancel";
        cancellationTokenSource = new CancellationTokenSource();
        grid.Start = grid.GridStates[Start.X, Start.Y];
        grid.End = new GridState(Target.X, Target.Y, grid);
        AStarSearch<GridState> aStar = new();

        try
        {
            var value = await aStar.FindPath(grid.Start, Search_Algorithms.Games.Shortest_Path.Grid.ManhattanDistance, delay: Convert.ToInt32(Delay * 1000), token: cancellationTokenSource.Token);
            foreach (var cell in value.Steps)
            {
                var lab = (Label)Grid.Children[(cell.X * Grid.Columns) + cell.Y];
                SetCellBackground(lab, CellColor.Path);
            }
            ((Button)sender).Content = "Search";
        }
        catch (OperationCanceledException)
        {
            foreach (var cell in grid.GridStates)
            {
                cell.IsVisited = false;
            }
            var temp = Start;
            SetStart_Click(Start.X , Start.Y);
            SetStart_Click(temp.X, temp.Y);
        }
    }

    private void AddRandomObstacles(object sender, RoutedEventArgs e)
    {
        ClearObstacles();
        if (!IsGridInitialized || !IsStartSet || !IsTargetSet)
        {
            MessageBox.Show("You should initialize the grid, add start point and Target first");
            return;
        }

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
                grid.GridStates[randomRow, randomCol].IsObstacle = true;
                SetCells_Click(Grid.Children[(randomRow * Grid.Columns) + randomCol], new RoutedEventArgs());
                obstacleCount++;
            }
        }
    }


    private void ClearObstacles()
    {
        foreach (var cell in grid.GridStates)
        {
            cell.IsObstacle = false;
            cell.IsVisited = false;
        }
        foreach (Label cell in Grid.Children)
        {
            var co = GetCoordinates(cell.Name);
            if (co != Start && co != Target)
            {
                SetCellBackground(cell, CellColor.Default);
            } 
            else if (co == Start)
            {
                SetCellBackground(cell, CellColor.Start);
            }
            else if (co == Target)
            {
                SetCellBackground(cell, CellColor.Target);
            }
        }
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
        Start = new(0, 0);
        var lab = (Label)sender;
        lab.Background = Brushes.Red;
        lab.MouseDown -= ResetStart_Click;
        lab.MouseDown += SetCells_Click;
        IsStartSet = false;
    }

    private void ResetTarget_Click(object sender, RoutedEventArgs e)
    {
        Target = new(Rows - 1, Columns - 1);
        var lab = (Label)sender;
        lab.Background = Brushes.Red;
        lab.MouseDown -= ResetTarget_Click;
        lab.MouseDown += SetCells_Click;
        IsTargetSet = false;
    }

    private void ResetCell_Click(Label cell)
    {

    }

    private void SetCells_Click(object sender, RoutedEventArgs e) 
    {
        var lab = (Label)sender;
        var coordinate = GetCoordinates(lab.Name);

        if (!IsStartSet)
        {
            SetCellBackground(lab, CellColor.Start);
            Start = coordinate;
            lab.MouseDown -= SetCells_Click;
            lab.MouseDown += ResetStart_Click;
            IsStartSet = true;
        }
        else if (!IsTargetSet)
        {
            SetCellBackground(lab, CellColor.Target);
            Target = coordinate;
            lab.MouseDown -= SetCells_Click;
            lab.MouseDown += ResetTarget_Click;
            IsTargetSet = true;
        }
        else if(coordinate != Start && coordinate != Target)
        {
            SetCellBackground(lab, CellColor.Obstacle);
            grid.SetObstacle(coordinate.X, coordinate.Y);
        }
    }

    private Coordinates GetCoordinates(string a)
    {
        var str = a[1..].Split('D');
        return new Coordinates(int.Parse(str[0]), int.Parse(str[1]));
    }

    private async void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState.Maximized == this.WindowState)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(1);
                Dispatcher.Invoke(() =>
                {
                    Grid.Width = Grid.ActualHeight;
                });
            });
        }
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