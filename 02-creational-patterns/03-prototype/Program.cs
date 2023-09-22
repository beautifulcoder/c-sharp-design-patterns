using System.Xml.Serialization;

var john = EmployeeFactory.NewMainOfficeEmployee("John Doe", 100);
var jane = EmployeeFactory.NewAuxOfficeEmployee("Jane Doe", 123);

Console.WriteLine(john);
Console.WriteLine(jane);

public record Address(string StreetName, int HouseNumber)
{
  public Address() : this(string.Empty, 0)
  {
  }

  public Address(Address address)
  {
    StreetName = address.StreetName;
    HouseNumber = address.HouseNumber;
  }
}

public record Person(string Name, Address Address)
{
  public Person() : this(string.Empty, new Address())
  {
  }

  public Person(Person person)
  {
    Name = person.Name;
    Address = new Address(person.Address);
  }
}

public static class DeepCopyExtensions
{
  public static T DeepCopy<T>(this T self)
  {
    using var stream = new MemoryStream();
    var s = new XmlSerializer(typeof(T));

    s.Serialize(stream, self);
    stream.Seek(0, SeekOrigin.Begin);
    stream.Position = 0;

    var copy = s.Deserialize(stream)!;
    return (T) copy;
  }
}

public class EmployeeFactory
{
  private static Person main = new("Main", new Address("123 East Dr", 0));
  private static Person aux = new("Aux", new Address("123B East Dr", 0));

  public static Person NewMainOfficeEmployee(string name, int suite) =>
    NewEmployee(main, name, suite);

  public static Person NewAuxOfficeEmployee(string name, int suite) =>
    NewEmployee(aux, name, suite);

  private static Person NewEmployee(Person proto, string name, int suite)
  {
    var person = proto.DeepCopy();
    var address = person.Address with {HouseNumber = suite};
    var copy = new Person(name, address);
    return copy;
  }
}
