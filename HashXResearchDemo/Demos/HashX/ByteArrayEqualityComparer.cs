namespace HashXResearchDemo.Demos.HashX;

public class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        if (x.Length != y.Length)
        {
            return false;
        }

        return !x.Where((t, i) => t != y[i]).Any();
    }

    public int GetHashCode(byte[] obj)
    {
        unchecked
        {
            var hash = 17;
            foreach (var b in obj)
            {
                hash = hash * 31 + b.GetHashCode();
            }

            return hash;
        }
    }
}
