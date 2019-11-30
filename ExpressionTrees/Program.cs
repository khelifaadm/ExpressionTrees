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
                   .Where(x => x.Age == 10)
                   .OrWhere(x => x.FirstName == "khelifa")
                   .AndWhere(x => x.FirstName.Contains("ade"))
                   .Select(x => new { x.FirstName, x.Age })
            );

        }
    }

    public class Builder<T> where T : new()
    {

        #region private
        private readonly List<Expression> _expressionContainer = new List<Expression>();
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
        public enum CombineOperator{
            And,
            Or
        }
        private List<CombineOperator> _combineOperators = new List<CombineOperator>();

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
                    var binaryExpr = (BinaryExpression)lamdaExpr.Body;
                    _expressionContainer.Add(binaryExpr);
                }
                else
                if (lamdaExpr.Body.NodeType == ExpressionType.Call)
                {
                    var callExpr = (MethodCallExpression)lamdaExpr.Body;
                    _expressionContainer.Add(callExpr);
                }
            }
            return this;
        }

        public Builder<T> AndWhere(Expression<Func<T, bool>> expr)
        {
            if (_expressionContainer.Count == 0)
            {
                throw new Exception("Call Where first");
            }            
            _combineOperators.Add(CombineOperator.And);
            Where(expr);
            return this;
        }

        public Builder<T> OrWhere(Expression<Func<T, bool>> expr)
        {
            if (_expressionContainer.Count == 0)
            {
                throw new Exception("Call Where first");
            }
            _combineOperators.Add(CombineOperator.Or);
            Where(expr);
            return this;
        }

        public string DumpWhere()
        {
            var result = string.Empty;
            int indexCombine = 0;
            foreach (var expr in _expressionContainer)
            {
                result += $"({Parse(expr)}) ";
                if (_combineOperators.Count > indexCombine)
                {
                    switch (_combineOperators[indexCombine])
                    {
                        case CombineOperator.And:
                            result += " And ";
                            break;
                        case CombineOperator.Or:
                            result += " or ";
                            break;
                        default:
                            throw new ArgumentException(_combineOperators[indexCombine].ToString() + " not implimented combine ");
                    }
                    indexCombine++;
                }
            }
            return result;
        }

        private string Parse(Expression expr)
        {
            if (_logicalExpressions.Contains(expr.NodeType))
            {
                var logicalSymbol = _logicalSymbol[_logicalExpressions.IndexOf(expr.NodeType)];
                var exprCast = (BinaryExpression)expr;                
                return $"{Parse(exprCast.Left)} {logicalSymbol} {Parse(exprCast.Right)}";
            }
            else if (expr.NodeType == ExpressionType.Constant)
            {
                var exprCast = (ConstantExpression)expr;
                if (exprCast.Type == typeof(string))
                {
                return $"\"{exprCast.Value}\"";

                }
                return $"{exprCast.Value}";
            }
            else if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var exprCast = (MemberExpression)expr;
                return $" {exprCast.Member.Name} ";
            }
            else if (expr.NodeType == ExpressionType.And)
            {
                var exprCast = (MemberExpression)expr;
                return $" and  ";
            }
            if (expr.NodeType == ExpressionType.Call)
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
                result += ") ";
                return result;

            }
            return null;
        }

        internal string Select(Expression<Func<T, object>> expr)
        {
            string result = $"SELECT From {_tableName} ";
            result += Environment.NewLine;


            if (expr.Body.NodeType == ExpressionType.New)
            {
                var newExpr = (NewExpression)expr.Body;
                foreach (var item in newExpr.Arguments)
                {
                    var memExpr =(MemberExpression) item;
                    result += $"{memExpr.Member.Name},";
                }
                result = result.Remove( result.Length - 1);
            }
            result += Environment.NewLine;
            result += $"Where {DumpWhere()}";
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
