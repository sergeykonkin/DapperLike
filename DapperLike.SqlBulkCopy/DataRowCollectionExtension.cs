using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DapperLike
{
    internal static class DataRowCollectionExtensions
    {
        public static void AddRange<T>(this DataRowCollection rows, IEnumerable<T> data,
            Func<T, object[]> convertEntity = null)
        {
            if (convertEntity == null)
            {
                foreach (T entity in data)
                {
                    rows.Add(entity);
                }
            }
            else
            {
                foreach (object[] row in data.Select(convertEntity))
                {
                    rows.Add(row);
                }
            }
        }
    }
}
