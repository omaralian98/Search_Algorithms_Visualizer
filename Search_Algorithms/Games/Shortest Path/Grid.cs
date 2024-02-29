namespace Search_Algorithms.Games.Shortest_Path;

public class Grid
{
    public int Rows { get; set; }
    public int Columns { get; set; }
    public GridState[,] GridStates { get; set; }
    public GridState Start { get; set; } 
    public GridState End { get; set; }
    public Grid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        GridStates = new GridState[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GridStates[i, j] = new GridState(i, j, this);
            }
        }
        Start = GridStates[0, 0];
        End = GridStates[rows - 1, columns - 1];
    }

    public void SetObstacle(int x, int y)
    {
        GridStates[x, y].IsObstacle = true;
    }

    /// <summary>
    /// This function finds the manhattan distance between the board and the goal board
    /// </summary>
    /// <param name="a">The Game</param>
    /// <returns>The manhattan distance</returns>
    public static int ManhattanDistance(ISearchable a)
    {
        var current = (GridState)a;
        var grid = current.Grid;
        int manhattanDistance = 0;
        manhattanDistance += Math.Abs(current.X - grid.End.X) + Math.Abs(current.Y - grid.End.Y);
        return manhattanDistance;
    }
}
