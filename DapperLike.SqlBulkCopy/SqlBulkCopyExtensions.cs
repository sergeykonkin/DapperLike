using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DapperLike
{
    /// <summary>
    /// Provides Dapper-like wrappers for SqlBulkCopy.
    /// </summary>
    public static partial class SqlBulkCopyExtensions
    {
        /// <summary>
        /// Performs bulk insertion of data using SqlBulkCopy.
        /// </summary>
        /// <typeparam name="T">Type of inserting data.</typeparam>
        /// <param name="connection">The already open <see cref="T:System.Data.SqlClient.SqlConnection" /> instance that will be used to perform the bulk copy.</param>
        /// <param name="data">Data to insert.</param>
        /// <param name="tableName">Optional. Destination table name. Will be inferred from data type name, if not specified.</param>
        /// <param name="columnName">Optional. Specify this value if you want to copy data into single column of the table.</param>
        /// <param name="transaction">Optional. An existing <see cref="T:System.Data.SqlClient.SqlTransaction" /> instance under which the bulk copy will occur.</param>
        /// <param name="commandTimeout">Optional. Number of seconds for the operation to complete before it times out. The default is 30 seconds. A value of 0 indicates no limit; the bulk copy will wait indefinitely.</param>
        /// <param name="options">Optional. A combination of values from the <see cref="T:System.Data.SqlClient.SqlBulkCopyOptions" /> enumeration that determines which data source rows are copied to the destination table.</param>
        public static void BulkInsert<T>(this IDbConnection connection,
            IEnumerable<T> data,
            string tableName = null,
            string columnName = null,
            IDbTransaction transaction = null,
            SqlBulkCopyOptions options = SqlBulkCopyOptions.Default,
            int? commandTimeout = null)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (!(connection is SqlConnection sqlConnection))
                throw new ArgumentException("SqlBulkCopy supports System.Data.SqlClient.SqlConnection connection type only.");

            if (transaction != null && !(transaction is SqlTransaction))
                throw new ArgumentException("SqlBulkCopy supports System.Data.SqlClient.SqlTransaction transaction type only.");

            if (commandTimeout.HasValue && commandTimeout < 0)
                throw new ArgumentOutOfRangeException(nameof(commandTimeout));

            BulkInsertImpl(
                sqlConnection,
                data,
                tableName,
                columnName,
                (SqlTransaction)transaction,
                options,
                commandTimeout);
        }

        private static void BulkInsertImpl<T>(
            SqlConnection sqlConnection,
            IEnumerable<T> data,
            string tableName,
            string columnName,
            SqlTransaction sqlTransaction,
            SqlBulkCopyOptions options,
            int? commandTimeout)
        {
            DataTable table = CreateTable(data, tableName, columnName);
            if (table.Rows.Count == 0) return;

            SqlBulkCopy sqlBulkCopy = GetSqlBulkCopy(sqlConnection, sqlTransaction, options, commandTimeout, table);
            using (sqlBulkCopy)
            {
                sqlBulkCopy.WriteToServer(table);
            }
        }

        private static SqlBulkCopy GetSqlBulkCopy(
            SqlConnection sqlConnection,
            SqlTransaction sqlTransaction,
            SqlBulkCopyOptions options,
            int? commandTimeout,
            DataTable table)
        {
            var sqlBulkCopy = new SqlBulkCopy(sqlConnection, options, sqlTransaction);

            if (commandTimeout.HasValue)
                sqlBulkCopy.BulkCopyTimeout = commandTimeout.Value;

            sqlBulkCopy.DestinationTableName = MsSqlEncode(table.TableName);
            foreach (DataColumn column in table.Columns)
            {
                sqlBulkCopy.ColumnMappings.Add(column.ColumnName, MsSqlEncode(column.ColumnName));
            }

            return sqlBulkCopy;
        }

        private static string MsSqlEncode(string name)
        {
            var parts = name
                .Split('.')
                .Select(part => "[" + part.Trim('[', ']') + "]");

            return string.Join(".", parts);
        }
    }
}
