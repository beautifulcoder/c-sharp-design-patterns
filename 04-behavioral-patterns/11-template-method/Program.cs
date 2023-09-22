using static System.Console;

var chess = new Chess();
chess.Run();

WriteLine();

var demo = new GameDemo();
demo.Main();

public abstract class Game
{
  public void Run()
  {
    Start();

    while (!HaveWinner)
    {
      TakeTurn();
    }

    WriteLine($"Player {WinningPlayer} wins.");
  }

  protected abstract void Start();
  protected abstract bool HaveWinner { get; }
  protected abstract void TakeTurn();
  protected abstract int WinningPlayer { get; }

  protected Game(int numberOfPlayers)
  {
    NumberOfPlayers = numberOfPlayers;
  }

  protected int CurrentPlayer;

  protected readonly int NumberOfPlayers;
}

public class Chess : Game
{
  public Chess() : base(2)
  {
  }

  protected override void Start()
  {
    WriteLine($"Starting a game of chess with {NumberOfPlayers} players.");
  }

  protected override bool HaveWinner => _turn == MaxTurns;

  protected override void TakeTurn()
  {
    WriteLine($"Turn {_turn++} taken by player {CurrentPlayer}.");
    CurrentPlayer = (CurrentPlayer + 1) % NumberOfPlayers;
  }

  protected override int WinningPlayer => CurrentPlayer;

  private const int MaxTurns = 10;
  private int _turn = 1;
}

public static class GameTemplate
{
  public static void Run(
    Action start,
    Action takeTurn,
    Func<bool> haveWinner,
    Func<int> winningPlayer)
  {
    start();

    while (!haveWinner())
    {
      takeTurn();
    }

    WriteLine($"Player {winningPlayer()} wins.");
  }
}

public class GameDemo
{
  public void Main()
  {
    const int numberOfPlayers = 2;
    int currentPlayer = 0;
    int turn = 1;
    const int maxTurns = 10;

    GameTemplate.Run(Start, TakeTurn, HaveWinner, WinningPlayer);
    return;

    int WinningPlayer() {
      return currentPlayer;
    }

    void TakeTurn()
    {
      WriteLine($"Turn {turn++} taken by player {currentPlayer}.");
      currentPlayer = (currentPlayer + 1) % numberOfPlayers;
    }

    bool HaveWinner()
    {
      return turn == maxTurns;
    }

    void Start()
    {
      WriteLine($"Starting a game of chess with {numberOfPlayers} players.");
    }
  }
}
