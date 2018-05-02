using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DapperLike
{
    public static partial class SqlBulkCopyExtensions
    {
        private static readonly IDictionary<Type, PropertyInfo[]> PropsCache = new Dictionary<Type, PropertyInfo[]>();

        private static DataTable CreateTable<T>(IEnumerable<T> data, string tableName, string columnName)
        {
            Type entityType = typeof(T);

            var table = new DataTable
            {
                Locale = CultureInfo.InvariantCulture,
                TableName = tableName ?? entityType.Name
            };

            if (columnName != null)
                BuildOneColumnTable(table, data, columnName);
            else
                BuildCustomTable(table, data);

            return table;
        }

        private static void BuildOneColumnTable<T>(DataTable table, IEnumerable<T> data, string columnName)
        {
            table.Columns.Add(new DataColumn(columnName, typeof(T)));
            table.Rows.AddRange(data);
        }

        private static void BuildCustomTable<T>(DataTable table, IEnumerable<T> data)
        {
            var columns = GetColumnNamesFromProps(typeof(T));
            table.Columns.AddRange(columns);
            table.Rows.AddRange(data, ConvertToRow<T>);
        }

        private static DataColumn[] GetColumnNamesFromProps(Type type)
        {
            return GetPropsWithCaching(type).Select(prop => new DataColumn(prop.Name, prop.PropertyType)).ToArray();
        }

        private static object[] ConvertToRow<T>(T entity)
        {
            var props = GetPropsWithCaching(typeof(T));
            var result = new object[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                result[i] = props[i].GetValue(entity);
            }

            return result;
        }

        private static PropertyInfo[] GetPropsWithCaching(Type type)
        {
            if (PropsCache.ContainsKey(type))
                return PropsCache[type];

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropsCache[type] = props;
            return props;
        }
    }
}
