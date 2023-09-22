var point = Point.NewPolarPoint(5, Math.PI / 4);
Console.WriteLine(point);

var point2 = Point.Factory.NewCartesianPoint(2, 3);
Console.WriteLine(point2);

_ = await Foo.CreateAsync();

var basic = GetFactory(false);
var basicRectangle = basic.Create(Shape.Rectangle);
basicRectangle.Draw();

var roundedSquare = GetFactory(true).Create(Shape.Square);
roundedSquare.Draw();

static ShapeFactory GetFactory(bool rounded)
{
  if (rounded)
    return new RoundedShapeFactory();
  else
    return new BasicShapeFactory();
}

public class Point
{
  private double x, y;

  protected Point(double x, double y)
  {
    this.x = x;
    this.y = y;
  }

  public override string ToString()
  {
    return $"{nameof(x)}: {x:F}, {nameof(y)}: {y:F}";
  }

  public static Point NewCartesianPoint(double x, double y)
  {
    return new Point(x, y);
  }

  public static Point NewPolarPoint(double rho, double theta)
  {
    return new Point(rho * Math.Cos(theta), rho * Math.Sin(theta));
  }

  public class Factory
  {
    public static Point NewCartesianPoint(float x, float y)
    {
      return new Point(x, y);
    }
  }
}

public class Foo
{
  protected Foo() {}

  public static Task<Foo> CreateAsync()
  {
    var result = new Foo();
    return result.InitAsync();
  }

  private async Task<Foo> InitAsync()
  {
    await Task.Delay(1000);
    return this;
  }
}

public interface IShape
{
  void Draw();
}

public class Square : IShape
{
  public void Draw() => Console.WriteLine("Basic square");
}

public class Rectangle : IShape
{
  public void Draw() => Console.WriteLine("Basic rectangle");
}

public class RoundedSquare : IShape
{
  public void Draw() => Console.WriteLine("Rounded square");
}

public class RoundedRectangle : IShape
{
  public void Draw() => Console.WriteLine("Rounded rectangle");
}

public enum Shape
{
  Square,
  Rectangle
}

public abstract class ShapeFactory
{
  public abstract IShape Create(Shape shape);
}

public class BasicShapeFactory : ShapeFactory
{
  public override IShape Create(Shape shape)
  {
    switch (shape)
    {
      case Shape.Square:
        return new Square();
      case Shape.Rectangle:
        return new Rectangle();
      default:
        throw new ArgumentOutOfRangeException(
          nameof(shape), shape, null);
    }
  }
}

public class RoundedShapeFactory : ShapeFactory
{
  public override IShape Create(Shape shape)
  {
    switch (shape)
    {
      case Shape.Square:
        return new RoundedSquare();
      case Shape.Rectangle:
        return new RoundedRectangle();
      default:
        throw new ArgumentOutOfRangeException(
          nameof(shape), shape, null);
    }
  }
}
