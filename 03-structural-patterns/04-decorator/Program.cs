var circle = new Circle(2);
Console.WriteLine(circle.AsString());

var redSquare = new ColoredShape(circle, "red");
Console.WriteLine(redSquare.AsString());

var redHalfTransparentSquare = new TransparentShape(redSquare, 0.5f);
Console.WriteLine(redHalfTransparentSquare.AsString());

circle.Resize(10);
Console.WriteLine(redHalfTransparentSquare.AsString());

public abstract class Shape
{
  public virtual string AsString() => string.Empty;
}

public sealed class Circle : Shape
{
  private float _radius;

  public Circle() : this(0)
  {
  }

  public Circle(float radius)
  {
    _radius = radius;
  }

  public void Resize(float factor)
  {
    _radius *= factor;
  }

  public override string AsString() => $"A circle of radius {_radius}";
}

public class ColoredShape : Shape
{
  private readonly Shape _shape;
  private readonly string _color;

  public ColoredShape(Shape shape, string color)
  {
    _shape = shape;
    _color = color;
  }

  public override string AsString()
    => $"{_shape.AsString()} has the color {_color}";
}

public class TransparentShape : Shape
{
  private readonly Shape _shape;
  private readonly float _transparency;

  public TransparentShape(Shape shape, float transparency)
  {
    _shape = shape;
    _transparency = transparency;
  }

  public override string AsString() =>
    $"{_shape.AsString()} has {_transparency * 100.0f}% transparency";
}
