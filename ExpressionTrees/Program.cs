using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace ExpressionTrees
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var list = new List<Student>().AsQueryable().Where(x => x.Age > 10);
            var students = new Builder<Student>();
            Console.WriteLine(
            students
                   .Where(x=>x.Age>7)
                   .AndWhere(x => x.Age < 10)
                   .AndWhere(x => x.FirstName.Contains("ade"))
                   .Select(x => new { x.FirstName, x.Age,NewField=10 })
            );
        }
    }

    public class Builder<T> where T : new()
    {
        #region private
        private Expression _expressionRoot;
        //private readonly List<Expression> _expressionContainer = new List<Expression>();
        private static string _tableName;

        private readonly List<ExpressionType> _logicalExpressions = new List<ExpressionType>
        {
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.Equal,
            ExpressionType.NotEqual,
        };

        private readonly List<string> _logicalSymbol = new List<string>
        {
            ">",
            ">=",
            "<",
            "<=",
            "=",
            "<>",
        };

        #endregion

        public Builder()
        {
            _tableName = $"{typeof(T).Name}s";
        }

        public Builder<T> From(string tableName = null)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                _tableName = tableName;
            }
            return this;
        }

        public Builder<T> Where(Expression<Func<T, bool>> expr)
        {
            if (expr.NodeType == ExpressionType.Lambda)
            {
                var lamdaExpr = (LambdaExpression)expr;
                if (_logicalExpressions.Contains(lamdaExpr.Body.NodeType))
                {
                    _expressionRoot = (BinaryExpression)lamdaExpr.Body;
                }
                else
                if (lamdaExpr.Body.NodeType == ExpressionType.Call)
                {
                    _expressionRoot = (MethodCallExpression)lamdaExpr.Body;
                }
            }
            else
            {
                throw new ArgumentException($" {typeof(T)} where argument must be lamda given {expr.Body}");
            }
            return this;
        }

        public Builder<T> AndWhere(Expression<Func<T, bool>> expr)
        {
            if (_expressionRoot == null)
            {
                throw new Exception("Call Where first");
            }

            _expressionRoot = Expression.AndAlso(_expressionRoot, expr.Body);

            return this;
        }

        public Builder<T> OrWhere(Expression<Func<T, bool>> expr)
        {
            if (_expressionRoot == null)
            {
                throw new Exception("Call Where first");
            }

            _expressionRoot = Expression.OrElse(_expressionRoot, expr.Body);

            return this;
        }

        public string DumpWhere()
        {
            return $"({Parse(_expressionRoot)}) ";
        }

        private string Parse(Expression expr)
        {
            if (_logicalExpressions.Contains(expr.NodeType))
            {
                var logicalSymbol = _logicalSymbol[_logicalExpressions.IndexOf(expr.NodeType)];
                var exprCast = (BinaryExpression)expr;
                return $"{Parse(exprCast.Left)}{logicalSymbol}{Parse(exprCast.Right)}";
            }
            else switch (expr.NodeType)
                {
                    case ExpressionType.Constant:
                        {
                            var exprCast = (ConstantExpression)expr;
                            if (exprCast.Type == typeof(string))
                            {
                                return $"\"{exprCast.Value}\"";
                            }
                            return $"{exprCast.Value}";
                        }

                    case ExpressionType.AndAlso:
                        {
                            var exprCast = (BinaryExpression)expr;
                            return $"({Parse(exprCast.Left)}) and ({Parse(exprCast.Right)})";
                        }
                    case ExpressionType.OrElse:
                        {
                            var exprCast = (BinaryExpression)expr;
                            return $" ({Parse(exprCast.Left)}) or({Parse(exprCast.Right)}) ";
                        }
                    case ExpressionType.MemberAccess:
                        {
                            var exprCast = (MemberExpression)expr;
                            return $" {exprCast.Member.Name} ";
                        }
                    case ExpressionType.Call:
                        {
                            var exprCast = (MethodCallExpression)expr;
                            string result = $"{Parse(exprCast.Object)} " + $"{exprCast.Method.Name} ";

                            if (exprCast.Arguments.Count == 1)
                            {
                                result += Parse(exprCast.Arguments[0]);
                            }
                            else
                                foreach (var arg in exprCast.Arguments)
                                {
                                    result += Parse(arg) + ",";
                                }
                            
                            return result;
                        }
                }
            return null;
        }

        internal string Select(Expression<Func<T, object>> expr)
        {
            string result = $"SELECT ";
            if (expr.Body.NodeType == ExpressionType.New)
            {
                var newExpr = (NewExpression)expr.Body;
                for (int i = 0; i < newExpr.Members.Count; i++)
                {
                    var memInfo = newExpr.Members[i];
                    var argExpr = newExpr.Arguments[i];
                    if (argExpr.NodeType == ExpressionType.Constant)
                    {
                        var conExpr = (ConstantExpression)argExpr;
                    result += $"{conExpr.Value} as {memInfo.Name},";
                    }
                    else
                    {
                        result += $"{memInfo.Name},";
                    }
                }
                result = result.Remove(result.Length - 1);
            }
            result += Environment.NewLine + $"From {_tableName} ";
            result += Environment.NewLine + $"Where {DumpWhere()}";
            return result;
        }
    }

    #region Sample class
    public class Student
    {
        public Student()
        {
        }

        public Student(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public Sex Sex { get; set; }

        public int Age { get; set; }
    }

    public enum Sex
    {
        Male,
        Female
    }
    #endregion
}
