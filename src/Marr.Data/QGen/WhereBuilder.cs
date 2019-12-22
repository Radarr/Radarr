using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Common;
using Marr.Data.Parameters;
using System.Reflection;
using Marr.Data.QGen.Dialects;

namespace Marr.Data.QGen
{
    /// <summary>
    /// This class utilizes the ExpressionVisitor base class, and it is responsible for creating the "WHERE" clause.
    /// It builds a protected StringBuilder class whose output is created when the ToString method is called.
    /// It also has some methods that coincide with Linq methods, to provide Linq compatibility.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereBuilder<T> : ExpressionVisitor
    {
        private string _constantWhereClause;
        private MapRepository _repos;
        private DbCommand _command;
        private string _paramPrefix;
        private bool _isLeftSide = true;
        protected bool _useAltName;
        protected Dialect _dialect;
        protected StringBuilder _sb;
        protected TableCollection _tables;
        protected bool _tablePrefix;

        public WhereBuilder(string whereClause, bool useAltName)
        {
            _constantWhereClause = whereClause;
            _useAltName = useAltName;
        }

        public WhereBuilder(DbCommand command, Dialect dialect, Expression filter, TableCollection tables, bool useAltName, bool tablePrefix)
        {
            _repos = MapRepository.Instance;
            _command = command;
            _dialect = dialect;
            _paramPrefix = command.ParameterPrefix();
            _sb = new StringBuilder();
            _useAltName = useAltName;
            _tables = tables;
            _tablePrefix = tablePrefix;

            if (filter != null)
            {
                _sb.AppendFormat("{0} ", PrefixText);
                base.Visit(filter);
            }
        }

        protected virtual string PrefixText
        {
            get
            {
                return "WHERE";
            }
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _sb.Append("(");

            _isLeftSide = true;
            Visit(expression.Left);

            _sb.AppendFormat(" {0} ", Decode(expression));

            _isLeftSide = false;
            Visit(expression.Right);

            _sb.Append(")");

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            string method = (expression as MethodCallExpression).Method.Name;
            switch (method)
            {
                case "Contains":
                    Write_Contains(expression);
                    break;

                case "StartsWith":
                    Write_StartsWith(expression);
                    break;

                case "EndsWith":
                    Write_EndsWith(expression);
                    break;

                case "In":
                    Write_In(expression);
                    break;

                default:
                    string msg = string.Format("'{0}' expressions are not yet implemented in the where clause expression tree parser.", method);
                    throw new NotImplementedException(msg);
            }

            return expression;
        }

        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (_isLeftSide)
            {
                string fqColumn = GetFullyQualifiedColumnName(expression.Member, expression.Expression.Type);
                _sb.Append(fqColumn);
            }
            else
            {
                // Add parameter to Command.Parameters
                string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());
                _sb.Append(paramName);

                object value = GetRightValue(expression);
                new ParameterChainMethods(_command, paramName, value);
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value != null)
            {
                // Add parameter to Command.Parameters
                string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());

                _sb.Append(paramName);

