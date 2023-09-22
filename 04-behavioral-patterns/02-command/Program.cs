var ba = new BankAccount();
var cmd = new BankAccountCommand(ba, BankAccountCommand.Action.Deposit, 100);
cmd.Call();
Console.WriteLine(ba);

Console.WriteLine();

var commands = new List<BankAccountCommand>
{
  new(ba, BankAccountCommand.Action.Deposit, 100),
  new(ba, BankAccountCommand.Action.Withdraw, 1000)
};

Console.WriteLine(ba);

foreach (var c in commands)
{
  c.Call();
}

Console.WriteLine(ba);

foreach (var c in Enumerable.Reverse(commands))
{
  c.Undo();
}

Console.WriteLine();

Console.WriteLine(ba);
var cmdDeposit = new BankAccountCommand(ba, BankAccountCommand.Action.Deposit, 100);
var cmdWithdraw = new BankAccountCommand(ba, BankAccountCommand.Action.Withdraw, 1000);
var composite = new CompositeBankAccountCommand(new[]
{
  cmdDeposit,
  cmdWithdraw
});

composite.Call();
Console.WriteLine(ba);

composite.Undo();
Console.WriteLine(ba);

Console.WriteLine();

var from = new BankAccount();
from.Deposit(100);
var to = new BankAccount();

var mtc = new MoneyTransferCommand(from, to, 1000);
mtc.Call();

Console.WriteLine(from);
Console.WriteLine(to);

mtc.Undo();

Console.WriteLine(from);
Console.WriteLine(to);

Console.WriteLine();

_ = new FunctionalCommand();

public class BankAccount
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

  public override string ToString() => $"{nameof(_balance)}: {_balance}";
}

public interface ICommand
{
  void Call();
  void Undo();
}

public class BankAccountCommand : ICommand
{
  private readonly BankAccount _account;

  public bool Success { get; set; }

  public enum Action
  {
    Deposit, Withdraw
  }

  private readonly Action _action;
  private readonly int _amount;

  public BankAccountCommand(BankAccount account, Action action, int amount)
  {
    _account = account;
    _action = action;
    _amount = amount;
  }

  public void Call()
  {
    switch (_action)
    {
      case Action.Deposit:
        _account.Deposit(_amount);
        Success = true;
        break;

      case Action.Withdraw:
        Success = _account.Withdraw(_amount);
        break;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  public void Undo()
  {
    if (!Success) return;

    switch (_action)
    {
      case Action.Deposit:
        _account.Withdraw(_amount);
        break;

      case Action.Withdraw:
        _account.Deposit(_amount);
        break;

      default:
        throw new ArgumentOutOfRangeException();
    }
  }
}

public class CompositeBankAccountCommand : List<BankAccountCommand>, ICommand
{
  public bool Success { get; set; }

  public CompositeBankAccountCommand()
  {
  }

  public CompositeBankAccountCommand(
    IEnumerable<BankAccountCommand> collection) : base(collection)
  {
  }

  public virtual void Call()
  {
    Success = true;
    ForEach(cmd =>
    {
      cmd.Call();
      Success &= cmd.Success;
    });
  }

  public void Undo()
  {
    foreach (var cmd in ((IEnumerable<BankAccountCommand>)this).Reverse())
    {
      cmd.Undo();
    }
  }
}

public class MoneyTransferCommand : CompositeBankAccountCommand
{
  public MoneyTransferCommand(BankAccount from, BankAccount to, int amount)
  {
    AddRange(new[]
    {
      new BankAccountCommand(from, BankAccountCommand.Action.Withdraw, amount),
      new BankAccountCommand(to, BankAccountCommand.Action.Deposit, amount)
    });
  }

  public override void Call()
  {
    BankAccountCommand? last = null;

    foreach (var cmd in this)
    {
      if (last == null || last.Success)
      {
        cmd.Call();
        last = cmd;
      }
      else
      {
        cmd.Undo();
        break;
      }
    }
  }
}

public class FunctionalCommand
{
  public class BankAccount
  {
    public int Balance;

    public override string ToString() => $"{nameof(Balance)}: {Balance}";
  }

  public void Deposit(BankAccount account, int amount)
  {
    account.Balance += amount;
  }

  public void Withdraw(BankAccount account, int amount)
  {
    if (account.Balance >= amount)
    {
      account.Balance -= amount;
    }
  }

  public FunctionalCommand()
  {
    var ba = new BankAccount();
    var commands = new List<Action>
    {
      () => Deposit(ba, 100),
      () => Withdraw(ba, 100)
    };

    commands.ForEach(c => c());

    Console.WriteLine(ba);
  }
}
