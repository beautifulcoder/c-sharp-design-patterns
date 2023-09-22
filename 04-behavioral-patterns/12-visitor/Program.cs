using System.Text;
using static System.Console;

using DictType = System.Collections.Generic.Dictionary<System.Type, System.Action<Expression2, System.Text.StringBuilder>>;

var e = new AdditionExpression(
  new DoubleExpression(1),
  new AdditionExpression(
    new DoubleExpression(2),
    new DoubleExpression(3)));
var sb = new StringBuilder();
e.Print(sb);
WriteLine(sb);

WriteLine();

var e2 = new AdditionExpression2(
  left: new DoubleExpression2(1),
  right: new AdditionExpression2(
    left: new DoubleExpression2(2),
    right: new DoubleExpression2(3)));
var sb2 = new StringBuilder();
e2.Print(sb2); // extension method goodness!
WriteLine(sb2);

WriteLine();

var e3 = new AdditionExpression3(
  new DoubleExpression3(1),
  new AdditionExpression3(
    new DoubleExpression3(2),
    new DoubleExpression3(3)));
var ep3 = e3.Print();
WriteLine(ep3);

var calc3 = new ExpressionCalculator();
calc3.Visit(e3);
WriteLine($"{ep3} = {calc3.Result}");

WriteLine();

var e4 = new MultiplicationExpression3(
  new DoubleExpression3(1),
  new AdditionExpression3(
    new DoubleExpression3(2),
    new DoubleExpression3(3)));
var ep4 = e4.Print();
WriteLine(ep4); // 1*(2+3)

var calc4 = new ExpressionCalculator();
calc4.Visit(e4);
WriteLine($"{ep4} = {calc4.Result}");

public abstract class Expression
{
  // adding a new operation
  public abstract void Print(StringBuilder sb);
}

public class DoubleExpression : Expression
{
  private readonly double _value;

  public DoubleExpression(double value)
  {
    _value = value;
  }

  public override void Print(StringBuilder sb)
  {
    sb.Append(_value);
  }
}

public class AdditionExpression : Expression
{
  private readonly Expression _left;
  private readonly Expression _right;

  public AdditionExpression(Expression left, Expression right)
  {
    _left = left;
    _right = right;
  }

  public override void Print(StringBuilder sb)
  {
    sb.Append("(");
    _left.Print(sb);
    sb.Append(" + ");
    _right.Print(sb);
    sb.Append(")");
  }
}

public abstract class Expression2
{
}

public class DoubleExpression2 : Expression2
{
  public double Value;

  public DoubleExpression2(double value)
  {
    Value = value;
  }
}

public class AdditionExpression2 : Expression2
{
  public Expression2 Left;
  public Expression2 Right;

  public AdditionExpression2(Expression2 left, Expression2 right)
  {
    Left = left;
    Right = right;
  }
}

public static class ExpressionPrinter
{
  private static readonly DictType Actions = new()
  {
    [typeof(DoubleExpression2)] = (e, sb) =>
    {
      var de = (DoubleExpression2) e;
      sb.Append(de.Value);
    },
    [typeof(AdditionExpression2)] = (e, sb) =>
    {
      var ae = (AdditionExpression2) e;
      sb.Append("(");
      Print(ae.Left, sb);
      sb.Append(" + ");
      Print(ae.Right, sb);
      sb.Append(")");
    }
  };

  public static void Print(this Expression2 e, StringBuilder sb)
  {
    Actions[e.GetType()](e, sb);
  }
}

public abstract class Expression3
{
  public Expression3? Parent;
  public virtual void Accept(IExpressionVisitor visitor) {}
}

public class DoubleExpression3 : Expression3
{
  public readonly double Value;

  public DoubleExpression3(double value) => Value = value;

  public override void Accept(IExpressionVisitor visitor)
    => visitor.Visit(this);
}

public class AdditionExpression3 : Expression3
{
  public readonly Expression3 Left, Right;

  public AdditionExpression3(Expression3 left, Expression3 right)
  {
    Left = left;
    Right = right;
    Left.Parent = Right.Parent = this;
  }

  public override void Accept(IExpressionVisitor visitor)
  {
    visitor.Visit(this);
  }
}

public class MultiplicationExpression3 : AdditionExpression3
{
  public MultiplicationExpression3(Expression3 left, Expression3 right)
    : base(left, right) {}

  public override void Accept(IExpressionVisitor visitor)
  {
    visitor.Visit(this);
  }
}

public interface IExpressionVisitor
{
  void Visit(DoubleExpression3 de);
  void Visit(AdditionExpression3 ae);
}

public class ExpressionPrinter3 : IExpressionVisitor
{
  private readonly StringBuilder _sb = new();

  public void Visit(DoubleExpression3 de)
  {
    _sb.Append(de.Value);
  }

  public void Visit(AdditionExpression3 ae)
  {
    var needBraces = ae.Parent is MultiplicationExpression3;
    if (needBraces) _sb.Append("(");
    ae.Left.Accept(this);
    _sb.Append(" + ");
    ae.Right.Accept(this);
    if (needBraces) _sb.Append(")");
  }

  public void Visit(MultiplicationExpression3 ae)
  {
    ae.Left.Accept(this);
    _sb.Append(" * ");
    ae.Right.Accept(this);
  }

  public override string ToString() => _sb.ToString();
}

public class ExpressionCalculator : IExpressionVisitor
{
  public double Result;

  // what you really want is double Visit(...)

  public void Visit(DoubleExpression3 de)
  {
    Result = de.Value;
  }

  public void Visit(AdditionExpression3 ae)
  {
    ae.Left.Accept(this);
    var a = Result;
    ae.Right.Accept(this);
    var b = Result;
    Result = a + b;
  }

  public void Visit(MultiplicationExpression3 ae)
  {
    ae.Left.Accept(this);
    var a = Result;
    ae.Right.Accept(this);
    var b = Result;
    Result = a * b;
  }
}

public static class ExtensionMethods
{
  public static string Print(this AdditionExpression3 e)
  {
    var ep = new ExpressionPrinter3();
    ep.Visit(e);
    return ep.ToString();
  }

  public static string Print(this MultiplicationExpression3 e)
  {
    var ep = new ExpressionPrinter3();
    ep.Visit(e);
    return ep.ToString();
  }
}
