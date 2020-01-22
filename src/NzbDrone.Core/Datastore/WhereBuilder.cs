using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dapper;

namespace NzbDrone.Core.Datastore
{
    public class WhereBuilder : ExpressionVisitor
    {
        protected StringBuilder _sb;

        private const DbType EnumerableMultiParameter = (DbType)(-1);
        private readonly string _paramNamePrefix;
        private readonly bool _requireConcreteValue = false;
        private int _paramCount = 0;
        private bool _gotConcreteValue = false;

        public WhereBuilder(Expression filter, bool requireConcreteValue)
        {
            _paramNamePrefix = Guid.NewGuid().ToString().Replace("-", "_");
            _requireConcreteValue = requireConcreteValue;
            _sb = new StringBuilder();

            Parameters = new DynamicParameters();

            if (filter != null)
            {
                Visit(filter);
            }
        }

        public DynamicParameters Parameters { get; private set; }

        private string AddParameter(object value, DbType? dbType = null)
        {
            _gotConcreteValue = true;
            _paramCount++;
            var name = _paramNamePrefix + "_P" + _paramCount;
            Parameters.Add(name, value, dbType);
            return '@' + name;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _sb.Append("(");

            Visit(expression.Left);

            _sb.AppendFormat(" {0} ", Decode(expression));

            Visit(expression.Right);

            _sb.Append(")");

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var method = expression.Method.Name;

            switch (expression.Method.Name)
            {
                case "Contains":
                    ParseContainsExpression(expression);
                    break;

                case "StartsWith":
                    ParseStartsWith(expression);
                    break;

                case "EndsWith":
                    ParseEndsWith(expression);
                    break;

                default:
                    var msg = string.Format("'{0}' expressions are not yet implemented in the where clause expression tree parser.", method);
                    throw new NotImplementedException(msg);
            }

            return expression;
        }

        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            var tableName = expression != null ? TableMapping.Mapper.TableNameMapping(expression.Expression.Type) : null;

            if (tableName != null)
            {
                _sb.Append($"\"{tableName}\".\"{expression.Member.Name}\"");
            }
            else
            {
                var value = GetRightValue(expression);

                if (value != null)
                {
                    // string is IEnumerable<Char> but we don't want to pick up that case
                    var type = value.GetType();
                    var typeInfo = type.GetTypeInfo();
                    var isEnumerable =
                        type != typeof(string) && (
                            typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                            (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>)));

                    var paramName = isEnumerable ? AddParameter(value, EnumerableMultiParameter) : AddParameter(value);
                    _sb.Append(paramName);
                }
                else
                {
                    _gotConcreteValue = true;
                    _sb.Append("NULL");
                }
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value != null)
            {
                var paramName = AddParameter(expression.Value);
                _sb.Append(paramName);
            }
            else
            {
                _gotConcreteValue = true;
                _sb.Append("NULL");
            }

            return expression;
        }

        private bool TryGetConstantValue(Expression expression, out object result)
        {
            if (expression is ConstantExpression constExp)
            {
                result = constExp.Value;
                return true;
            }

            result = null;
            return false;
        }

        private bool TryGetPropertyValue(MemberExpression expression, out object result)
        {
            if (expression.Expression is MemberExpression nested)
            {
                // Value is passed in as a property on a parent entity
                var container = (nested.Expression as ConstantExpression).Value;
                var entity = GetFieldValue(container, nested.Member);
                result = GetFieldValue(entity, expression.Member);
                return true;
            }

            result = null;
            return false;
        }

        private bool TryGetVariableValue(MemberExpression expression, out object result)
        {
            // Value is passed in as a variable
            if (expression.Expression is ConstantExpression nested)
            {
                result = GetFieldValue(nested.Value, expression.Member);
                return true;
            }

            result = null;
            return false;
        }

        private object GetRightValue(Expression expression)
        {
            if (TryGetConstantValue(expression, out var constValue))
            {
                return constValue;
            }

            var memberExp = expression as MemberExpression;

            if (TryGetPropertyValue(memberExp, out var propValue))
            {
                return propValue;
            }

            if (TryGetVariableValue(memberExp, out var variableValue))
            {
                return variableValue;
            }

            return null;
        }

        private object GetFieldValue(object entity, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                return (member as FieldInfo).GetValue(entity);
            }

            if (member.MemberType == MemberTypes.Property)
            {
                return (member as PropertyInfo).GetValue(entity);
            }

            throw new ArgumentException(string.Format("WhereBuilder could not get the value for {0}.{1}.", entity.GetType().Name, member.Name));
        }

        private bool IsNullVariable(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant &&
                TryGetConstantValue(expression, out var constResult) &&
                constResult == null)
            {
                return true;
            }

            if (expression.NodeType == ExpressionType.MemberAccess &&
                expression is MemberExpression member &&
                TryGetVariableValue(member, out var variableResult) &&
                variableResult == null)
            {
                return true;
            }

            return false;
        }

        private string Decode(BinaryExpression expression)
        {
            if (IsNullVariable(expression.Right))
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

        private void ParseContainsExpression(MethodCallExpression expression)
        {
            var list = expression.Object;

            if (list != null && list.Type == typeof(string))
            {
                ParseStringContains(expression);
                return;
            }

            ParseEnumerableContains(expression);
        }

        private void ParseEnumerableContains(MethodCallExpression body)
        {
            // Fish out the list and the item to compare
            // It's in a different form for arrays and Lists
            var list = body.Object;
            Expression item;

            if (list != null)
            {
                // Generic collection
                item = body.Arguments[0];
            }
            else
            {
                // Static method
                // Must be Enumerable.Contains(source, item)
                if (body.Method.DeclaringType != typeof(Enumerable) || body.Arguments.Count != 2)
                {
                    throw new NotSupportedException("Unexpected form of Enumerable.Contains");
                }

                list = body.Arguments[0];
                item = body.Arguments[1];
            }

            _sb.Append("(");

            Visit(item);

            _sb.Append(" IN ");

            Visit(list);

            _sb.Append(")");
        }

        private void ParseStringContains(MethodCallExpression body)
        {
            _sb.Append("(");

            Visit(body.Object);

            _sb.Append(" LIKE '%' || ");

            Visit(body.Arguments[0]);

            _sb.Append(" || '%')");
        }

        private void ParseStartsWith(MethodCallExpression body)
        {
            _sb.Append("(");

            Visit(body.Object);

            _sb.Append(" LIKE ");

            Visit(body.Arguments[0]);

            _sb.Append(" || '%')");
        }

        private void ParseEndsWith(MethodCallExpression body)
        {
            _sb.Append("(");

            Visit(body.Object);

            _sb.Append(" LIKE '%' || ");

            Visit(body.Arguments[0]);

            _sb.Append(")");
        }

        public override string ToString()
        {
            var sql = _sb.ToString();

            if (_requireConcreteValue && !_gotConcreteValue)
            {
                var e = new InvalidOperationException("WhereBuilder requires a concrete condition");
                e.Data.Add("sql", sql);
                throw e;
            }

            return sql;
        }
    }
}
