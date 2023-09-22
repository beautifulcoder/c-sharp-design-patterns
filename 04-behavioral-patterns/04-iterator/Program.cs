using System.Collections;
using static System.Console;

var root = new Node<int>(1, new Node<int>(2), new Node<int>(3));

var it = new InOrderIterator<int>(root);

// C++ style
while (it.MoveNext())
{
  Write(it.Current?.Value);
  Write(',');
}
WriteLine();

// C# style
var tree = new BinaryTree<int>(root);

WriteLine(string.Join(",", tree.NaturalInOrder.Select(x => x.Value)));

var data = new [,] { { 1, 2 }, { 3, 4 } };
var sum = new OneDAdapter<int>(data).Sum();

WriteLine(sum);

public class Node<T>
{
  public T Value;
  public Node<T>? Left, Right;
  public Node<T>? Parent;

  public Node(T value)
  {
    Value = value;
  }

  public Node(T value, Node<T> left, Node<T> right)
  {
    Value = value;
    Left = left;
    Right = right;

    left.Parent = right.Parent = this;
  }
}

public class InOrderIterator<T>
{
  public Node<T>? Current { get; set; }
  private readonly Node<T> _root;
  private bool _yieldedStart;

  public InOrderIterator(Node<T> root)
  {
    _root = Current = root;
    while (Current.Left != null)
    {
      Current = Current.Left;
    }
  }

  public void Reset()
  {
    Current = _root;
    _yieldedStart = true;
  }

  public bool MoveNext()
  {
    if (!_yieldedStart)
    {
      _yieldedStart = true;
      return true;
    }

    if (Current is {Right: not null})
    {
      Current = Current.Right;
      while (Current.Left != null)
      {
        Current = Current.Left;
      }
      return true;
    }

    var p = Current?.Parent;
    while (p != null && Current == p.Right)
    {
      Current = p;
      p = p.Parent;
    }
    Current = p;
    return Current != null;
  }
}

public class BinaryTree<T>
{
  private readonly Node<T> _root;

  public BinaryTree(Node<T> root)
  {
    _root = root;
  }

  public InOrderIterator<T> GetEnumerator()
  {
    return new InOrderIterator<T>(_root);
  }

  public IEnumerable<Node<T>> NaturalInOrder
  {
    get
    {
      foreach (var node in TraverseInOrder(_root))
        yield return node;
      yield break;

      IEnumerable<Node<T>> TraverseInOrder(Node<T> current)
      {
        while (true)
        {
          if (current.Left != null)
          {
            foreach (var left in TraverseInOrder(current.Left)) yield return left;
          }

          yield return current;

          if (current.Right != null)
          {
            current = current.Right;
            continue;
          }

          break;
        }
      }
    }
  }
}

public class OneDAdapter<T> : IEnumerable<T>
  where T : struct
{
  private readonly T[,] _arr;
  private readonly int _w;
  private readonly int _h;

  public OneDAdapter(T[,] arr)
  {
    _arr = arr;
    _w = arr.GetLength(0);
    _h = arr.GetLength(1);
  }

  public IEnumerator<T> GetEnumerator()
  {
    for (var y = 0; y < _h; ++y)
    {
      for (var x = 0; x < _w; ++x)
      {
        yield return _arr[x, y];
      }
    }
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
