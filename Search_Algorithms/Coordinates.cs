namespace Search_Algorithms;
public struct Coordinates(int a, int b)
{
    public int x = a;
    public int y = b;
    public decimal Distance(Coordinates b) => (decimal)Math.Sqrt(Math.Pow(b.x - x, 2) + Math.Pow(b.y - y, 2));
    public static int _2dTo1d(Coordinates a) => ((a.x + 1) * a.y) + a.x + 1;
    public static Coordinates _1dTo2d(int index, int n) => new((index - 1) / n, (index - 1) % n);
    public static bool operator ==(Coordinates a, Coordinates b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Coordinates a, Coordinates b) => !(a == b);
    public override string ToString() => $"({x}, {y})";
}