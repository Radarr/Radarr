using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper;
using NzbDrone.Common.Reflection;

namespace NzbDrone.Core.Datastore
{
    public static class MappingExtensions
    {
        public static PropertyInfo GetMemberName<T>(this Expression<Func<T, object>> member)
        {
            var memberExpression = member.Body as MemberExpression;
            if (memberExpression == null)
            {
                memberExpression = (member.Body as UnaryExpression).Operand as MemberExpression;
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }

    public class TableMapper
    {
        public TableMapper()
        {
            IgnoreList = new Dictionary<Type, List<PropertyInfo>>();
            TableMap = new Dictionary<Type, string>();
        }

        public Dictionary<Type, List<PropertyInfo>> IgnoreList { get; set; }
        public Dictionary<Type, string> TableMap { get; set; }

        public ColumnMapper<TEntity> Entity<TEntity>(string tableName)
        {
            TableMap.Add(typeof(TEntity), tableName);

            if (IgnoreList.TryGetValue(typeof(TEntity), out var list))
            {
                return new ColumnMapper<TEntity>(list);
            }

            list = new List<PropertyInfo>();
            IgnoreList[typeof(TEntity)] = list;
            return new ColumnMapper<TEntity>(list);
        }

        public List<PropertyInfo> ExcludeProperties(Type x)
        {
            return IgnoreList.ContainsKey(x) ? IgnoreList[x] : new List<PropertyInfo>();
        }

        public string TableNameMapping(Type x)
        {
            return TableMap.ContainsKey(x) ? TableMap[x] : null;
        }
    }

    public class ColumnMapper<T>
    {
        private readonly List<PropertyInfo> _ignoreList;

        public ColumnMapper(List<PropertyInfo> ignoreList)
        {
            _ignoreList = ignoreList;
        }

        public ColumnMapper<T> AutoMapPropertiesWhere(Func<PropertyInfo, bool> predicate)
        {
            Type entityType = typeof(T);
            var properties = entityType.GetProperties();
            _ignoreList.AddRange(properties.Where(x => !predicate(x)));

            return this;
        }

        public ColumnMapper<T> RegisterModel()
        {
            return AutoMapPropertiesWhere(IsMappableProperty);
        }

        public ColumnMapper<T> Ignore(Expression<Func<T, object>> property)
        {
            _ignoreList.Add(property.GetMemberName());
            return this;
        }

        public static bool IsMappableProperty(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;

            if (propertyInfo == null)
            {
                return false;
            }

            if (!propertyInfo.IsReadable() || !propertyInfo.IsWritable())
            {
                return false;
            }

            // This is a bit of a hack but is the only way to see if a type has a handler set in Dapper
#pragma warning disable 618
            SqlMapper.LookupDbType(propertyInfo.PropertyType, "", false, out var handler);
#pragma warning restore 618
            if (propertyInfo.PropertyType.IsSimpleType() || handler != null)
            {
                return true;
            }

            return false;
        }
    }
}
