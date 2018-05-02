using System.Collections.Generic;
using System.Linq;

namespace DapperLike.SqlBulkCopy.Tests
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null))
                return false;

            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            if (obj.Length == 0)
                return 0;

            if (obj.Length == 1)
                return obj[1];

            unchecked
            {
                return obj.Skip(1).Aggregate(obj[0], (acc, cur) => (byte) (acc ^ cur));
            }
        }
    }
}
