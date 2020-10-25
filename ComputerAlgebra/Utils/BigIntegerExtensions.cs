using System.Numerics;

namespace ComputerAlgebra
{
    public static class BigIntegerExtensions
    {
        /// <summary>
        /// Returns the trailing number of 0 digits in x.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int InsignificantDigits(this BigInteger x, int b)
        {
            BigInteger d = x;
            for (int i = 0; ; ++i)
            {
                BigInteger r;
                d = BigInteger.DivRem(d, b, out r);
                if (!r.IsZero)
                    return i;
            }
        }

        public static int InsignificantDigits(this BigInteger x) { return InsignificantDigits(x, 10); }
    }
}
