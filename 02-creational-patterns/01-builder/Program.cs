using System.Text;

var builder = HtmlElement
  .Create("ul")
  .AddChild("li", "hello")
  .AddChild("li", "world");
Console.WriteLine(builder);

var pb = new PersonBuilder()
  .Lives
    .At("123 London Road")
    .In("London")
    .WithPostCode("SW12BC")

  .Works
    .At("Frabrikam")
    .AsA("Engineer")
    .Earning(123000);
Console.WriteLine(pb);

var person = new PersonBuilderOcp()
  .Called("Dimitri")
  .WorksAs("Programmer")
  .Build();
Console.WriteLine(person);

class HtmlElement
{
  public string? Name, Text;
  public List<HtmlElement> Elements = new();
  private const int identSize = 2;

  public HtmlElement() {}
  public HtmlElement(string name, string text)
  {
    Name = name;
    Text = text;
  }

  public static HtmlBuilder Create(string name) => new(name);

  public override string ToString()
  {
    var sb = new StringBuilder();
    sb.Append($"<{Name}>");

    if (!string.IsNullOrWhiteSpace(Text))
    {
      sb.Append($"{Text}");
    }

    foreach (var e in Elements)
    {
      sb.Append(e);
    }

    sb.Append($"</{Name}>");

    return sb.ToString();
  }
}

class HtmlBuilder
{
  protected readonly string rootName;
  protected HtmlElement root = new();

  public HtmlBuilder(string rootName)
  {
    this.rootName = rootName;
    root.Name = rootName;
  }

  public HtmlBuilder AddChild(string childName, string childText)
  {
    var e = new HtmlElement(childName, childText);
    root.Elements.Add(e);

    return this;
  }

  public static implicit operator HtmlElement(HtmlBuilder builder) =>
    builder.root;

  public HtmlElement Build() => root;

  public override string ToString() => root.ToString();
}

public class Person
{
  public string StreetAddress = default!, Postcode = default!, City = default!;

  public string CompanyName = default!, Position = default!;
  public int AnnualIncome;

  public string Name = default!;

  public override string ToString()
  {
    var sb = new StringBuilder();
    if (!string.IsNullOrWhiteSpace(StreetAddress))
    {
      sb.Append($"StreetAddress: {StreetAddress},");
    }
    if (!string.IsNullOrWhiteSpace(Name))
    {
      sb.Append($"Name: {Name},");
    }
    if (!string.IsNullOrWhiteSpace(Postcode))
    {
      sb.Append($" Postcode: {Postcode},");
    }
    if (!string.IsNullOrWhiteSpace(City))
    {
      sb.Append($" City: {City},");
    }
    if (!string.IsNullOrWhiteSpace(CompanyName))
    {
      sb.Append($" CompanyName: {CompanyName},");
    }
    if (!string.IsNullOrWhiteSpace(Position))
    {
      sb.Append($" Position: {Position},");
    }
    if (AnnualIncome > 0)
    {
      sb.Append($" AnnualIncome: {AnnualIncome}");
    }

    return sb.ToString();
  }
}

public class PersonBuilder
{
  protected Person person;

  public PersonBuilder() => person = new Person();
  protected PersonBuilder(Person person) => this.person = person;

  public PersonAddressBuilder Lives => new(person);
  public PersonJobBuilder Works => new(person);

  public static implicit operator Person(PersonBuilder pb) =>
    pb.person;
  public override string ToString() => person.ToString();
}

public class PersonAddressBuilder : PersonBuilder
{
  public PersonAddressBuilder(Person person) : base(person)
  {
    this.person = person;
  }

  public PersonAddressBuilder At(string streetAddress)
  {
    person.StreetAddress = streetAddress;
    return this;
  }

  public PersonAddressBuilder WithPostCode(string postcode)
  {
    person.Postcode = postcode;
    return this;
  }

  public PersonAddressBuilder In(string city)
  {
    person.City = city;
    return this;
  }
}

public class PersonJobBuilder : PersonBuilder
{
  public PersonJobBuilder(Person person) : base(person)
  {
    this.person = person;
  }

  public PersonJobBuilder At(string companyName)
  {
    person.CompanyName = companyName;
    return this;
  }

  public PersonJobBuilder AsA(string position)
  {
    person.Position = position;
    return this;
  }

  public PersonJobBuilder Earning(int annualIncome)
  {
    person.AnnualIncome = annualIncome;
    return this;
  }
}

public abstract class FunctionalBuilder<TSubject, TSelf>
  where TSelf : FunctionalBuilder<TSubject, TSelf>
  where TSubject : new()
{
  private readonly List<Func<TSubject, TSubject>> actions = new();

  public TSelf Do(Action<TSubject> action) =>
    AddAction(action);

  private TSelf AddAction(Action<TSubject> action)
  {
    actions.Add(p => {
      action(p);
      return p;
    });
    return (TSelf) this;
  }

  public TSubject Build()
    => actions.Aggregate(new TSubject(), (p, f) => f(p));
}

public sealed class PersonBuilderOcp
  : FunctionalBuilder<Person, PersonBuilderOcp>
{
  public PersonBuilderOcp Called(string name)
    => Do(p => p.Name = name);
}

public static class PersonBuilderExtensions
{
  public static PersonBuilderOcp WorksAs(
    this PersonBuilderOcp builder, string position)
    => builder.Do(p => p.Position = position);
}