                var parameter = new ParameterChainMethods(_command, paramName, expression.Value).Parameter;
            }
            else
            {
                _sb.Append("NULL");
            }

            return expression;
        }

        private object GetRightValue(Expression expression)
        {
            object rightValue = null;

            var simpleConstExp = expression as ConstantExpression;
            if (simpleConstExp == null) // Value is not directly passed in as a constant
            {
                MemberExpression memberExp = expression as MemberExpression;
                ConstantExpression constExp = null;

                // Value may be nested in multiple levels of objects/properties, so traverse the MemberExpressions
                // until a ConstantExpression property value is found, and then unwind the stack to get the value.
                var memberNames = new Stack<string>();

                while (memberExp != null)
                {
                    memberNames.Push(memberExp.Member.Name);

                    // Function calls are not supported - user needs to simplify their Where expression.
                    var methodExp = memberExp.Expression as MethodCallExpression;
                    if (methodExp != null)
                    {
                        var errMsg = string.Format("Function calls are not supported by the Where clause expression parser.  Please evaluate your function call, '{0}', manually and then use the resulting paremeter value in your Where expression.",  methodExp.Method.Name);
                        throw new NotSupportedException(errMsg);
                    }

                    constExp = memberExp.Expression as ConstantExpression;
                    memberExp = memberExp.Expression as MemberExpression;
                }

                object entity = constExp.Value;
                while (memberNames.Count > 0)
                {
                    string entityName = memberNames.Pop();
                    entity = _repos.ReflectionStrategy.GetFieldValue(entity, entityName);
                }
                rightValue = entity;
            }
            else // Value is passed in directly as a constant
            {
                rightValue = simpleConstExp.Value;
            }

            return rightValue;
        }

        protected string GetFullyQualifiedColumnName(MemberInfo member, Type declaringType)
        {
            if (_tablePrefix)
            {
                Table table = _tables.FindTable(declaringType);

                if (table == null)
                {
                    string msg = string.Format("The property '{0} -> {1}' you are trying to reference in the 'WHERE' statement belongs to an entity that has not been joined in your query.  To reference this property, you must join the '{0}' entity using the Join method.",
                        declaringType,
                        member.Name);

                    throw new DataMappingException(msg);
                }

                string columnName = DataHelper.GetColumnName(declaringType, member.Name, _useAltName);
                return _dialect.CreateToken(string.Format("{0}.{1}", table.Alias, columnName));
            }
            else
            {
                string columnName = DataHelper.GetColumnName(declaringType, member.Name, _useAltName);
                return _dialect.CreateToken(columnName);
            }
        }

        private string Decode(BinaryExpression expression)
        {
            bool isRightSideNullConstant = expression.Right.NodeType ==
                ExpressionType.Constant &&
                ((ConstantExpression)expression.Right).Value == null;

            if (isRightSideNullConstant)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal: return "IS";
                    case ExpressionType.NotEqual: return "IS NOT";
                }
            }

            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.And: return "AND";
                case ExpressionType.Equal: return "=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Or: return "OR";
                default: throw new NotSupportedException(string.Format("{0} statement is not supported", expression.NodeType.ToString()));
            }
        }

        private void Write_Contains(MethodCallExpression body)
        {
            // Add parameter to Command.Parameters
            object value = GetRightValue(body.Arguments[0]);
            string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());
            var parameter = new ParameterChainMethods(_command, paramName, value).Parameter;

            MemberExpression memberExp = (body.Object as MemberExpression);
            string fqColumn = GetFullyQualifiedColumnName(memberExp.Member, memberExp.Expression.Type);
            _sb.AppendFormat(_dialect.ContainsFormat, fqColumn, paramName);
        }

        private void Write_In(MethodCallExpression body)
        {
            object value = GetRightValue(body.Arguments[1]);
            //string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());
            //var parameter = new ParameterChainMethods(_command, paramName, value).Parameter;

            MemberExpression memberExp = (body.Arguments[0] as MemberExpression);
            string fqColumn = GetFullyQualifiedColumnName(memberExp.Member, memberExp.Expression.Type);
            _sb.AppendFormat(_dialect.InFormat, fqColumn, string.Join(",", value as List<int>));
        }

        private void Write_StartsWith(MethodCallExpression body)
        {
            // Add parameter to Command.Parameters
            object value = GetRightValue(body.Arguments[0]);
            string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());
            var parameter = new ParameterChainMethods(_command, paramName, value).Parameter;

            MemberExpression memberExp = (body.Object as MemberExpression);
            string fqColumn = GetFullyQualifiedColumnName(memberExp.Member, memberExp.Expression.Type);
            _sb.AppendFormat(_dialect.StartsWithFormat, fqColumn, paramName);
        }

        private void Write_EndsWith(MethodCallExpression body)
        {
            // Add parameter to Command.Parameters
            object value = GetRightValue(body.Arguments[0]);
            string paramName = string.Concat(_paramPrefix, "P", _command.Parameters.Count.ToString());
            var parameter = new ParameterChainMethods(_command, paramName, value).Parameter;

            MemberExpression memberExp = (body.Object as MemberExpression);
            string fqColumn = GetFullyQualifiedColumnName(memberExp.Member, memberExp.Expression.Type);
            _sb.AppendFormat(_dialect.EndsWithFormat, fqColumn, paramName);
        }

        /// <summary>
        /// Appends the current where clause with another where clause.
        /// </summary>
        /// <param name="where">The second where clause that is being appended.</param>
        /// <param name="appendType">AND / OR</param>
        internal void Append(WhereBuilder<T> where, WhereAppendType appendType)
        {
            _constantWhereClause = string.Format("{0} {1} {2}",
                ToString(),
                appendType.ToString(),
                where.ToString().Replace("WHERE ", string.Empty));
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_constantWhereClause))
            {
                return _sb.ToString();
            }
            return _constantWhereClause;
        }
    }

    internal enum WhereAppendType
    {
        AND,
        OR
    }
}
