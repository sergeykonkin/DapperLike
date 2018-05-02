using System.Data;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class NotSqlTransaction : IDbTransaction
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Commit()
        {
            throw new System.NotImplementedException();
        }

        public void Rollback()
        {
            throw new System.NotImplementedException();
        }

        public IDbConnection Connection { get; }
        public IsolationLevel IsolationLevel { get; }
    }
}
