using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace DapperLike
{
    internal static class ReflectionHelper
    {
        private static readonly IDictionary<Type, Tuple<MemberInfo, string>[]> _columnMapCache = new ConcurrentDictionary<Type, Tuple<MemberInfo, string>[]>();
        private static readonly IDictionary<Type, string> _tableNameCache = new ConcurrentDictionary<Type, string>();

        public static Tuple<MemberInfo, string>[] GetColumnMap(Type type)
        {
            Tuple<MemberInfo, string>[] GetColumnMapImpl(Type t)
            {
                return t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Cast<MemberInfo>()
                    .Union(type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    .Where(member => member.GetCustomAttribute<NotMappedAttribute>() == null)
                    .Select(
                        member => new Tuple<MemberInfo, string>(
                            member,
                            member.GetCustomAttribute<ColumnAttribute>()?.Name ?? member.Name))
                    .ToArray();
            }

            if (_columnMapCache.ContainsKey(type))
            {
                return _columnMapCache[type];
            }

            var columnMap = GetColumnMapImpl(type);

            _columnMapCache.Add(type, columnMap);
            return columnMap;
        }

        public static string GetTableName(Type type)
        {
            string GetTableNameImpl(Type t)
            {
                var tableAttr = t.GetCustomAttribute<TableAttribute>();
                if (tableAttr == null)
                {
                    return t.Name;
                }

                return (tableAttr.Schema == null ? "" : tableAttr.Schema + ".")
                       + tableAttr.Name;
            }

            if (_tableNameCache.ContainsKey(type))
            {
                return _tableNameCache[type];
            }

            var name = GetTableNameImpl(type);
            _tableNameCache[type] = name;
            return name;
        }
        public static object GetValue(this MemberInfo memberInfo, object obj)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).GetValue(obj);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
