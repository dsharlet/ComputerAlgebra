using System;
using System.Numerics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represent a rational number with System.Numerics.BigInteger as the numerator and denominator.
    /// </summary>
    public struct BigRational : IComparable<BigRational>, IEquatable<BigRational>, IFormattable
    {
        private BigInteger n, d;

        private void Canonicalize()
        {
            if (n.IsZero)
            {
                d = 1;
            }
            else
            {
                BigInteger gcd = BigInteger.GreatestCommonDivisor(n, d);
                if (!gcd.IsOne)
                {
                    n /= gcd;
                    d /= gcd;
                }

                if (d.Sign == -1)
                {
                    n = -n;
                    d = -d;
                }
            }
        }

        public BigRational(BigInteger Integer) { n = Integer; d = 1; }
        public BigRational(BigInteger Numerator, BigInteger Denominator) { n = Numerator; d = Denominator; Canonicalize(); }

        public static BigRational Unchecked(int n, int d) { BigRational x = new BigRational(); x.n = n; x.d = d; return x; }
        public static BigRational Unchecked(BigInteger n, BigInteger d) { BigRational x = new BigRational(); x.n = n; x.d = d; return x; }
        public bool Equals(int n, int d) { return this.n == n && this.d == d; }

        private static int InsignificantDigits(BigInteger x)
        {
            for (int i = 0; i < 1000; ++i)
                if (x % BigInteger.Pow(10, i + 1) != 0)
                    return i;
            return 0;
        }

        private static string ToLaTeX(BigInteger x)
        {
            int insig = InsignificantDigits(x);
            if (insig >= 3)
                return (x / BigInteger.Pow(10, insig)).ToString() + @"\times 10^{" + insig + "}";
            else
                return x.ToString();
        }

        public string ToLaTeX()
        {
            string ns = ToLaTeX(n);
            if (d == 1) return ns;

            string nd = ToLaTeX(d);

            if (ns.Length <= 2 && nd.Length <= 2)
                return "^{" + ns + "}_{" + nd + "}";
            else
                return @"\frac{" + ns + "}{" + nd + "}";
        }

        public bool EqualsZero() { return n.IsZero; }
        public bool EqualsOne() { return n == d; }
        public bool IsInteger() { return d == 1; }

        // IEquatable interface.
        public bool Equals(BigRational x) { return d == x.d && n == x.n; }

        // IComparable interface.
        public int CompareTo(BigRational x)
        {
            // Try comparing signs first, to avoid big integer multiply.
            int sign = n.Sign.CompareTo(x.n.Sign);
            if (sign != 0) return sign;

            // Try comparing the numerators and denominators directly to avoid big integer multiply.
            int nc = n.CompareTo(x.n);
            int dc = d.CompareTo(x.d);

            if (dc == 0) return nc;
            if (nc == 0) return -dc;

            return (n * x.d).CompareTo(x.n * d);
        }

        // IFormattable interface.
        public string ToString(string format, IFormatProvider formatProvider) { return ((double)this).ToString(format, formatProvider); }
        public string ToString(string format) { return ToString(format, null); }

        // object interface.
        public override bool Equals(object obj)
        {
            if (obj is BigRational)
                return Equals((BigRational)obj);
            else
                return base.Equals(obj);
        }
        public override int GetHashCode() { return n.GetHashCode() ^ d.GetHashCode(); }
        public override string ToString()
        {
            if (d != 1)
                return n.ToString() + "/" + d.ToString();
            else
                return n.ToString();
        }

        // Arithmetic operators.
        public static BigRational operator -(BigRational a) { return Unchecked(-a.n, a.d); }
        public static BigRational operator *(BigRational a, BigRational b) { return new BigRational(a.n * b.n, a.d * b.d); }
        public static BigRational operator /(BigRational a, BigRational b) { return new BigRational(a.n * b.d, a.d * b.n); }
        public static BigRational operator +(BigRational a, BigRational b) { return new BigRational(a.n * b.d + b.n * a.d, a.d * b.d); }
        public static BigRational operator -(BigRational a, BigRational b) { return new BigRational(a.n * b.d - b.n * a.d, a.d * b.d); }
        public static BigRational operator ^(BigRational a, int b)
        {
            if (b < 0)
                return Unchecked(BigInteger.Pow(a.d, -b), BigInteger.Pow(a.n, -b));
            else
                return Unchecked(BigInteger.Pow(a.n, b), BigInteger.Pow(a.d, b));
        }
        public static BigRational operator ^(BigRational a, BigRational b)
        {
            if (!b.d.IsOne)
                return Math.Pow((double)a, (double)b);
            else
                return a ^ (int)b.n;
        }
        public static BigRational operator %(BigRational a, BigRational b) { return a - Floor(a / b) * b; }

        // Relational operators.
        public static bool operator ==(BigRational a, BigRational b) { return a.Equals(b); }
        public static bool operator !=(BigRational a, BigRational b) { return !a.Equals(b); }
        public static bool operator <(BigRational a, BigRational b) { return a.CompareTo(b) < 0; }
        public static bool operator <=(BigRational a, BigRational b) { return a.CompareTo(b) <= 0; }
        public static bool operator >(BigRational a, BigRational b) { return a.CompareTo(b) > 0; }
        public static bool operator >=(BigRational a, BigRational b) { return a.CompareTo(b) >= 0; }

        // Conversions.
        public static implicit operator BigRational(BigInteger x) { return new BigRational(x); }
        public static implicit operator BigRational(long x) { return new BigRational(x); }
        public static implicit operator BigRational(int x) { return new BigRational(x); }

        public static implicit operator BigRational(double x)
        {
            // http://stackoverflow.com/questions/389993/extracting-mantissa-and-exponent-from-double-in-c-sharp

            // Translate the double into sign, exponent and mantissa.
            long bits = BitConverter.DoubleToInt64Bits(x);
            // Note that the shift is sign-extended, hence the test against -1 not 1
            int sign = (bits < 0) ? -1 : 1;
            int exponent = (int)((bits >> 52) & 0x7ffL);
            long mantissa = bits & 0xfffffffffffffL;

            // Subnormal numbers; exponent is effectively one higher,
            // but there's no extra normalisation bit in the mantissa
            if (exponent == 0)
            {
                exponent++;
            }
            // Normal numbers; leave exponent as it is but add extra
            // bit to the front of the mantissa
            else
            {
                mantissa = mantissa | (1L << 52);
            }

            // Bias the exponent. It's actually biased by 1023, but we're
            // treating the mantissa as m.0 rather than 0.m, so we need
            // to subtract another 52 from it.
            exponent -= 1023 + 52; // 1075;

            if (mantissa > 0)
            {
                /* Normalize */
                while ((mantissa & 1) == 0)
                {    /*  i.e., Mantissa is even */
                    mantissa >>= 1;
                    exponent++;
                }
            }

            if (exponent > 0)
                return new BigRational(new BigInteger(sign * mantissa) << exponent);
            else
                return new BigRational(new BigInteger(sign * mantissa), new BigInteger(1) << -exponent);
        }

        private static BigInteger b1 = 1UL << 32;
        private static BigInteger b2 = b1 * b1;
        public static implicit operator BigRational(decimal x)
        {
            int[] bits = decimal.GetBits(x);

            int sign = (bits[3] & (1 << 31)) != 0 ? -1 : 1;
            int exponent = (bits[3] >> 16) & ((1 << 7) - 1);

            return new BigRational(
                sign * (bits[0] + bits[1] * b1 + bits[2] * b2),
                BigInteger.Pow(10, exponent));
        }

        public static explicit operator BigInteger(BigRational x) { return x.n / x.d; }
        public static explicit operator decimal(BigRational x) { return (decimal)x.n / (decimal)x.d; }
        public static explicit operator long(BigRational x) { return (long)(x.n / x.d); }
        public static explicit operator int(BigRational x) { return (int)(x.n / x.d); }
        public static explicit operator double(BigRational x) { return (double)x.n / (double)x.d; }

        // Useful functions.
        public static BigRational Abs(BigRational x) { return BigRational.Unchecked(BigInteger.Abs(x.n), x.d); }
        public static int Sign(BigRational x) { return x.n.Sign; }

        public static BigInteger Floor(BigInteger n, BigInteger d)
        {
            BigInteger r;
            BigInteger q = BigInteger.DivRem(n, d, out r);

            if (r.IsZero)
                return q;
            else if (r.Sign == -1)
                return q - 1;
            else
                return q;
        }

        public static BigInteger Floor(BigRational x) { return Floor(x.n, x.d); }
        public static BigInteger Ceiling(BigRational x) { return Floor(x.n + x.d - 1, x.d); }
        public static BigInteger Round(BigRational x) { return Floor(x.n + (x.d >> 1), x.d); }
    }
}
