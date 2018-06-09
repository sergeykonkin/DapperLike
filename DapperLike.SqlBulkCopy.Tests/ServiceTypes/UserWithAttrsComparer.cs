using System.Collections.Generic;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class UserWithAttrsComparer : IEqualityComparer<UserWithAttrs>
    {
        public bool Equals(UserWithAttrs x, UserWithAttrs y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            return
                string.Equals(x.FirstName, y.FirstName) &&
                string.Equals(x.LastName, y.LastName) &&
                x.Age == y.Age &&
                x.IsMale == y.IsMale;
        }

        public int GetHashCode(UserWithAttrs obj)
        {
            unchecked
            {
                var hashCode = (obj.FirstName != null ? obj.FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.LastName != null ? obj.LastName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Age;
                hashCode = (hashCode * 397) ^ obj.IsMale.GetHashCode();
                return hashCode;
            }
        }
    }
}
