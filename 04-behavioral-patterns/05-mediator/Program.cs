using MediatR;
using Microsoft.Extensions.DependencyInjection;
using static System.Console;

var room = new ChatRoom();

var john = new Person("John");
var jane = new Person("Jane");

room.Join(john);
room.Join(jane);

john.Say("hi room");
jane.Say("oh, hey john");

var simon = new Person("Simon");
room.Join(simon);
simon.Say("hi everyone!");

jane.PrivateMessage("Simon", "glad you could join us!");

var game = new Game();
var player = new Player("Sam", game);
_ = new Coach(game);

WriteLine();

player.Score(); // coach says: well done, Sam
player.Score(); // coach says: well done, Sam
player.Score(); // ignored by coach

WriteLine();

var services = new ServiceCollection();
services.AddSingleton<IMediator, Mediator>();
services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));

var container = services.BuildServiceProvider();
var mediator = container.GetRequiredService<IMediator>();

var response = await mediator.Send(new PingCommand());

WriteLine($"We got a pong at {response.Timestamp}");

public class Person
{
  public string Name;
  public ChatRoom Room = default!;

  public Person(string name) => Name = name;

  public void Receive(string sender, string message)
  {
    var s = $"{sender}: '{message}'";
    WriteLine($"[{Name}'s chat session] {s}");
  }

  public void Say(string message) => Room.Broadcast(Name, message);

  public void PrivateMessage(string who, string message)
  {
    Room.Message(Name, who, message);
  }
}

public class ChatRoom
{
  private readonly List<Person> _people = new();

  public void Broadcast(string source, string message)
  {
    foreach (var p in _people.Where(p => p.Name != source))
    {
      p.Receive(source, message);
    }
  }

  public void Join(Person p)
  {
    var joinMsg = $"{p.Name} joins the chat";
    Broadcast("room", joinMsg);

    p.Room = this;
    _people.Add(p);
  }

  public void Message(string source, string destination, string message)
  {
    _people
      .FirstOrDefault(p => p.Name == destination)
      ?.Receive(source, message);
  }
}

public abstract class GameEventArgs : EventArgs
{
  public abstract void Print();
}

public class PlayerScoredEventArgs : GameEventArgs
{
  public string PlayerName;
  public int GoalsScoredSoFar;

  public PlayerScoredEventArgs
    (string playerName, int goalsScoredSoFar)
  {
    PlayerName = playerName;
    GoalsScoredSoFar = goalsScoredSoFar;
  }

  public override void Print()
  {
    WriteLine($"{PlayerName} has scored! " +
      $"(their {GoalsScoredSoFar} goal)");
  }
}

public class Game
{
  public event EventHandler<GameEventArgs>? Events;

  public void Fire(GameEventArgs args)
  {
    Events?.Invoke(this, args);
  }
}

public class Player
{
  private readonly string _name;
  public int GoalsScored;
  private readonly Game _game;

  public Player(string name, Game game)
  {
    _name = name;
    _game = game;
  }

  public void Score()
  {
    GoalsScored++;
    var args = new PlayerScoredEventArgs(_name, GoalsScored);
    _game.Fire(args);
  }
}

public class Coach
{
  public Coach(Game game)
  {
    // celebrate if player has scored <3 goals
    game.Events += (_, args) =>
    {
      if (args is PlayerScoredEventArgs {GoalsScoredSoFar: < 3} scored)
      {
        WriteLine($"coach says: well done, {scored.PlayerName}");
      }
    };
  }
}

public record struct PongResponse(DateTime Timestamp);

public class PingCommand : IRequest<PongResponse>
{
  // nothing here
}

public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
  public async Task<PongResponse> Handle(
    PingCommand request,
    CancellationToken cancellationToken)
  {
    return await Task
      .FromResult(new PongResponse(DateTime.UtcNow))
      .ConfigureAwait(false);
  }
}
