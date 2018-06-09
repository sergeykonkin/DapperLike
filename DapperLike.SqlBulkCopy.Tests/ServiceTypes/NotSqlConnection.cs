using System.Data;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class NotSqlConnection : IDbConnection
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public IDbTransaction BeginTransaction()
        {
            throw new System.NotImplementedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new System.NotImplementedException();
        }

        public IDbCommand CreateCommand()
        {
            throw new System.NotImplementedException();
        }

        public void Open()
        {
            throw new System.NotImplementedException();
        }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; }
        public string Database { get; }
        public ConnectionState State { get; }
    }
}
