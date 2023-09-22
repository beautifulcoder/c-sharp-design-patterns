using System.Text;
using static System.Console;

{
  var firstNames = Enumerable.Range(0, 100).Select(_ => RandomString());
  var lastNames = Enumerable.Range(0, 100).Select(_ => RandomString()).ToList();

  var names = (from firstName in firstNames
      from lastName in lastNames
      select new User($"{firstName} {lastName}"))
    .ToList();

  for (var i = 0; i < 4; i++)
  {
    WriteLine(names[i].FullName);
  }

  ForceGc();
}

WriteLine();

{
  var firstNames = Enumerable.Range(0, 100).Select(_ => RandomString());
  var lastNames = Enumerable.Range(0, 100).Select(_ => RandomString()).ToList();

  var names = (from firstName in firstNames
      from lastName in lastNames
      select new User2($"{firstName} {lastName}"))
    .ToList();

  for (var i = 0; i < 4; i++)
  {
    WriteLine(names[i].FullName);
  }


  ForceGc();
}

WriteLine();

{
  var ft = new FormattedText("This is a brave new world");
  ft.Capitalize(10, 15);
  WriteLine(ft);

  var bft = new BetterFormattedText("This is a brave new world");
  bft.GetRange(10, 15).Capitalize = true;
  WriteLine(bft);
}

return;

static string RandomString()
{
  Random rand = new();
  return new string(
    Enumerable.Range(0, 10).Select(_ => (char) ('a' + rand.Next(26))).ToArray());
}

void ForceGc()
{
  GC.Collect();
  GC.WaitForPendingFinalizers();
  GC.Collect();
}

public class User
{
  public string FullName { get; }

  public User(string fullName)
  {
    FullName = fullName;
  }
}

public class User2
{
  private static readonly List<string> Strings = new();
  private readonly int[] _names;

  public User2(string fullName)
  {
    _names = fullName.Split(' ').Select(GetOrAdd).ToArray();
    return;

    int GetOrAdd(string s)
    {
      var idx = Strings.IndexOf(s);
      if (idx != -1) return idx;

      Strings.Add(s);
      return Strings.Count - 1;
    }
  }

  public string FullName => string.Join(" ", _names.Select(i => Strings[i]));
}

public class FormattedText
{
  private readonly string _plainText;

  public FormattedText(string plainText)
  {
    _plainText = plainText;
    _capitalize = new bool[plainText.Length];
  }

  public void Capitalize(int start, int end)
  {
    for (var i = start; i <= end; ++i)
      _capitalize[i] = true;
  }

  private readonly bool[] _capitalize;

  public override string ToString()
  {
    var sb = new StringBuilder();
    for (var i = 0; i < _plainText.Length; i++)
    {
      var c = _plainText[i];
      sb.Append(_capitalize[i] ? char.ToUpper(c) : c);
    }
    return sb.ToString();
  }
}

public class BetterFormattedText
{
  private readonly string _plainText;
  private readonly List<TextRange> _formatting = new();

  public BetterFormattedText(string plainText)
  {
    _plainText = plainText;
  }

  public TextRange GetRange(int start, int end)
  {
    var range = new TextRange {Start = start, End = end};
    _formatting.Add(range);
    return range;
  }

  public override string ToString()
  {
    var sb = new StringBuilder();

    for (var i = 0; i < _plainText.Length; i++)
    {
      var c = _plainText[i];
      c = _formatting
        .Where(range => range.Covers(i) && range.Capitalize)
        .Aggregate(c, (current, _) => char.ToUpperInvariant(current));

      sb.Append(c);
    }

    return sb.ToString();
  }

  public class TextRange
  {
    public int Start, End;
    public bool Capitalize, Bold, Italic;

    public bool Covers(int position)
    {
      return position >= Start && position <= End;
    }
  }
}
