## SqlBulkCopy wrapper with Dapper-like API
[![Build status](https://img.shields.io/vso/build/sergeykonkin/e640aa5e-254c-4469-8857-a4e79ac456ab/16.svg)]()
[![NuGet](https://img.shields.io/nuget/v/DapperLike.SqlBulkCopy.svg)](https://www.nuget.org/packages/DapperLike.SqlBulkCopy)

This library wraps [SqlBulkCopy](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlbulkcopy.aspx) class with [Dapper](https://github.com/StackExchange/Dapper)-like API. It provides extension methods for `IDbConnection` (just like Dapper do), but will only work for `SqlConnection` (for obvious reasons).

### Usage Example

Table
```sql
CREATE TABLE [User] (
    Id int PRIMARY KEY IDENTITY,
    FirstName nvarchar(250),
    LastName nvarchar(250),
    Age int,
    Gender bit
)
```

POCO
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public bool Gender { get; set; }
}  
```
Code
```csharp
IEnumerable<User> users = GetUsers();
connection.BulkInsert(users);
```
All properties will be automatically mapped to table columns as well as the type name will be mapped to table name.

Let's check:
```csharp
// Using Dapper to Query users:
IEnumerable<User> inserted = connection.Query<User>("SELECT * FROM [User]");

// Assert
Assert.IsTrue(users.SequenceEqual(inserted, new UserComparer())); // true
```

#### Simple data to single column
```csharp
List<byte[]> blobs = GetBinaryData();
connection.BulkInsert(blobs, tableName: "dbo.Data", columnName: "Blob");
```
Table and column names must be specified explicitly since they cannot be inferred from type/prop names.

#### Anonymous types
```csharp
IEnumerable<DomainObject> domainObjects = GetData();
var dto = domainObjects
    .Select(obj => new
    {
        Foo = obj.Prop1,
        Bar = obj.Prop2.Normalize(),
        Baz = ConvertToBaz(obj.Prop3)
    });
    
connection.BulkInsert(dto, tableName: "FooBar");
```
Column names can be inferred from anonymous type props, but table name from type name cannot, so again it must be specified explicitly.