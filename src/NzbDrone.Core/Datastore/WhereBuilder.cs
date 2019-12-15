using System;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace NzbDrone.Core.Datastore
{
    public class WhereBuilder : ExpressionVisitor
    {
        private const DbType EnumerableMultiParameter = (DbType)(-1);

        private readonly string _paramNamePrefix;
        private int _paramCount = 0;
        protected StringBuilder _sb;

        public DynamicParameters Parameters { get; private set; }

        public WhereBuilder(Expression filter)
        {
            _paramNamePrefix = Guid.NewGuid().ToString().Replace("-", "_");
            Parameters = new DynamicParameters();
            _sb = new StringBuilder();

            if (filter != null)
            {
                base.Visit(filter);
            }            
        }

        private string AddParameter(object value, DbType? dbType = null)
        {
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
            string method = expression.Method.Name;

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
                    string msg = string.Format("'{0}' expressions are not yet implemented in the where clause expression tree parser.", method);
                    throw new NotImplementedException(msg);
            }

            return expression;
        }


        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            string tableName = TableMapping.Mapper.TableNameMapping(expression.Expression.Type);

            if (tableName != null)
            {
                _sb.Append($"\"{tableName}\".\"{expression.Member.Name}\"");
            }
            else
            {
                object value = GetRightValue(expression);

                // string is IEnumerable<Char> but we don't want to pick up that case
                var type = value.GetType();
                var typeInfo = type.GetTypeInfo();
                bool isEnumerable =
                    type != typeof(string) && (
                        typeInfo.ImplementedInterfaces.Any(ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                        (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        );

                string paramName;
                if (isEnumerable)
                {
                    paramName = AddParameter(value, EnumerableMultiParameter);
                }
                else
                {
                    paramName = AddParameter(value);
                }

                _sb.Append(paramName);
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value != null)
            {
                string paramName = AddParameter(expression.Value);
                _sb.Append(paramName);
            }
            else
            {
                _sb.Append("NULL");
            }

            return expression;
        }

        private object GetRightValue(Expression rightExpression)
        {
            object rightValue = null;

            var right = rightExpression as ConstantExpression;
            if (right == null) // Value is not directly passed in as a constant
            {
                var rightMemberExp = (rightExpression as MemberExpression);
                var parentMemberExpression = rightMemberExp.Expression as MemberExpression;
                if (parentMemberExpression != null) // Value is passed in as a property on a parent entity
                {
                    var memberInfo = (rightMemberExp.Expression as MemberExpression).Member;
                    var container = ((rightMemberExp.Expression as MemberExpression).Expression as ConstantExpression).Value;
                    var entity = GetFieldValue(container, memberInfo);
                    rightValue = GetFieldValue(entity, rightMemberExp.Member);
                }
                else // Value is passed in as a variable
                {
                    var parent = (rightMemberExp.Expression as ConstantExpression).Value;
                    rightValue = GetFieldValue(parent, rightMemberExp.Member);
                }
            }
            else // Value is passed in directly as a constant
            {
                rightValue = right.Value;
            }

            return rightValue;
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
            return _sb.ToString();
        }
    } 
}
