var vector = new VectorRenderer();
var circle = new Circle(vector, 5);

circle.Draw();
circle.Resize(3);
circle.Draw();

var raster = new RasterRenderer();
circle = new Circle(raster, 5);

circle.Draw();
circle.Resize(3);
circle.Draw();

public interface IRenderer
{
  void RenderCircle(float radius);
}

public abstract class Shape
{
  protected IRenderer renderer;

  protected Shape(IRenderer renderer)
  {
    this.renderer = renderer;
  }

  public abstract void Draw();
  public abstract void Resize(float factor);
}

public class Circle : Shape
{
  private float _radius;

  public Circle(IRenderer renderer, float radius) : base(renderer)
  {
    _radius = radius;
  }

  public override void Draw()
  {
    renderer.RenderCircle(_radius);
  }

  public override void Resize(float factor)
  {
    _radius *= factor;
  }
}

public class VectorRenderer : IRenderer
{
  public void RenderCircle(float radius)
  {
    Console.WriteLine($"Drawing a circle of radius {radius}");
  }
}

public class RasterRenderer : IRenderer
{
  public void RenderCircle(float radius)
  {
    Console.WriteLine($"Drawing a circle of radius {radius}");
  }
}
