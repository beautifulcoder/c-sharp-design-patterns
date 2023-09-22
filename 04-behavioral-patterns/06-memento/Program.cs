using static System.Console;

var ba = new BankAccount(100);
var m1 = ba.Deposit(50);
var m2 = ba.Deposit(25);
WriteLine(ba);

ba.Restore(m1);
WriteLine(ba);

ba.Restore(m2);
WriteLine(ba);

ba.Undo();
WriteLine($"Undo 1: {ba}");
ba.Undo();
WriteLine($"Undo 2: {ba}");
ba.Redo();
WriteLine($"Redo 2: {ba}");

// check that we can alter after multiple undos
ba.Undo();
ba.Undo();
ba.Deposit(100);
WriteLine(ba);
WriteLine(ba.ChangeCount);

public class Memento
{
  public int Balance { get; }

  public Memento(int balance)
  {
    Balance = balance;
  }
}

public class BankAccount
{
  private int _balance;
  private readonly List<Memento> _changes = new();
  private int _current = -1;

  public int ChangeCount => _changes.Count;

  public BankAccount(int balance)
  {
    _balance = balance;
    AddChange(new Memento(balance));
  }

  public Memento Deposit(int amount)
  {
    _balance += amount;
    return AddChange(new Memento(_balance));
  }

  public void Restore(Memento? m)
  {
    if (m == null) return;

    _balance = m.Balance;
    _changes.Add(m);
    _current = _changes.Count - 1;
  }

  public Memento? Undo()
  {
    if (_current <= 0) return null;

    var m = _changes[--_current];
    _balance = m.Balance;

    return m;
  }

  public Memento? Redo()
  {
    if (_current + 1 >= _changes.Count) return null;

    var m = _changes[++_current];
    _balance = m.Balance;

    return m;

  }

  public override string ToString()
  {
    return $"{nameof(_balance)}: {_balance}";
  }

  private Memento AddChange(Memento m)
  {
    if (_changes.Count > _current + 1)
    {
      _changes.RemoveRange(_current + 1, _changes.Count - _current - 1);
    }

    _changes.Add(m);
    ++_current;

    return m;
  }
}
