var j = new Journal();
j.AddEntry("I cried today.");
j.AddEntry("I ate a bug.");

var apple = new Product("Apple", Color.Green, Size.Small);
var tree = new Product("Tree", Color.Green, Size.Large);
var house = new Product("House", Color.Blue, Size.Large);

var products = new [] { apple, tree, house };
var bf = new BetterFilter();

Console.WriteLine("Green products:");
foreach (var p in bf.Filter(products, new ColorSpecification(Color.Green)))
{
  Console.WriteLine($" - {p.Name} is green");
}

var rc = new Rectangle(2, 3);
UseIt(rc);

var sq = new Square(5);
UseIt(sq);

static void UseIt(Rectangle r)
{
  r.Height = 10;
  Console.WriteLine($"Expected area of {10 * r.Width}, got {r.Area}");
}

// Single Responsibility
public class Journal
{
  private readonly List<string> entries = new();
  private static int count = 0;

  public void AddEntry(string text)
  {
    entries.Add($"{++count}: {text}");
  }

  public void RemoveEntry(int index)
  {
    entries.RemoveAt(index);
  }

  public void Save(string filename, bool overwrite = false)
  {
    File.WriteAllText(filename, ToString());
  }
}

public class PersistenceManager
{
  public void SaveToFile(Journal journal, string filename, bool overwrite = false)
  {
    if (overwrite || !File.Exists(filename))
    {
      File.WriteAllText(filename, journal.ToString());
    }
  }
}

// Open-Closed Principle
public enum Color
{
  Red, Green, Blue
}

public enum Size
{
  Small, Medium, Large, Yuge
}

public class Product
{
  public string Name;
  public Color Color;
  public Size Size;

  public Product(string name, Color color, Size size)
  {
    Name = name;
    Color = color;
    Size = size;
  }
}

public class ProductFilter
{
  public IEnumerable<Product> FilterByColor(IEnumerable<Product> products, Color color)
  {
    foreach (var p in products)
    {
      if (p.Color == color)
      {
        yield return p;
      }
    }
  }

  public IEnumerable<Product> FilterBySize(IEnumerable<Product> products, Size size)
  {
    foreach (var p in products)
    {
      if (p.Size == size)
      {
        yield return p;
      }
    }
  }

  public IEnumerable<Product> FilterBySizeAndColor(
    IEnumerable<Product> products,
    Size size,
    Color color)
  {
    foreach (var p in products)
    {
      if (p.Size == size && p.Color == color)
      {
        yield return p;
      }
    }
  }
}

public interface ISpecification<T>
{
  bool IsSatisfied(T item);
}

public interface IFilter<T>
{
  IEnumerable<T> Filter(IEnumerable<T> items, ISpecification<T> spec);
}

public class BetterFilter : IFilter<Product>
{
  public IEnumerable<Product> Filter(IEnumerable<Product> items, ISpecification<Product> spec)
  {
    foreach (var i in items)
    {
      if (spec.IsSatisfied(i))
      {
        yield return i;
      }
    }
  }
}

public class ColorSpecification : ISpecification<Product>
{
  private Color color;

  public ColorSpecification(Color color)
  {
    this.color = color;
  }

  public bool IsSatisfied(Product p)
  {
    return p.Color == color;
  }
}

// Liskov Substitution Principle

public class Rectangle
{
  public int Width { get; set; }
  public int Height { get; set; }

  public Rectangle()
  {
  }

  public Rectangle(int width, int height)
  {
    Width = width;
    Height = height;
  }

  public int Area => Width * Height;
}

public class Square : Rectangle
{
  public Square(int side)
  {
    Width = Height = side;
  }

  public new int Width
  {
    set { base.Width = base.Height = value; }
  }

  public new int Height
  {
    set { base.Width = base.Height = value; }
  }
}

// Interface Segregation Principle

public interface IPrinter
{
  void Print(object d);
}

public interface IScanner
{
  void Scan(object d);
}

public class Printer : IPrinter
{
  public void Print(object d)
  {
  }
}

public class Photocopier : IPrinter, IScanner
{
  public void Print(object d)
  {
  }

  public void Scan(object d)
  {
  }
}

public interface IMultiFunctionDevice : IPrinter, IScanner
{
}

public class MultiFunctionMachine : IMultiFunctionDevice
{
  private IPrinter printer;
  private IScanner scanner;

  public MultiFunctionMachine(IPrinter printer, IScanner scanner)
  {
    this.printer = printer;
    this.scanner = scanner;
  }

  public void Print(object d)
  {
    printer.Print(d);
  }

  public void Scan(object d)
  {
    scanner.Scan(d);
  }
}

public class MyParams
{
  public int a;
  public int b;
  public bool c = false;
  public int d = 42;
  public float e = 1.0f;

  public MyParams(int a, int b)
  {
    this.a = a;
    this.b = b;
  }
}

public class Foo
{
  public Foo(MyParams myParams)
  {
  }
}

// Dependency Inversion Principle

public enum Relationship
{
  Parent,
  Child,
  Sibling
}

public class Person
{
  public string Name = default!;
}

public interface IRelationshipBrowser
{
  IEnumerable<Person> FindAllChildrenOf(string name);
}

public class Relationships : IRelationshipBrowser
{
  private List<(Person, Relationship, Person)> relations = new();

  public IEnumerable<Person> FindAllChildrenOf(string name)
  {
    return relations
      .Where(x => x.Item1.Name == name && x.Item2 == Relationship.Parent)
      .Select(r => r.Item3);
  }
}

public class Research
{
  public Research(IRelationshipBrowser browser)
  {
    foreach (var p in browser.FindAllChildrenOf("John"))
    {
      Console.WriteLine($"John has a child called {p.Name}");
    }
  }
}
