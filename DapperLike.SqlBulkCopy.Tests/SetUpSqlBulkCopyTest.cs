using System.Data;
using Dapper;
using NUnit.Framework;
using RimDev.Automation.Sql;

namespace DapperLike.SqlBulkCopy.Tests
{
    public static class Env
    {
        public static LocalDb Db { get; set; }
    }

    [SetUpFixture]
    public class SetUpSqlBulkCopyTest
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Env.Db = CreateNewDb();
        }

        [OneTimeTearDown]
        public void RunAfterAllTests()
        {
            Env.Db.Dispose();
        }

        private static LocalDb CreateNewDb()
        {
            var db = new LocalDb(version: "mssqllocaldb");
            IDbConnection conn = db.OpenConnection();

            conn.Execute(
                @"
CREATE TABLE [User] (
    Id int PRIMARY KEY IDENTITY,
    FirstName nvarchar(250),
    LastName nvarchar(250),
    Age int,
    Gender bit
)

CREATE TABLE [Data] (
    Blob varbinary(100)
)

CREATE TABLE [Anon] (
    Foo nvarchar(250),
    Bar int,
    Baz datetime2
)
            ");

            return db;
        }
    }
}
