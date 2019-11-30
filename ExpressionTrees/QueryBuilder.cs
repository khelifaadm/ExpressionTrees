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
            switch (expr.NodeType)
            {
                case ExpressionType.Convert:
                    {
                        var exprCast = (UnaryExpression)expr;
                        return $"{Parse(exprCast.Operand)}";
                    }
                case ExpressionType.GreaterThan:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} > {Parse(exprCast.Right)}";
                    }
                case ExpressionType.GreaterThanOrEqual:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} >= {Parse(exprCast.Right)}";
                    }
                case ExpressionType.LessThan:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} < {Parse(exprCast.Right)}";
                    }
                case ExpressionType.LessThanOrEqual:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} <= {Parse(exprCast.Right)}";
                    }
                case ExpressionType.Equal:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} = {Parse(exprCast.Right)}";
                    }
                case ExpressionType.NotEqual:
                    {
                        var exprCast = (BinaryExpression)expr;
                        return $"{Parse(exprCast.Left)} <> {Parse(exprCast.Right)}";
                    }
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

                        if (exprCast.Arguments.Count == 1)
                        {
                            var val = exprCast.Arguments[0].ToString().Trim('"');
                            if (exprCast.Method.Name == "StartsWith")
                            {
                                return $"{Parse(exprCast.Object)} like \"{val}%\"";
                            }
                            else
                            if (exprCast.Method.Name == "EndsWith")
                            {
                                return $"{Parse(exprCast.Object)} like \"%{val}\"";
                            }
                            else
                                if (exprCast.Method.Name == "Contains")
                            {
                                return $"{Parse(exprCast.Object)} like \"%{val}%\"";
                            }

                            return Parse(exprCast.Object) + " " + exprCast.Method.Name + " " + Parse(exprCast.Arguments[0]);
                        }
                        else
                        {
                            string result = $"{Parse(exprCast.Object)} {exprCast.Method.Name} ";
                            foreach (var arg in exprCast.Arguments)
                            {
                                result += Parse(arg) + ",";
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

                if (lamdaExpr.Body.NodeType == ExpressionType.Call)
                {
                    _rootExpression = (MethodCallExpression)lamdaExpr.Body;
                }
                else
                {
                    _rootExpression = (BinaryExpression)lamdaExpr.Body;
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
