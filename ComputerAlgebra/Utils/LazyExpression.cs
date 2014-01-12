using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    /// <summary>
    /// Expression that is evaluated upon conversion to Expression.
    /// </summary>
    public class LazyExpression
    {
        private Expression value;
        public Expression Evaluate() { return value.Evaluate(); }

        /// <summary>
        /// Construct a new lazy expression.
        /// </summary>
        /// <param name="Expr">Expression to be lazily evaluated.</param>
        public LazyExpression(Expression Expr) { value = Expr; }
        
        /// <summary>
        /// Implicit conversion from LazyExpression to Expression.
        /// </summary>
        /// <param name="LazyExpr"></param>
        /// <returns>Evaluated value of x.</returns>
        public static implicit operator Expression(LazyExpression x) { return x.Evaluate(); }

        public static implicit operator LazyExpression(Real x) { return new LazyExpression(x); }
        public static implicit operator LazyExpression(BigRational x) { return new LazyExpression(x); }
        public static implicit operator LazyExpression(decimal x) { return new LazyExpression(x); }
        public static implicit operator LazyExpression(double x) { return new LazyExpression(x); }
        public static implicit operator LazyExpression(int x) { return new LazyExpression(x); }
        public static implicit operator LazyExpression(string x) { return new LazyExpression(x); }

        public static implicit operator bool(LazyExpression x) { return (Expression)x; }
        public static explicit operator Real(LazyExpression x) { return (Real)(Expression)x; }
        public static explicit operator double(LazyExpression x) { return (double)(Expression)x; }

        public static bool operator true(LazyExpression x) { return ((Expression)x).IsTrue(); }
        public static bool operator false(LazyExpression x) { return ((Expression)x).IsFalse(); }

        // Expression operators.
        public static LazyExpression operator +(LazyExpression L, LazyExpression R) { return new LazyExpression(Sum.New(L.value, R.value)); }
        public static LazyExpression operator -(LazyExpression L, LazyExpression R) { return new LazyExpression(Sum.New(L.value, Unary.Negate(R.value))); }
        public static LazyExpression operator *(LazyExpression L, LazyExpression R) { return new LazyExpression(Product.New(L.value, R.value)); }
        public static LazyExpression operator /(LazyExpression L, LazyExpression R) { return new LazyExpression(Product.New(L.value, Power.New(R.value, -1))); }
        public static LazyExpression operator ^(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.Power(L.value, R.value)); }
        public static LazyExpression operator -(LazyExpression O) { return new LazyExpression(Product.New(-1, O.value)); }

        public static LazyExpression operator &(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.And(L.value, R.value)); }
        public static LazyExpression operator |(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.Or(L.value, R.value)); }
        public static LazyExpression operator !(LazyExpression O) { return new LazyExpression(Unary.Not(O.value)); }

        public static LazyExpression operator ==(LazyExpression L, LazyExpression R)
        {
            if (ReferenceEquals(L.value, null) || ReferenceEquals(R.value, null))
                return new LazyExpression(Constant.New(ReferenceEquals(L.value, R.value)));
            return new LazyExpression(Binary.Equal(L.value, R.value));
        }
        public static LazyExpression operator !=(LazyExpression L, LazyExpression R)
        {
            if (ReferenceEquals(L.value, null) || ReferenceEquals(R.value, null))
                return new LazyExpression(Constant.New(!ReferenceEquals(L.value, R.value)));
            return new LazyExpression(Binary.NotEqual(L.value, R.value));
        }
        public static LazyExpression operator <(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.Less(L.value, R.value)); }
        public static LazyExpression operator <=(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.LessEqual(L.value, R.value)); }
        public static LazyExpression operator >(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.Greater(L.value, R.value)); }
        public static LazyExpression operator >=(LazyExpression L, LazyExpression R) { return new LazyExpression(Binary.GreaterEqual(L.value, R.value)); }

        // object interface.
        public override int GetHashCode() { return value.GetHashCode(); }
        public override bool Equals(object obj) { return value.Equals(obj); }
    }
}
