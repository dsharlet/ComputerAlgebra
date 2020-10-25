namespace ComputerAlgebra
{
    ///// <summary>
    ///// Represents a floating point number with arbitrary length mantissa and exponent.
    ///// </summary>
    //public struct BigFloat : IComparable<BigFloat>, IEquatable<BigFloat>, IFormattable
    //{
    //    private BigInteger m, e;

    //    public BigFloat(BigInteger n) : this(n, 0) { }
    //    public BigFloat(BigInteger Mantissa, BigInteger Exponent) 
    //    { 
    //        m = Mantissa; 
    //        e = Exponent; 

    //        while ((m & 255) == 0)
    //        {
    //            m >>= 8;
    //            e += 8;
    //        }
    //        while ((m & 1) == 0)
    //        {
    //            m >>= 1;
    //            e += 1;
    //        }
    //    }

    //    public static BigFloat operator -(BigFloat x) { return new BigFloat(-x.m, x.e); }
    //    public static BigFloat operator +(BigFloat l, BigFloat r)
    //    {
    //        // Create a new BigFloat with the smaller exponent of l, r.
    //        if (l.e > r.e)
    //            return new BigFloat((l.m << (int)(l.e - r.e)) + r.m, r.e);
    //        else
    //            return new BigFloat((r.m << (int)(r.e - l.e)) + l.m, l.e);
    //    }
    //    public static BigFloat operator -(BigFloat l, BigFloat r) { return l + -r; }
    //    public static BigFloat operator *(BigFloat l, BigFloat r) { return new BigFloat(l.m * r.m, l.e + r.e); }
    //    public static BigFloat operator /(BigFloat l, BigFloat r) { return new BigFloat(l.m / r.m, l.e - r.e); }
    //}
}
