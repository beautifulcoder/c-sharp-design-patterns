var gen = new MagicSquareGenerator();
var square = gen.Generate(3);
Console.WriteLine(gen.Print(square));

Console.WriteLine();

var uniqueSquare = gen.Generate<UniqueGenerator, Splitter, Verifier>(3);
Console.WriteLine(gen.Print(uniqueSquare));

public class Generator
{
  private static readonly Random Random = new();

  public virtual List<int> Generate(int count)
  {
    return Enumerable.Range(0, count)
      .Select(_ => Random.Next(1, 6))
      .ToList();
  }
}

public class UniqueGenerator : Generator
{
  public override List<int> Generate(int count)
  {
    List<int> result;

    do
    {
      result = base.Generate(count);
    } while (result.Distinct().Count() != result.Count);

    return result;
  }
}

public class Splitter
{
  public List<List<int>> Split(List<List<int>> array)
  {
    var result = new List<List<int>>();

    var rowCount = array.Count;
    var colCount = array[0].Count;

    // get the rows
    for (var r = 0; r < rowCount; ++r)
    {
      var theRow = new List<int>();
      for (var c = 0; c < colCount; ++c)
        theRow.Add(array[r][c]);
      result.Add(theRow);
    }

    // get the columns
    for (var c = 0; c < colCount; ++c)
    {
      var theCol = new List<int>();
      for (var r = 0; r < rowCount; ++r)
        theCol.Add(array[r][c]);
      result.Add(theCol);
    }

    // now the diagonals
    var diag1 = new List<int>();
    var diag2 = new List<int>();
    for (var c = 0; c < colCount; ++c)
    {
      for (var r = 0; r < rowCount; ++r)
      {
        if (c == r)
          diag1.Add(array[r][c]);
        var r2 = rowCount - r - 1;
        if (c == r2)
          diag2.Add(array[r][c]);
      }
    }

    result.Add(diag1);
    result.Add(diag2);

    return result;
  }
}

public class Verifier
{
  public bool Verify(List<List<int>> array)
  {
    if (!array.Any()) return false;

    var expected = array.First().Sum();

    return array.All(t => t.Sum() == expected);
  }
}

public class MagicSquareGenerator
{
  public List<List<int>> Generate(int size)
  {
    return Generate<Generator, Splitter, Verifier>(size);
  }

  public List<List<int>> Generate<TGenerator, TSplitter, TVerifier>(int size)
    where TGenerator : Generator, new()
    where TSplitter : Splitter, new()
    where TVerifier : Verifier, new()
  {
    var g = new TGenerator();
    var s = new TSplitter();
    var v = new TVerifier();

    List<List<int>> square;

    do
    {
      square = new List<List<int>>();
      for (var i = 0; i < size; ++i)
        square.Add(g.Generate(size));
    } while (!v.Verify(s.Split(square)));

    return square;
  }

  public string Print(List<List<int>> square)
  {
    return string.Join(Environment.NewLine, square.Select(x => string.Join(" ", x)));
  }
}
