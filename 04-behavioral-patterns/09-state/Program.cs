using Stateless;
using static System.Console;

var ls = new Switch();
ls.On();
ls.Off();
ls.Off();

WriteLine();

var handmade = new HandmadeDemo();
handmade.Main();

WriteLine();

var lightSwitch = new StatelessLightSwitch();
lightSwitch.Main();

public class Switch
{
  public State State = new OffState();
  public void On()  { State.On(this);  }
  public void Off() { State.Off(this); }
}

public abstract class State
{
  public virtual void On(Switch sw)
  {
    WriteLine("Light is already on.");
  }

  public virtual void Off(Switch sw)
  {
    WriteLine("Light is already off.");
  }
}

public class OnState : State
{
  public OnState()
  {
    WriteLine("Light turned on.");
  }

  public override void Off(Switch sw)
  {
    WriteLine("Turning light off...");
    sw.State = new OffState();
  }
}

public class OffState : State
{
  public OffState()
  {
    WriteLine("Light turned off.");
  }

  public override void On(Switch sw)
  {
    WriteLine("Turning light on...");
    sw.State = new OnState();
  }
}

public class HandmadeDemo
{
  private enum State
  {
    OffHook,
    Connecting,
    Connected,
    OnHold,
    OnHook
  }

  private enum Trigger
  {
    CallDialed,
    HungUp,
    CallConnected,
    PlacedOnHold,
    TakenOffHold,
    LeftMessage
  }

  private static readonly Dictionary<State, List<(Trigger, State)>> Rules = new()
  {
    [State.OffHook] = new List<(Trigger, State)>
    {
      (Trigger.CallDialed, State.Connecting)
    },
    [State.Connecting] = new List<(Trigger, State)>
    {
      (Trigger.HungUp, State.OnHook),
      (Trigger.CallConnected, State.Connected)
    },
    [State.Connected] = new List<(Trigger, State)>
    {
      (Trigger.LeftMessage, State.OnHook),
      (Trigger.HungUp, State.OnHook),
      (Trigger.PlacedOnHold, State.OnHold)
    },
    [State.OnHold] = new List<(Trigger, State)>
    {
      (Trigger.TakenOffHold, State.Connected),
      (Trigger.HungUp, State.OnHook)
    }
  };

  public void Main()
  {
    var state = State.OffHook;
    const State exitState = State.OnHook;

    var queue = new Queue<int>(new[]{0, 1, 2, 0, 0});

    do
    {
      WriteLine($"The phone is currently {state}");
      WriteLine("Select a trigger:");

      for (var i = 0; i < Rules[state].Count; i++)
      {
        var (t, _) = Rules[state][i];
        WriteLine($"{i}. {t}");
      }

      var input = queue.Dequeue();
      WriteLine(input);

      var (_, s) = Rules[state][input];
      state = s;
    } while (state != exitState);
    WriteLine("We are done using the phone.");
  }
}

public class StatelessLightSwitch
{
  private enum Trigger
  {
    On, Off
  }

  public void Main()
  {
    // false = off, true = on

    var light = new StateMachine<bool, Trigger>(false);

    light.Configure(false) // if the light is off...
      .Permit(Trigger.On, true) // we can turn it on
      .OnEntry(transition =>
      {
        WriteLine(transition.IsReentry ? "Light is already off!" : "Switching light off");
      })
      .PermitReentry(Trigger.Off)
      .Ignore(Trigger.Off); // but if it's already off we do nothing

    // same for when the light is on
    light.Configure(true)
      .Permit(Trigger.Off, false)
      .OnEntry(() => WriteLine("Turning light on"))
      .Ignore(Trigger.On);

    light.Fire(Trigger.On);  // Turning light on
    light.Fire(Trigger.Off); // Turning light off
    light.Fire(Trigger.Off); // Light is already off!
  }
}
