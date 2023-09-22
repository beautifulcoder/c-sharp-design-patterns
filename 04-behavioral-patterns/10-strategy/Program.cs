using System.Text;
using static System.Console;

var tp = new TextProcessor();
tp.SetOutputFormat(OutputFormat.Markdown);
tp.AppendList(new []{"foo", "bar", "baz"});
WriteLine(tp);

tp.Clear();
tp.SetOutputFormat(OutputFormat.Html);
tp.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp);

WriteLine();

var tp2 = new TextProcessor<MarkdownListStrategy>();
tp2.AppendList(new []{"foo", "bar", "baz"});
WriteLine(tp2);

var tp3 = new TextProcessor<HtmlListStrategy>();
tp3.AppendList(new[] { "foo", "bar", "baz" });
WriteLine(tp3);

WriteLine();

var people = new List<Person>
{
  new(2, "Barry", 27),
  new(3, "Ana", 20),
  new(1, "Carl", 5)
};

people.Sort();
people.ForEach(WriteLine);
WriteLine();

people.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
people.ForEach(WriteLine);
WriteLine();

people.Sort(Person.NameComparer);
people.ForEach(WriteLine);
WriteLine();

public enum OutputFormat
{
  Markdown,
  Html
}

public interface IListStrategy
{
  void Start(StringBuilder sb);
  void End(StringBuilder sb);
  void AddListItem(StringBuilder sb, string item);
}

public class MarkdownListStrategy : IListStrategy
{
  public void Start(StringBuilder sb)
  {
    // markdown doesn't require a list preamble
  }

  public void End(StringBuilder sb)
  {
  }

  public void AddListItem(StringBuilder sb, string item)
  {
    sb.AppendLine($" * {item}");
  }
}

public class HtmlListStrategy : IListStrategy
{
  public void Start(StringBuilder sb) => sb.AppendLine("<ul>");

  public void End(StringBuilder sb) => sb.AppendLine("</ul>");

  public void AddListItem(StringBuilder sb, string item)
  {
    sb.AppendLine($"  <li>{item}</li>");
  }
}

public class TextProcessor
{
  private readonly StringBuilder _sb = new();
  private IListStrategy _listStrategy = default!;

  public void SetOutputFormat(OutputFormat format)
  {
    _listStrategy = format switch
    {
      OutputFormat.Markdown => new MarkdownListStrategy(),
      OutputFormat.Html => new HtmlListStrategy(),
      _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };
  }

  public void AppendList(IEnumerable<string> items)
  {
    _listStrategy.Start(_sb);
    foreach (var item in items)
      _listStrategy.AddListItem(_sb, item);
    _listStrategy.End(_sb);
  }

  public StringBuilder Clear()
  {
    return _sb.Clear();
  }

  public override string ToString() => _sb.ToString();
}

public class TextProcessor<TL> where TL : IListStrategy, new()
{
  private readonly StringBuilder _sb = new();
  private readonly IListStrategy _listStrategy = new TL();

  public void AppendList(IEnumerable<string> items)
  {
    _listStrategy.Start(_sb);
    foreach (var item in items)
      _listStrategy.AddListItem(_sb, item);
    _listStrategy.End(_sb);
  }

  public override string ToString()
  {
    return _sb.ToString();
  }
}

public class Person : IEquatable<Person>, IComparable<Person>
{
  public readonly int Id;
  public string Name;
  public int Age;

  public int CompareTo(Person? other)
  {
    if (ReferenceEquals(this, other)) return 0;
    if (ReferenceEquals(null, other)) return 1;
    return Id.CompareTo(other.Id);
  }

  public Person(int id, string name, int age)
  {
    Id = id;
    Name = name;
    Age = age;
  }

  public bool Equals(Person? other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return Id == other.Id;
  }

  public override bool Equals(object? obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((Person) obj);
  }

  public override int GetHashCode()
  {
    return Id;
  }

  public static bool operator ==(Person left, Person right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Person left, Person right)
  {
    return !Equals(left, right);
  }

  private struct NameRelationalComparer : IComparer<Person>
  {
    public int Compare(Person? x, Person? y)
    {
      if (ReferenceEquals(x, y)) return 0;
      if (ReferenceEquals(null, y)) return 1;
      if (ReferenceEquals(null, x)) return -1;
      return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }
  }

  public static IComparer<Person> NameComparer { get; } = new NameRelationalComparer();

  public override string ToString() => $"Id: {Id}\tName: {Name}\tAge: {Age}";
}
