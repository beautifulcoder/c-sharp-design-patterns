using static System.Console;

var goblin = new Creature("Goblin", 1, 1);
WriteLine(goblin);

var root = new CreatureModifier(goblin);

root.Add(new DoubleAttackModifier(goblin));
root.Add(new DoubleAttackModifier(goblin));

root.Add(new IncreaseDefenseModifier(goblin));

// eventually...
root.Handle();
WriteLine(goblin);

WriteLine();

var game = new Game();
var goblinQ = new CreatureQ(game, "Strong Goblin", 2, 2);
WriteLine(goblinQ);

using (new DoubleAttackModifierQ(game, goblinQ))
{
  WriteLine(goblinQ);
  using (new IncreaseDefenseModifierQ(game, goblinQ))
  {
    WriteLine(goblinQ);
  }
}

WriteLine(goblinQ);

public class Creature
{
  public string Name;
  public int Attack, Defense;

  public Creature(string name, int attack, int defense)
  {
    Name = name;
    Attack = attack;
    Defense = defense;
  }

  public override string ToString()
  {
    return $"{nameof(Name)}: {Name}, {nameof(Attack)}: {Attack}, {nameof(Defense)}: {Defense}";
  }
}

public class CreatureModifier
{
  protected Creature Creature;
  protected CreatureModifier? Next;

  public CreatureModifier(Creature creature)
  {
    this.Creature = creature;
  }

  public void Add(CreatureModifier cm)
  {
    if (Next != null) Next.Add(cm);
    else Next = cm;
  }

  public virtual void Handle() => Next?.Handle();
}

public class DoubleAttackModifier : CreatureModifier
{
  public DoubleAttackModifier(Creature creature)
    : base(creature) {}

  public override void Handle()
  {
    WriteLine($"Doubling {Creature.Name}’s attack");
    Creature.Attack *= 2;
    base.Handle();
  }
}

public class IncreaseDefenseModifier : CreatureModifier
{
  public IncreaseDefenseModifier(Creature creature)
    : base(creature) {}

  public override void Handle()
  {
    if (Creature.Attack <= 2)
    {
      WriteLine($"Increasing {Creature.Name}'s defense");
      Creature.Defense++;
    }

    base.Handle();
  }
}

public class Game // mediator pattern
{
  public event EventHandler<Query>? Queries; // effectively a chain

  public void PerformQuery(object sender, Query q)
  {
    Queries?.Invoke(sender, q);
  }
}

public class Query
{
  public string CreatureName;

  public enum Argument
  {
    Attack, Defense
  }

  public Argument WhatToQuery;

  public int Value; // bidirectional

  public Query(string creatureName, Argument whatToQuery, int value)
  {
    CreatureName = creatureName;
    WhatToQuery = whatToQuery;
    Value = value;
  }
}

public class CreatureQ
{
  private readonly Game _game;
  public string Name;
  private readonly int _attack;
  private readonly int _defense;

  public CreatureQ(Game game, string name, int attack, int defense)
  {
    _game = game;
    Name = name;
    _attack = attack;
    _defense = defense;
  }

  public int Attack
  {
    get
    {
      var q = new Query(Name, Query.Argument.Attack, _attack);
      _game.PerformQuery(this, q);
      return q.Value;
    }
  }

  public int Defense
  {
    get
    {
      var q = new Query(Name, Query.Argument.Defense, _defense);
      _game.PerformQuery(this, q);
      return q.Value;
    }
  }

  public override string ToString() // no game
  {
    return $"{nameof(Name)}: {Name}, " +
           $"{nameof(_attack)}: {Attack}, " +
           $"{nameof(_defense)}: {Defense}";
    // ^^^^^^ using a property  ^^^^^^^^^
  }
}

public abstract class CreatureModifierQ : IDisposable
{
  protected readonly Game Game;
  protected readonly CreatureQ Creature;

  protected CreatureModifierQ(Game game, CreatureQ creature)
  {
    Game = game;
    Creature = creature;
    game.Queries += Handle;
  }

  protected abstract void Handle(object? sender, Query q);

  public void Dispose()
  {
    Game.Queries -= Handle;
  }
}

public class DoubleAttackModifierQ : CreatureModifierQ
{
  public DoubleAttackModifierQ(Game game, CreatureQ creature) 
    : base(game, creature) {}

  protected override void Handle(object? sender, Query q)
  {
    if (q.CreatureName == Creature.Name &&
        q.WhatToQuery == Query.Argument.Attack)
      q.Value *= 2;
  }
}

public class IncreaseDefenseModifierQ : CreatureModifierQ
{
  public IncreaseDefenseModifierQ(Game game, CreatureQ creature) : base(game, creature)
  {
  }

  protected override void Handle(object? sender, Query q)
  {
    if (q.CreatureName == Creature.Name &&
      q.WhatToQuery == Query.Argument.Defense)
    {
      q.Value += 2;
    }
  }
}
