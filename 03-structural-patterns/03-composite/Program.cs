using System.Collections;
using System.Collections.ObjectModel;
using System.Text;

var drawing = new GraphicObject {Name = "My Drawing"};
drawing.Children.Add(new Square {Color = "Red"});
drawing.Children.Add(new Circle {Color = "Yellow"});

var group = new GraphicObject();
group.Children.Add(new Circle {Color = "Blue"});
group.Children.Add(new Square {Color = "Blue"});
drawing.Children.Add(group);

Console.WriteLine(drawing);

var neuron1 = new Neuron();
var neuron2 = new Neuron();
var layer1 = new NeuronLayer(3);
var layer2 = new NeuronLayer(4);

neuron1.ConnectTo(neuron2);
neuron1.ConnectTo(layer1);
layer2.ConnectTo(neuron1);
layer1.ConnectTo(layer2);

Console.WriteLine("Layer 1");
Console.WriteLine(layer1);

Console.WriteLine("Layer 2");
Console.WriteLine(layer2);

public class GraphicObject
{
  public virtual string Name { get; set; } = "Group";
  public string Color = default!;

  private readonly Lazy<List<GraphicObject>> _children = new();
  public List<GraphicObject> Children => _children.Value;

  private void Print(StringBuilder sb, int depth)
  {
    sb.Append(new string('*', depth))
      .Append(string.IsNullOrWhiteSpace(Color) ? string.Empty : $"{Color} ")
      .AppendLine($"{Name}");

    foreach (var child in Children)
      child.Print(sb, depth + 1);
  }

  public override string ToString()
  {
    var sb = new StringBuilder();
    Print(sb, 0);
    return sb.ToString();
  }
}

public class Circle : GraphicObject
{
  public override string Name => "Circle";
}

public class Square : GraphicObject
{
  public override string Name => "Square";
}

public class Neuron : Scalar<Neuron>
{
  public List<Neuron> In = new(), Out = new();

  public override string ToString() => $"From: {In.Count} To: {Out.Count}";
}

public class NeuronLayer : Collection<Neuron>
{
  public NeuronLayer(int count)
  {
    while (count --> 0)
      Add(new Neuron());
  }

  public override string ToString()
  {
    var sb = new StringBuilder();

    foreach (var neuron in this)
    {
      sb.AppendLine($"{neuron}");
    }

    return sb.ToString();
  }
}

public static class ExtensionMethods
{
  public static void ConnectTo(
    this IEnumerable<Neuron> self,
    IEnumerable<Neuron> other)
  {
    if (ReferenceEquals(self, other)) return;

    var otherList = other.ToList();

    foreach (var from in self)
    {
      foreach(var to in otherList)
      {
        from.Out.Add(to);
        to.In.Add(from);
      }
    }
  }
}

public abstract class Scalar<T> : IEnumerable<T>
  where T : Scalar<T>
{
  public IEnumerator<T> GetEnumerator()
  {
    yield return (T) this;
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}

public class Foo : Scalar<Foo> {}
