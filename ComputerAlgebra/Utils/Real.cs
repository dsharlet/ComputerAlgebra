﻿using System;
using System.ComponentModel;
using System.Numerics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Arbitrary precision real number. Represents numbers as close to exactly as possible.
    /// </summary>
    [TypeConverter(typeof(RealConverter))]
    public struct Real : IComparable<Real>, IEquatable<Real>, IFormattable
    {
        private BigRational r;

        private static BigRational PositiveInfinity = BigRational.Unchecked(1, 0);
        private static BigRational NegativeInfinity = BigRational.Unchecked(-1, 0);

        private static BigRational NaN = BigRational.Unchecked(0, 0);

        public Real(int x) { r = x; }
        public Real(decimal x) { r = x; }
        public Real(BigInteger x) { r = x; }
        public Real(BigRational x) { r = x; }
        public Real(double x)
        {
            if (double.IsNaN(x))
                r = NaN;
            else if (double.IsPositiveInfinity(x))
                r = PositiveInfinity;
            else if (double.IsNegativeInfinity(x))
                r = NegativeInfinity;
            else
                r = x;
        }

        public static Real Parse(string s) { return double.Parse(s); }

        public static readonly Real Infinity = new Real(PositiveInfinity);

        public bool IsNaN() { return r.Equals(0, 0); }
        public bool IsInteger() { return r.IsInteger(); }
        public bool EqualsZero() { return r.EqualsZero(); }
        public bool EqualsOne() { return r.EqualsOne(); }

        public static explicit operator bool(Real x) { return !x.r.EqualsZero(); }
        public static explicit operator int(Real x) { return (int)x.r; }
        public static explicit operator double(Real x) { return (double)x.r; }
        public static explicit operator decimal(Real x) { return (decimal)x.r; }
        public static explicit operator BigInteger(Real x) { return (BigInteger)x.r; }
        public static explicit operator BigRational(Real x) { return x.r; }

        public static implicit operator Real(bool x) { return new Real(x ? 1 : 0); }
        public static implicit operator Real(int x) { return new Real(x); }
        public static implicit operator Real(double x) { return new Real(x); }
        public static implicit operator Real(decimal x) { return new Real(x); }
        public static implicit operator Real(BigInteger x) { return new Real(x); }
        public static implicit operator Real(BigRational x) { return new Real(x); }

        // Relational operators.
        public static bool operator ==(Real a, Real b) { return a.CompareTo(b) == 0; }
        public static bool operator !=(Real a, Real b) { return a.CompareTo(b) != 0; }
        public static bool operator <(Real a, Real b) { return a.CompareTo(b) < 0; }
        public static bool operator <=(Real a, Real b) { return a.CompareTo(b) <= 0; }
        public static bool operator >(Real a, Real b) { return a.CompareTo(b) > 0; }
        public static bool operator >=(Real a, Real b) { return a.CompareTo(b) >= 0; }

        // Arithmetic operators.
        public static Real operator +(Real a, Real b) { return a.r + b.r; }
        public static Real operator -(Real a, Real b) { return a.r - b.r; }
        public static Real operator *(Real a, Real b) { return a.r * b.r; }
        public static Real operator %(Real a, Real b) { return a.r % b.r; }
        public static Real operator /(Real a, Real b) { return a.r / b.r; }
        public static Real operator ^(Real a, Real b)
        {
            if (b.IsInteger())
                return a.r ^ (int)b.r;
            else
                return Math.Pow((double)a.r, (double)b.r);
        }
        public static Real operator -(Real x) { return -x.r; }

        public static readonly Real Pi = Math.PI;
        public static readonly Real e = Math.E;

        // Math functions
        public static Real Abs(Real x) { return BigRational.Abs(x.r); }
        public static int Sign(Real x) { return BigRational.Sign(x.r); }

        public static Real Min(Real x, Real y) { return x < y ? x : y; }
        public static Real Max(Real x, Real y) { return x > y ? x : y; }

        public static Real Sin(Real x) { return Math.Sin((double)x); }
        public static Real Cos(Real x) { return Math.Cos((double)x); }
        public static Real Tan(Real x) { return Math.Tan((double)x); }
        public static Real Sec(Real x) { return 1 / Cos(x); }
        public static Real Csc(Real x) { return 1 / Sin(x); }
        public static Real Cot(Real x) { return 1 / Tan(x); }

        public static Real ArcSin(Real x) { return Math.Asin((double)x); }
        public static Real ArcCos(Real x) { return Math.Acos((double)x); }
        public static Real ArcTan(Real x) { return Math.Atan((double)x); }
        public static Real ArcSec(Real x) { return ArcCos(1 / x); }
        public static Real ArcCsc(Real x) { return ArcSin(1 / x); }
        public static Real ArcCot(Real x) { return ArcTan(1 / x); }

        public static Real Sinh(Real x) { return Math.Sinh((double)x); }
        public static Real Cosh(Real x) { return Math.Cosh((double)x); }
        public static Real Tanh(Real x) { return Math.Tanh((double)x); }
        public static Real Sech(Real x) { return 1 / Cosh(x); }
        public static Real Csch(Real x) { return 1 / Sinh(x); }
        public static Real Coth(Real x) { return 1 / Tanh(x); }

        public static Real ArcSinh(Real x) { return Ln(x + Sqrt(x * x + 1)); }
        public static Real ArcCosh(Real x) { return Ln(x + Sqrt(x * x - 1)); }
        public static Real ArcTanh(Real x) { return (Ln(1 + x) - Ln(1 - x)) / 2; }
        public static Real ArcSech(Real x) { return ArcCosh(1 / x); }
        public static Real ArcCsch(Real x) { return ArcSinh(1 / x); }
        public static Real ArcCoth(Real x) { return ArcTanh(1 / x); }

        public static Real Sqrt(Real x) { return Math.Sqrt((double)x); }
        public static Real Exp(Real x) { return Math.Exp((double)x); }
        public static Real Ln(Real x) { return Math.Log((double)x); }
        public static Real Log(Real x, Real b) { return Math.Log((double)x, (double)b); }
        public static Real Log10(Real x) { return Math.Log((double)x, 10.0); }

        public static Real Floor(Real x) { return BigRational.Floor(x.r); }
        public static Real Ceiling(Real x) { return BigRational.Ceiling(x.r); }
        public static Real Round(Real x) { return BigRational.Round(x.r); }

        // IComparable interface.
        public int CompareTo(Real x) { return r.CompareTo(x.r); }

        // IEquatable interface.
        public bool Equals(Real x) { return r.Equals(x.r); }

        public string ToLaTeX()
        {
            if (Equals(Infinity)) return @"\infty";
            if (Equals(-Infinity)) return @"-\infty";
            if (Equals(NaN)) return "NaN";

            return r.ToLaTeX();
        }

        // IFormattable interface.
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (Equals(Infinity)) return "\u221E";
            if (Equals(-Infinity)) return "-\u221E";
            if (Equals(NaN)) return "NaN";

            return r.ToString(format, formatProvider);
        }
        public string ToString(string format) { return ToString(format, null); }

        // object interface.
        public override bool Equals(object obj) { return obj is Real ? Equals((Real)obj) : base.Equals(obj); }
        public override int GetHashCode() { return r.GetHashCode(); }
        public override string ToString() { return r.ToString(); }
    }
}
