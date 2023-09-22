using System.Diagnostics;
using System.Dynamic;
using System.Text;
using ImpromptuInterface;

var car = new CarProxy(new Driver(12));
car.Drive();

var c = new Creature
{
  Agility = 12
};
Console.WriteLine(c);

Console.WriteLine(10m * 5.Percent());
Console.WriteLine(2.Percent() + 3m.Percent());

Console.WriteLine();

var creatures = new BadCreature[] { new() };
foreach (var creature in creatures)
{
  creature.X++; // not memory-efficient
}

var creatures2 = new Creatures(100);
foreach (var creature in creatures2)
{
  creature.X++;
}

var img = new LazyBitmap("pokemon.png");
Console.WriteLine("About to draw the image");
img.Draw();
Console.WriteLine("Done drawing the image");

Console.WriteLine();

var ba = Log<BankAccount>.As<IBankAccount>();

ba.Deposit(100);
ba.Withdraw(50);

Console.WriteLine(ba);

public class Car : ICar
{
  public void Drive()
  {
    Console.WriteLine("Car being driven");
  }
}

public interface ICar
{
  void Drive();
}

public class Driver
{
  public int Age { get; set; }

  public Driver(int age)
  {
    Age = age;
  }
}

public class CarProxy : ICar
{
  private readonly Car _car = new();
  private readonly Driver _driver;

  public CarProxy(Driver driver)
  {
    _driver = driver;
  }

  public void Drive()
  {
    if (_driver.Age >= 16)
      _car.Drive();
    else
      Console.WriteLine("Driver too young");
  }
}

public class Property<T> where T : new()
{
  private T _value;
  private readonly string _name;

  public T Value
  {
    get => _value;

    set
    {
      if (Equals(_value, value)) return;
      Console.WriteLine($"Assigning {value} to {_name}");
      _value = value;
    }
  }

  public Property() : this(default!) {}

  public Property(T value, string name = "")
  {
    _value = value;
    _name = name;
  }

  public static implicit operator T(Property<T> property)
  {
    return property.Value; // int n = p_int
  }

  public static implicit operator Property<T>(T value)
  {
    return new Property<T>(value); // Property<int> p = 123;
  }
}

public class Creature
{
  public Property<int> agility = new(10, nameof(agility));

  public int Agility
  {
    get => agility.Value;
    set => agility.Value = value;
  }

  public override string ToString() => $"{Agility}";
}

[DebuggerDisplay("{value*100.0f}%")]
public readonly record struct Percentage(decimal _value)
{
  private readonly decimal _value = _value;

  public static decimal operator *(decimal f, Percentage p)
  {
    return f * p._value;
  }

  public static Percentage operator +(Percentage a, Percentage b)
  {
    return new Percentage(a._value + b._value);
  }

  public static implicit operator Percentage(int value)
  {
    return value.Percent();
  }

  public override string ToString()
  {
    return $"{_value*100}%";
  }
}

public static class PercentageExtensions
{
  public static Percentage Percent(this int value)
  {
    return new Percentage(value/100.0m);
  }

  public static Percentage Percent(this decimal value)
  {
    return new Percentage(value/100.0m);
  }
}

public class BadCreature
{
  public byte Age;
  public int X, Y;
}

public class Creatures
{
  private readonly int _size;
  private readonly byte [] _age;
  private readonly int[] _x;
  private readonly int[] _y;

  public Creatures(int size)
  {
    _size = size;
    _age = new byte[size];
    _x = new int[size];
    _y = new int[size];
  }

  public readonly struct Creature
  {
    private readonly Creatures _creatures;
    private readonly int _index;

    public Creature(Creatures creatures, int index)
    {
      _creatures = creatures;
      _index = index;
    }

    public ref byte Age => ref _creatures._age[_index];
    public ref int X => ref _creatures._x[_index];
    public ref int Y => ref _creatures._y[_index];
  }


  public Creature this[int index]
    => new(this, index);

  public IEnumerator<Creature> GetEnumerator()
  {
    for (var pos = 0; pos < _size; ++pos)
      yield return new Creature(this, pos);
  }
}

public interface IImage
{
  void Draw();
}

public class Bitmap : IImage
{
  private readonly string _filename;

  public Bitmap(string filename)
  {
    _filename = filename;
    Console.WriteLine($"Loading image from {filename}");
  }

  public void Draw()
  {
    Console.WriteLine($"Drawing image {_filename}");
  }
}

public class LazyBitmap : IImage
{
  private readonly string _filename;
  private Bitmap? _bitmap;

  public LazyBitmap(string filename)
  {
    _filename = filename;
  }

  public void Draw()
  {
    _bitmap ??= new Bitmap(_filename);
    _bitmap.Draw();
  }
}

public interface IBankAccount
{
  void Deposit(int amount);
  bool Withdraw(int amount);
}

public class BankAccount : IBankAccount
{
  private int _balance;
  private const int OverdraftLimit = -500;

  public void Deposit(int amount)
  {
    _balance += amount;
    Console.WriteLine($"Deposited ${amount}, balance is now {_balance}");
  }

  public bool Withdraw(int amount)
  {
    if (_balance - amount < OverdraftLimit) return false;

    _balance -= amount;
    Console.WriteLine($"Withdrew ${amount}, balance is now {_balance}");
    return true;
  }

  public override string ToString()
  {
    return $"{nameof(_balance)}: {_balance}";
  }
}

public class Log<T> : DynamicObject
  where T : class, new()
{
  private readonly T _subject;
  private readonly Dictionary<string, int> _methodCallCount = new();

  protected Log(T subject)
  {
    _subject = subject ?? throw new ArgumentNullException(paramName: nameof(subject));
  }

  public static TI As<TI>(T subject) where TI : class
  {
    if (!typeof(TI).IsInterface)
      throw new ArgumentException("TI must be an interface type");

    return new Log<T>(subject).ActLike<TI>();
  }

  public static TI As<TI>() where TI : class
  {
    if (!typeof(TI).IsInterface)
      throw new ArgumentException("TI must be an interface type");

    return new Log<T>(new T()).ActLike<TI>();
  }

  public override bool TryInvokeMember(
    InvokeMemberBinder binder,
    object?[]? args,
    out object? result)
  {
    try
    {
      Console.WriteLine($"Invoking {_subject.GetType().Name}.{binder.Name} with arguments [{string.Join(",", args ?? Array.Empty<object?>())}]");

      if (_methodCallCount.ContainsKey(binder.Name)) _methodCallCount[binder.Name]++;
      else _methodCallCount.Add(binder.Name, 1);

      result = _subject
        .GetType()
        .GetMethod(binder.Name)
        ?.Invoke(_subject, args);
      return true;
    }
    catch
    {
      result = null;
      return false;
    }
  }

  public string Info
  {
    get
    {
      var sb = new StringBuilder();
      foreach (var kv in _methodCallCount)
        sb.AppendLine($"{kv.Key} called {kv.Value} time(s)");
      return sb.ToString();
    }
  }

  public override string ToString()
  {
    return $"{Info}{_subject}";
  }
}
