using System.Collections;
using System.Linq;

public class Sprite
{
    public IList<byte> Bytes { get; set; } = new List<byte>();

    private int _hashcode;
    public override int GetHashCode()
    {
        if (_hashcode == 0)
        {
            int hash = 17;
            foreach (var b in Bytes)
                hash = hash * 23 + b.GetHashCode();
            _hashcode = hash;
        }
        return _hashcode;
    }

}
