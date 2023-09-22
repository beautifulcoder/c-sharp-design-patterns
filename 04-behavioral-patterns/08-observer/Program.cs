using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Console;

var person = new Person();
person.FallsIll += CallDoctor;
person.CatchACold();

WriteLine();

var btn = new Button();
var window = new Window(btn);
var windowRef = new WeakReference(window);
btn.Fire();

WriteLine("Setting window to null");
window = null;

GC.Collect();
WriteLine($"Window {(window == null ? "null" : "not null")} alive? {windowRef.IsAlive}");

WriteLine();

_ = new Demo();

WriteLine();

var product = new Product{Name="Book"};
var window2 = new Window2{ProductName = "Book"};

// want to ensure that when product name changes
// in one component, it also changes in another

product.PropertyChanged += (_, eventArgs) =>
{
  if (eventArgs.PropertyName == "Name")
  {
    WriteLine("Name changed in Product");
    window2.ProductName = product.Name;
  }
};

window2.PropertyChanged += (_, eventArgs) =>
{
  if (eventArgs.PropertyName == "ProductName")
  {
    WriteLine("Name changed in Window");
    product.Name = window2.ProductName;
  }
};

using var binding = new BidirectionalBinding(
  product,
  () => product.Name,
  window2,
  () => window2.ProductName);

// there is no infinite loop because of
// self-assignment guard
product.Name = "Table";
window2.ProductName = "Chair";

WriteLine(product);
WriteLine(window2);

WriteLine();

var p = new Person3();
p.PropertyChanged += (_, eventArgs) =>
{
  WriteLine($"{eventArgs.PropertyName} has changed");
};
p.Age = 15; // should not really affect CanVote :)
p.Citizen = true;
WriteLine($"Can vote: {p.CanVote}");

return;

static void CallDoctor(object? sender, FallsIllEventArgs eventArgs)
{
  WriteLine($"A doctor has been called to {eventArgs.Address}");
}

public class FallsIllEventArgs : EventArgs
{
  public string? Address;
}

public class Person
{
  public void CatchACold()
  {
    FallsIll?.Invoke(
      this,
      new FallsIllEventArgs {Address = "123 London Road"});
  }

  public event EventHandler<FallsIllEventArgs>? FallsIll;
}

public class Button
{
  public event EventHandler? Clicked;

  public void Fire()
  {
    Clicked?.Invoke(this, EventArgs.Empty);
  }
}

public class Window
{
  public Window(Button button)
  {
    button.Clicked += ButtonOnClicked;
  }

  private static void ButtonOnClicked(object? sender, EventArgs eventArgs)
  {
    WriteLine("Button clicked (Window handler)");
  }

  ~Window()
  {
    WriteLine("Window finalized");
  }
}

public class Event
{
  // something that can happen
}

public class FallsIllEvent2 : Event
{
  public string? Address;
}

public class Person2 : IObservable<Event>
{
  private readonly HashSet<Subscription> _subscriptions = new();

  public IDisposable Subscribe(IObserver<Event> observer)
  {
    var subscription = new Subscription(this,observer);
    _subscriptions.Add(subscription);

    return subscription;
  }

  public void CatchACold()
  {
    foreach (var sub in _subscriptions)
    {
      sub.Observer.OnNext(new FallsIllEvent2 {Address = "123 London Road"});
    }
  }

  private class Subscription : IDisposable
  {
    private readonly Person2 _person;
    public readonly IObserver<Event> Observer;

    public Subscription(Person2 person, IObserver<Event> observer)
    {
      _person = person;
      Observer = observer;
    }

    public void Dispose()
    {
      _person._subscriptions.Remove(this);
    }
  }
}

public class Demo : IObserver<Event>
{
  public Demo()
  {
    var person = new Person2();
    person.Subscribe(this);

    person.CatchACold();
  }

  public void OnNext(Event value)
  {
    if (value is FallsIllEvent2 args)
    {
      WriteLine($"A doctor has been called to {args.Address}");
    }
  }

  public void OnError(Exception error){}
  public void OnCompleted(){}
}

public class Product : INotifyPropertyChanged
{
  private string? _name;

  public string? Name
  {
    get => _name;

    set
    {
      if (value == _name)
      {
        return;
      }

      _name = value;
      OnPropertyChanged();
    }
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(
      this,
      new PropertyChangedEventArgs(propertyName));
  }

