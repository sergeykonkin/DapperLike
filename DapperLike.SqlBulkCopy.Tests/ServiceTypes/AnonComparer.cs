using System.Collections.Generic;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class AnonComparer : IEqualityComparer<Anon>
    {
        public bool Equals(Anon x, Anon y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            return
                string.Equals(x.Foo, y.Foo) &&
                x.Bar == y.Bar &&
                x.Baz == y.Baz;
        }

        public int GetHashCode(Anon obj)
        {
            unchecked
            {
                var hashCode = (obj.Foo != null ? obj.Foo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Bar;
                hashCode = (hashCode * 397) ^ obj.Baz.GetHashCode();
                return hashCode;
            }
        }
    }
}
