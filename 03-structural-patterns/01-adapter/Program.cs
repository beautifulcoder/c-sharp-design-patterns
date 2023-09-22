using System.Collections;
using System.Collections.ObjectModel;
using MoreLinq.Extensions;
using static System.Console;

var d = new Demo();
d.Main();

public class Point
{
  public readonly int X;
  public readonly int Y;

  public Point(int x, int y)
  {
    X = x;
    Y = y;
  }

  protected bool Equals(Point other)
  {
    return X == other.X && Y == other.Y;
  }

  public override bool Equals(object? obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((Point) obj);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return (X * 397) ^ Y;
    }
  }

  public override string ToString()
  {
    return $"({X}, {Y})";
  }
}

public class Line
{
  public readonly Point? Start, End;

  public Line(Point start, Point end)
  {
    Start = start;
    End = end;
  }

  protected bool Equals(Line other)
  {
    return Equals(Start, other.Start) && Equals(End, other.End);
  }

  public override bool Equals(object? obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((Line) obj);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return ((Start != null ? Start.GetHashCode() : 0) * 397)
             ^ (End != null ? End.GetHashCode() : 0);
    }
  }
}

public abstract class VectorObject : Collection<Line>
{
}

public class VectorRectangle : VectorObject
{
  public VectorRectangle(int x, int y, int width, int height)
  {
    Add(new Line(new Point(x, y), new Point(x + width, y)));
    Add(new Line(new Point(x + width, y), new Point(x + width, y + height)));
    Add(new Line(new Point(x, y), new Point(x, y + height)));
    Add(new Line(new Point(x, y + height), new Point(x + width, y + height)));
  }
}

public class LineToPointAdapter : IEnumerable<Point>
{
  //private static int _count;
  private static readonly Dictionary<int, List<Point>> Cache = new();
  private readonly int _hash;

  public LineToPointAdapter(Line line)
  {
    _hash = line.GetHashCode();
    if (Cache.ContainsKey(_hash)) return; // we already have it

    //WriteLine($"{++_count}: Generating points for line [{line.Start!.X},{line.Start!.Y}]" +
    //          $"-[{line.End!.X},{line.End!.Y}] (with caching)");
    //                                                 ^^^^

    var points = new List<Point>();

    var left = Math.Min(line.Start!.X, line.End!.X);
    var right = Math.Max(line.Start.X, line.End.X);
    var top = Math.Min(line.Start.Y, line.End.Y);
    var bottom = Math.Max(line.Start.Y, line.End.Y);
    var dx = right - left;
    var dy = line.End.Y - line.Start.Y;

    if (dx == 0)
    {
      for (var y = top; y <= bottom; ++y)
      {
        points.Add(new Point(left, y));
      }
    }
    else if (dy == 0)
    {
      for (var x = left; x <= right; ++x)
      {
        points.Add(new Point(x, top));
      }
    }

    Cache.Add(_hash, points);
  }

  public IEnumerator<Point> GetEnumerator()
  {
    return Cache[_hash].GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}

public class Demo
{
  private static readonly List<VectorObject> VectorObjects = new()
  {
    new VectorRectangle(1, 1, 10, 10),
    new VectorRectangle(3, 3, 6, 6)
  };

  // the interface we have
  public static void DrawPoint(Point p)
  {
    Write(".");
  }

  public void Main()
  {
    Draw();
  }

  private static void Draw()
  {
    foreach (var vo in VectorObjects)
    {
      foreach (var line in vo)
      {
        var adapter = new LineToPointAdapter(line);
        adapter.ForEach(DrawPoint);
        WriteLine();
      }

      WriteLine();
    }
  }
}