  public override string ToString()
  {
    return $"Product: {Name}";
  }
}

public class Window2 : INotifyPropertyChanged
{
  private string? _productName;

  public string? ProductName
  {
    get => _productName;

    set
    {
      if (value == _productName)
      {
        return;
      }

      _productName = value;
      OnPropertyChanged();
    }
  }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged(
    [CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(
      this,
      new PropertyChangedEventArgs(propertyName));
  }

  public override string ToString()
  {
    return $"Window: {ProductName}";
  }
}

public sealed class BidirectionalBinding : IDisposable
{
  private bool _disposed;

  public BidirectionalBinding(
    INotifyPropertyChanged first,
    Expression<Func<object>> firstProperty,
    INotifyPropertyChanged second,
    Expression<Func<object>> secondProperty)
  {
    if (firstProperty.Body is MemberExpression firstExpr 
        && secondProperty.Body is MemberExpression secondExpr)
    {
      if (firstExpr.Member is PropertyInfo firstProp
          && secondExpr.Member is PropertyInfo secondProp)
      {
        first.PropertyChanged += (_, _) =>
        {
          if (!_disposed)
          {
            secondProp.SetValue(second, firstProp.GetValue(first));
          }
        };

        second.PropertyChanged += (_, _) =>
        {
          if (!_disposed)
          {
            firstProp.SetValue(first, secondProp.GetValue(second));
          }
        };
      }
    }
  }

  public void Dispose()
  {
    _disposed = true;
  }
}

public class Person3 : PropertyNotificationSupport
{
  private int _age;

  public int Age
  {
    get => _age;
    set => SetValue(value, ref _age);
  }

  public bool Citizen
  {
    get => _citizen;
    set => SetValue(value, ref _citizen);
  }

  private readonly Func<bool> _canVote;
  private bool _citizen;
  public bool CanVote => _canVote();

  public Person3()
  {
    _canVote = Property(
      nameof(CanVote),
      () => Citizen && Age >= 16);
  }
}

public class PropertyNotificationSupport :
  INotifyPropertyChanged,
  INotifyPropertyChanging
{
  private readonly Dictionary<string, HashSet<string>> _affectedBy = new();

  public event PropertyChangedEventHandler? PropertyChanged;

  protected virtual void OnPropertyChanged
    ([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    foreach (var affected in _affectedBy.Keys)
    {
      if (_affectedBy[affected].Contains(propertyName!))
      {
        OnPropertyChanged(affected);
      }
    }
  }

  protected Func<T> Property<T>(string name, Expression<Func<T>> expr)
  {
    WriteLine($"Creating computed property for expression {expr}");

    var visitor = new MemberAccessVisitor(GetType());
    visitor.Visit(expr);

    if (visitor.PropertyNames.Any())
    {
      if (!_affectedBy.ContainsKey(name))
      {
        _affectedBy.Add(name, new HashSet<string>());
      }

      foreach (var propName in visitor.PropertyNames)
      {
        if (propName != name)
        {
          _affectedBy[name].Add(propName);
        }
      }
    }

    return expr.Compile();
  }

  private class MemberAccessVisitor : ExpressionVisitor
  {
    private readonly Type _declaringType;
    public readonly IList<string> PropertyNames = new List<string>();

    public MemberAccessVisitor(Type declaringType)
    {
      _declaringType = declaringType;
    }

    public override Expression Visit(Expression? expr)
    {
      if (expr is {NodeType: ExpressionType.MemberAccess})
      {
        var memberExpr = (MemberExpression)expr;
        if (memberExpr.Member.DeclaringType == _declaringType)
        {
          PropertyNames.Add(memberExpr.Member.Name);
        }
      }

      return base.Visit(expr!);
    }
  }

  public event PropertyChangingEventHandler? PropertyChanging;

  protected virtual void OnPropertyChanging(
    [CallerMemberName] string? propertyName = null)
  {
    PropertyChanging?.Invoke(
      this,
      new PropertyChangingEventArgs(propertyName));
  }

  protected void SetValue<T>(
    T value,
    ref T field,
    [CallerMemberName] string? propertyName = null)
  {
    if (value!.Equals(field)) return;

    OnPropertyChanging(propertyName);
    field = value;
    OnPropertyChanged(propertyName);
  }
}
