using MoreLinq;
using static System.Console;

_ = MyDatabase.Instance;
_ = MyDatabase.Instance;

WriteLine();

var db = SingletonDatabase.Instance;

// works just fine while you're working with a real database.
const string city = "Tokyo";
WriteLine($"{city} has population {db.GetPopulation(city)}");

WriteLine();

var rf = new SingletonRecordFinder();
var names = new[] {"Seoul", "Mexico City"};
WriteLine($"Total population: {rf.TotalPopulation(names)}");

var db2 = new DummyDatabase();
var rf2 = new ConfigurableRecordFinder(db2);
WriteLine($"Total population: {rf2.GetTotalPopulation(new [] {"alpha", "gamma"})}");

WriteLine();

_ = new ChiefExecutiveOfficer
{
  Name = "Adam Smith",
  Age = 55
};

var ceo2 = new ChiefExecutiveOfficer();
WriteLine(ceo2);

WriteLine();

_ = Printer.Get(Subsystem.Main);
var backup = Printer.Get(Subsystem.Backup);

var backupAgain = Printer.Get(Subsystem.Backup);

WriteLine($"Same ref: {ReferenceEquals(backup, backupAgain)}");

public class MyDatabase
{
  private MyDatabase()
  {
    WriteLine("Initializing database");
  }

  private static readonly Lazy<MyDatabase> InternalInstance =
    new(() => new MyDatabase());

  public static MyDatabase Instance => InternalInstance.Value;
}

public interface IDatabase
{
  int GetPopulation(string name);
}

public class SingletonDatabase : IDatabase
{
  private readonly Dictionary<string, int> _capitals;

  private SingletonDatabase()
  {
    WriteLine("Initializing database");

    _capitals = File.ReadAllLines(
        Path.Combine(
          new FileInfo(typeof(IDatabase).Assembly.Location)
            .DirectoryName!,
          "capitals.txt")
      )
      .Batch(2)
      .ToDictionary(
        list => list.ElementAt(0).Trim(),
        list => int.Parse(list.ElementAt(1)));
  }

  public int GetPopulation(string name)
  {
    return _capitals[name];
  }

  // laziness + thread safety
  private static readonly Lazy<SingletonDatabase> InternalInstance =
    new(() => new SingletonDatabase());

  public static IDatabase Instance => InternalInstance.Value;
}

public class SingletonRecordFinder
{
  public int TotalPopulation(IEnumerable<string> names)
  {
    return names.Sum(name => SingletonDatabase.Instance.GetPopulation(name));
  }
}

public class ConfigurableRecordFinder
{
  private readonly IDatabase _database;

  public ConfigurableRecordFinder(IDatabase database)
  {
    _database = database;
  }

  public int GetTotalPopulation(IEnumerable<string> names)
  {
    return names.Sum(name => _database.GetPopulation(name));
  }
}

public class DummyDatabase : IDatabase
{
  public int GetPopulation(string name)
  {
    return new Dictionary<string, int>
    {
      ["alpha"] = 1,
      ["beta"] = 2,
      ["gamma"] = 3
    }[name];
  }
}

public class ChiefExecutiveOfficer
{
  private static string? _name;
  private static int _age;

  public string? Name
  {
    get => _name;
    set => _name = value;
  }

  public int Age
  {
    get => _age;
    set => _age = value;
  }

  public override string ToString()
  {
    return $"{nameof(Name)}: {Name}, {nameof(Age)}: {Age}";
  }
}

public enum Subsystem
{
  Main,
  Backup
}

public class Printer
{
  private Printer() { }

  public static Printer Get(Subsystem ss)
  {
    if (Instances.TryGetValue(ss, out var singleInstance))
      return singleInstance;

    var instance = new Printer();
    Instances[ss] = instance;
    return instance;
  }

  private static readonly Dictionary<Subsystem, Printer> Instances = new();
}
