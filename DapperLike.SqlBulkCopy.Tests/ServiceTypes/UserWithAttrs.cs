namespace DapperLike.SqlBulkCopy.Tests
{
    public class UserWithAttrs
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        [Column("Gender")]
        public bool IsMale { get; set; }

        [Ignore]
        public string FullName => $"{FirstName} {LastName}";
    }
}
