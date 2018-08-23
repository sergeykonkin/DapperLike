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
        private static DataTable CreateTable<T>(IEnumerable<T> data, string columnName)
        {
            var table = new DataTable
            {
                Locale = CultureInfo.InvariantCulture
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
            var columnMap = ReflectionHelper.GetColumnMap(typeof(T));
            var columns = columnMap.Select(ToDataColumn).ToArray();
            table.Columns.AddRange(columns);
            table.Rows.AddRange(data, ConvertToRow);
        }

        private static object[] ConvertToRow<T>(T entity)
        {
            var props = ReflectionHelper.GetColumnMap(typeof(T)).Select(t => t.Item1).ToArray();
            var result = new object[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                result[i] = props[i].GetValue(entity);
            }

            return result;
        }

        private static DataColumn ToDataColumn(Tuple<MemberInfo, string> tuple)
        {
            var memberType = tuple.Item1.GetMemberType();
            var nullableType = Nullable.GetUnderlyingType(memberType);

            return new DataColumn(tuple.Item2, nullableType ?? memberType);
        }
    }
}
