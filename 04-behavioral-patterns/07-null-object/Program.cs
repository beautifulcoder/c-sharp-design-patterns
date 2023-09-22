using System.Dynamic;
using ImpromptuInterface;
using static System.Console;

var log1 = new NullLog();
var ba1 = new BankAccount(log1);
ba1.Deposit(100);
ba1.Withdraw(200);

var log2 = new ConsoleLog();
var ba2 = new BankAccount(log2);
ba2.Deposit(100);
ba2.Withdraw(200);

var log3 = Null<ILog>.Instance;
var ba3 = new BankAccount(log3);
ba3.Deposit(100);
ba3.Withdraw(200);

public interface ILog
{
  void Info(string msg);
  void Warn(string msg);
}

public sealed class NullLog : ILog
{
  public void Info(string msg) { }
  public void Warn(string msg) { }
}

public class ConsoleLog : ILog
{
  public void Info(string msg)
  {
    WriteLine(msg);
  }

  public void Warn(string msg)
  {
    WriteLine("WARNING: " + msg);
  }
}

public class OptionalLog: ILog
{
  private readonly ILog? _impl;

  public OptionalLog(ILog? impl)
  {
    _impl = impl;
  }

  public void Info(string msg)
  {
    _impl?.Info(msg);
  }

  public void Warn(string msg)
  {
    _impl?.Warn(msg);
  }
}

public class Null<T> : DynamicObject where T : class
{
  public static T Instance
  {
    get
    {
      if (!typeof(T).IsInterface)
      {
        throw new ArgumentException("I must be an interface type");
      }

      return new Null<T>().ActLike<T>();
    }
  }

  public override bool TryInvokeMember(
    InvokeMemberBinder binder,
    object?[]? args,
    out object? result)
  {
    result = Activator.CreateInstance(binder.ReturnType);

    return true;
  }
}

public class BankAccount
{
  private readonly ILog _log;
  private int _balance;

  private const ILog? NoLogging = null;

  public BankAccount(ILog? log = NoLogging)
  {
    _log = new OptionalLog(log);
  }

  public void Deposit(int amount)
  {
    _balance += amount;
    // check for null everywhere
    _log.Info($"Deposited ${amount}, balance is now {_balance}");
  }

  public void Withdraw(int amount)
  {
    if (_balance >= amount)
    {
      _balance -= amount;
      _log.Info($"Withdrew ${amount}, we have ${_balance} left");
    }
    else
    {
      _log.Warn($"Could not withdraw ${amount} because " +
                $"balance is only ${_balance}");
    }
  }
}
