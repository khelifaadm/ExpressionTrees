using System;
using System.Collections.Generic;
using System.Linq.Expressions;
namespace ExpressionTrees
{
    public class QueryBuilder<T> where T : new()
    {
        #region private
        /// <summary>
        /// the root of expression  tree
        /// </summary>
        private Expression _rootExpression;
        private static string _tableName;

        /// <summary>
        /// allowed Logical ExpressionType
        /// </summary>
        private readonly List<ExpressionType> _logicalExpressions = new List<ExpressionType>
        {
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.Equal,
            ExpressionType.NotEqual,
        };

        /// <summary>
        /// allowed Logical operators
        /// </summary>
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

        public QueryBuilder()
        {
            _tableName = $"{typeof(T).Name}s";
        }

        #region Private methods
        /// <summary>
        /// Create sql Where from Expression
        /// </summary>
        private string DumpWhere()
        {
            return $"({Parse(_rootExpression)}) ";
        }

        /// <summary>
        /// The pincipal method that visit the tree and create sql where statement
        /// </summary>
        private string Parse(Expression expr)
        {
            if (_logicalExpressions.Contains(expr.NodeType))
            {
                var logicalSymbol = _logicalSymbol[_logicalExpressions.IndexOf(expr.NodeType)];
                var exprCast = (BinaryExpression)expr;
                return Parse(exprCast.Left) + logicalSymbol + Parse(exprCast.Right);
            }
            else
            {
                switch (expr.NodeType)
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
                            string result = $"{Parse(exprCast.Object)} {exprCast.Method.Name} ";

                            if (exprCast.Arguments.Count == 1)
                            {
                                result += Parse(exprCast.Arguments[0]);
                            }
                            else
                            {
                                foreach (var arg in exprCast.Arguments)
                                {
                                    result += Parse(arg) + ",";
                                }
                            }

                            return result;
                        }
                }
            }

            return null;
        }
        #endregion

        #region Public Methods
        public QueryBuilder<T> From(string tableName = null)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                _tableName = tableName;
            }
            return this;
        }

        public QueryBuilder<T> Where(Expression<Func<T, bool>> expr)
        {
            if (expr.NodeType == ExpressionType.Lambda)
            {
                var lamdaExpr = (LambdaExpression)expr;
                if (_logicalExpressions.Contains(lamdaExpr.Body.NodeType))
                {
                    _rootExpression = (BinaryExpression)lamdaExpr.Body;
                }
                else
                if (lamdaExpr.Body.NodeType == ExpressionType.Call)
                {
                    _rootExpression = (MethodCallExpression)lamdaExpr.Body;
                }
            }
            else
            {
                throw new ArgumentException($" {typeof(T)} where argument must be lamda given {expr.Body}");
            }
            return this;
        }

        public QueryBuilder<T> AndWhere(Expression<Func<T, bool>> expr)
        {
            if (_rootExpression == null)
            {
                throw new Exception("Call Where first");
            }

            _rootExpression = Expression.AndAlso(_rootExpression, expr.Body);

            return this;
        }

        public QueryBuilder<T> OrWhere(Expression<Func<T, bool>> expr)
        {
            if (_rootExpression == null)
            {
                throw new Exception("Call Where first");
            }

            _rootExpression = Expression.OrElse(_rootExpression, expr.Body);

            return this;
        }

        /// <summary>
        /// Create select part of sql query, Ignite tree parsing
        /// </summary>
        public string Select(Expression<Func<T, object>> expr)
        {
            string result = "SELECT ";
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
            result += Environment.NewLine + $"FROM {_tableName} ";
            result += Environment.NewLine + $"WHERE {DumpWhere()}";
            return result;
        }
        #endregion
    }
}
