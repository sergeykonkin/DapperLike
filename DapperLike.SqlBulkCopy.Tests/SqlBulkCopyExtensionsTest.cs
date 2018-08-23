using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Bogus;
using Bogus.DataSets;
using Dapper;
using NUnit.Framework;

namespace DapperLike.SqlBulkCopy.Tests
{
    [TestFixture]
    public class SqlBulkCopyExtensionsTest
    {
        private IDbConnection _connection;

        [SetUp]
        public void SetUp()
        {
            _connection = Env.Db.OpenConnection();
            _connection.Execute(@"
                TRUNCATE TABLE [User]
                TRUNCATE TABLE [Data]
                TRUNCATE TABLE [Anon]");
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should map custom type to SQL table and insert data")]
        [TestCase(true, TestName = "BulkInsertAsync should map custom type to SQL table and insert data")]
        public void BulkInsert__DataOfCustomTypePassed__MappedToTableAndInserted(bool async)
        {
            Name.Gender ConvertGender(bool boolean) => boolean ? Name.Gender.Male : Name.Gender.Female;

            // Arrange
            List<User> data = new Faker<User>()
                .RuleFor(u => u.Gender, (f, u) => f.Random.Bool())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(ConvertGender(u.Gender)))
                .RuleFor(u => u.LastName, (f, u) => f.Name.FirstName(ConvertGender(u.Gender)))
                .RuleFor(u => u.Age, (f, u) => f.Random.Number(18, 99))
                .Generate(1000);

            // Act
            if (async)
                _connection.BulkInsertAsync(data, commandTimeout: 10).GetAwaiter().GetResult();
            else
                _connection.BulkInsert(data, commandTimeout: 10);

            List<User> inserted = _connection.Query<User>("SELECT * FROM [User]").ToList();

            // Assert
            Assert.IsTrue(inserted.All(u => u.Id != 0));
            Assert.IsTrue(data.SequenceEqual(inserted, new UserComparer()));
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should insert single SQL compatible data by provided column name")]
        [TestCase(true, TestName = "BulkInsertAsync should insert single SQL compatible data by provided column name")]
        public void BulkInsert__DataOfSqlCompatibleTypePassed__InsertedByColumnName(bool async)
        {
            // Arrange
            var f = new Faker();
            var data = new List<byte[]>();
            for (int i = 0; i < 25; i++)
            {
                var length = f.Random.Number(1, 100);
                var array = new byte[length];
                for (int j = 0; j < length; j++)
                {
                    array[j] = f.Random.Byte();
                }

                data.Add(array);
            }

            // Act
            if (async)
                _connection.BulkInsertAsync(data, tableName: "dbo.Data", columnName: "Blob").GetAwaiter().GetResult();
            else
                _connection.BulkInsert(data, tableName: "dbo.Data", columnName: "Blob");

            var inserted = _connection.Query<byte[]>("SELECT * FROM [Data]").ToList();

            // Assert
            Assert.IsTrue(data.SequenceEqual(inserted, new ByteArrayComparer()));
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should map anonymous type to SQL table and insert data")]
        [TestCase(true, TestName = "BulkInsertAsync should map anonymous type to SQL table and insert data")]
        public void BulkInsert__DataOfAnonimousTypePassed__MappedToTableAndInserted(bool async)
        {
            // Arrange
            List<Anon> data = new Faker<Anon>()
                .RuleFor(u => u.Foo, (f, u) => f.Random.String2(f.Random.Number(10, 50)))
                .RuleFor(u => u.Bar, (f, u) => f.Random.Number(int.MinValue, int.MaxValue))
                .RuleFor(u => u.Baz, (f, u) => f.Date.Between(new DateTime(1753, 1, 2), DateTime.MaxValue))
                .Generate(1000);

            // Act
            var anonymous = data.Select(a => new {a.Foo, a.Bar, a.Baz});

            if (async)
                _connection.BulkInsertAsync(anonymous, tableName: "Anon").GetAwaiter().GetResult();
            else
                _connection.BulkInsert(anonymous, tableName: "Anon");

            List<Anon> inserted = _connection.Query<Anon>("SELECT * FROM [Anon]").ToList();

            // Assert
            Assert.IsTrue(data.SequenceEqual(inserted, new AnonComparer()));
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should respect [Column] and [NotMapped] attribute")]
        [TestCase(true, TestName = "BulkInsertAsync should respect [Column] and [NotMapped] attribute")]
        public void BulkInsert__TypeWithColumnAndNotMappedAttrsPassed__MappedToTableAndInserted(bool async)
        {
            Name.Gender ConvertGender(bool boolean) => boolean ? Name.Gender.Male : Name.Gender.Female;

            // Arrange
            List<UserWithAttrs> data = new Faker<UserWithAttrs>()
                .RuleFor(u => u.IsMale, (f, u) => f.Random.Bool())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(ConvertGender(u.IsMale)))
                .RuleFor(u => u.LastName, (f, u) => f.Name.FirstName(ConvertGender(u.IsMale)))
                .RuleFor(u => u.Age, (f, u) => f.Random.Number(18, 99))
                .Generate(1000);

            // Act
            if (async)
                _connection.BulkInsertAsync(data, "User", commandTimeout: 10).GetAwaiter().GetResult();
            else
                _connection.BulkInsert(data, "User", commandTimeout: 10);

            List<UserWithAttrs> inserted = _connection
                .Query<User>("SELECT * FROM [User]")
                .AsEnumerable()
                .Select(u => new UserWithAttrs
                {
                    Id = u.Id,
                    IsMale = u.Gender,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age
                })
                .ToList();

            // Assert
            Assert.IsTrue(inserted.All(u => u.Id != 0));
            Assert.IsTrue(data.SequenceEqual(inserted, new UserWithAttrsComparer()));
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should reopen closed connection and close it afterward")]
        [TestCase(true, TestName = "BulkInsertAsync should reopen closed connection and close it afterward")]
        public void BulkInsert__ClosedConnection__ShouldReopenAndReclose(bool async)
        {
            // Arrange
            var conn = Env.Db.OpenConnection();
            conn.Close();
            var data = new byte[10];
            new Random().NextBytes(data);

            // Act
            if (async)
                _connection.BulkInsertAsync(new [] {data}, "Data", "Blob", commandTimeout: 10).GetAwaiter().GetResult();
            else
                _connection.BulkInsert(new [] {data},  "Data", "Blob", commandTimeout: 10);

            // Assert
            Assert.AreEqual(ConnectionState.Closed, conn.State);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should support nullable columns")]
        [TestCase(true, TestName = "BulkInsertAsync should support nullable columns")]
        public void BulkInsert__TypeWithNullablePropertyUsed__ShoudNotThrow(bool async)
        {
            // Arrange
            var data = new List<WithNullable>
            {
                new WithNullable {Id = 5, Data = null}
            };

            // Act && Assert
            Assert.That(
                () =>
                {
                    if (async)
                        _connection.BulkInsertAsync(data).GetAwaiter().GetResult();
                    else
                        _connection.BulkInsert(data);
                },
                Throws.Nothing);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should throw if null connection argument is passed")]
        [TestCase(true, TestName = "BulkInsertAsync should throw if null connection argument is passed")]
        public void BulkInsert__NullConnectionPassed__ArgumentNullExceptionThrown(bool async)
        {
            Assert.That(
                () =>
                {
                    if (async)
                        SqlBulkCopyExtensions.BulkInsertAsync(null, Enumerable.Empty<int>()).GetAwaiter().GetResult();
                    else
                        SqlBulkCopyExtensions.BulkInsert(null, Enumerable.Empty<int>());
                },
                Throws.ArgumentNullException);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should throw if null data argument is passed")]
        [TestCase(true, TestName = "BulkInsertAsync should throw if null data argument is passed")]
        public void BulkInsert__NullDataPassed__ArgumentNullExceptionThrown(bool async)
        {
            Assert.That(
                () =>
                {
                    if (async)
                        _connection.BulkInsertAsync<int>(null).GetAwaiter().GetResult();
                    else
                        _connection.BulkInsert<int>(null);
                },
                Throws.ArgumentNullException);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should throw if connection argument not of SqlConnection type is passed")]
        [TestCase(true, TestName = "BulkInsertAsync should throw if connection argument not of SqlConnection type is passed")]
        public void BulkInsert__NotSqlConnectionPassed__ArgumentExceptionThrown(bool async)
        {
            Assert.That(
                () =>
                {
                    if (async)
                        new NotSqlConnection().BulkInsertAsync(Enumerable.Empty<int>()).GetAwaiter().GetResult();
                    else
                        new NotSqlConnection().BulkInsert(Enumerable.Empty<int>());
                },
                Throws.ArgumentException);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should throw if transaction argument not of SqlTransaction type is passed")]
        [TestCase(true, TestName = "BulkInsertAsync should throw if transaction argument not of SqlTransaction type is passed")]
        public void BulkInsert__NotSqlTransactionPassed__ArgumentExceptionThrown(bool async)
        {
            Assert.That(
                () =>
                {
                    if (async)
                        _connection.BulkInsertAsync(Enumerable.Empty<int>(), transaction: new NotSqlTransaction()).GetAwaiter().GetResult();
                    else
                        _connection.BulkInsert(Enumerable.Empty<int>(), transaction: new NotSqlTransaction());
                },
                Throws.ArgumentException);
        }

        [Test(TestOf = typeof(SqlBulkCopyExtensions))]
        [TestCase(false, TestName = "BulkInsert should throw if negative value for commandTimeout argument is passed")]
        [TestCase(true, TestName = "BulkInsertAsync should throw if negative value for commandTimeout argument is passed")]
        public void BulkInsert__NegativeTimeoutPassed__ArgumentOutOfRangeExceptionThrown(bool async)
        {
            Assert.That(
                () =>
                {
                    if (async)
                        _connection.BulkInsertAsync(Enumerable.Empty<int>(), commandTimeout: -10).GetAwaiter().GetResult();
                    else
                        _connection.BulkInsert(Enumerable.Empty<int>(), commandTimeout: -10);
                },
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
