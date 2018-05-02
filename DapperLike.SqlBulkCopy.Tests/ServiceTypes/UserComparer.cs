using System.Collections.Generic;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class UserComparer : IEqualityComparer<User>
    {
        public bool Equals(User x, User y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            return
                string.Equals(x.FirstName, y.FirstName) &&
                string.Equals(x.LastName, y.LastName) &&
                x.Age == y.Age &&
                x.Gender == y.Gender;
        }

        public int GetHashCode(User obj)
        {
            unchecked
            {
                var hashCode = (obj.FirstName != null ? obj.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.LastName != null ? obj.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Age;
                hashCode = (hashCode * 397) ^ obj.Gender.GetHashCode();
                return hashCode;
            }
        }
    }
}
