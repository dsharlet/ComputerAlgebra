using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a polynomial of one variable.
    /// </summary>
    public class Polynomial : Sum
    {
        protected DefaultDictionary<int, Expression> coefficients;
        protected Expression variable;

        public override IEnumerable<Expression> Terms
        {
            get
            {
                foreach (KeyValuePair<int, Expression> i in coefficients)
                    yield return Product.New(i.Value, Power.New(variable, Constant.New(i.Key)));
            }
        }

        public Expression Variable { get { return variable; } }

        /// <summary>
        /// Get the coefficients of this Polynomial.
        /// </summary>
        public IEnumerable<KeyValuePair<int, Expression>> Coefficients { get { return coefficients.OrderBy(i => i.Key); } }

        /// <summary>
        /// Find the degree of this Polynomial.
        /// </summary>
        public int Degree { get { return coefficients.Max(i => i.Key, 0); } }

        /// <summary>
        /// Access the coefficients of this polynomial.
        /// </summary>
        /// <param name="d">Degree of the term to access.</param>
        /// <returns></returns>
        public Expression this[int d] { get { return coefficients[d]; } }

        private Polynomial(IEnumerable<KeyValuePair<int, Expression>> Coefficients, Expression Variable)
        {
            coefficients = new DefaultDictionary<int, Expression>(0);
            foreach (KeyValuePair<int, Expression> i in Coefficients)
                coefficients.Add(i.Key, i.Value);
            variable = Variable;
        }

        private Polynomial(DefaultDictionary<int, Expression> Coefficients, Expression Variable)
        {
            coefficients = Coefficients;
            variable = Variable;
        }

        /// <summary>
        /// Construct a polynomial of x from f(x).
        /// </summary>
        /// <param name="f"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Polynomial New(Expression f, Expression x)
        {
            // Match each term to A*x^N where A is constant with respect to x, and N is an integer.
            Variable A = PatternVariable.New("A", i => !i.DependsOn(x));
            Variable N = PatternVariable.New("N", i => i.IsInteger());
            Expression TermPattern = Product.New(A, Power.New(x, N));

            DefaultDictionary<int, Expression> P = new DefaultDictionary<int, Expression>(0);

            foreach (Expression i in Sum.TermsOf(f))
            {
                MatchContext Matched = TermPattern.Matches(i, Arrow.New(x, x));
                if (Matched == null)
                    throw new ArgumentException("f is not a polynomial of x.");

                int n = (int)Matched[N];
                P[n] += Matched[A];
            }

            return new Polynomial(P, x);
        }

        public Expression Factor()
        {
            // Check if there is a simple factor of x.
            if (this[0].EqualsZero())
                return Product.New(variable, new Polynomial(Coefficients.Where(i => i.Key != 0).ToDictionary(i => i.Key - 1, i => i.Value), variable).Factor()).Evaluate();

            DefaultDictionary<Expression, int> factors = new DefaultDictionary<Expression, int>(0);
            switch (Degree)
            {
                //case 2:
                //    Expression a = this[2];
                //    Expression b = this[1];
                //    Expression c = this[0];

                //    // D = b^2 - 4*a*c
                //    Expression D = Add.New(Multiply.New(b, b), Multiply.New(-4, a, c));
                //    factors[Binary.Divide(Add.New(Unary.Negate(b), Call.Sqrt(D)), Multiply.New(2, a))] += 1;
                //    factors[Binary.Divide(Add.New(Unary.Negate(b), Call.Sqrt(D)), Multiply.New(2, a))] += 1;
                //    break;
                default:
                    return this;
            }

            // Assemble expression from factors.
            //return Multiply.New(factors.Select(i => Power.New(Binary.Subtract(x, i.Key), i.Value)));
        }

        /// <summary>
        /// Test if this polynomial is equal to P.
        /// </summary>
        /// <param name="P"></param>
        /// <returns></returns>
        public bool Equals(Polynomial P)
        {
            int d = Degree;
            if (d != P.Degree)
                return false;
            for (int i = 0; i <= d; ++i)
                if (!this[i].Equals(P[i]))
                    return false;
            return true;
        }

        public override bool Equals(Expression E)
        {
            if (E is Polynomial P)
                return Equals(P);
            return base.Equals(E);
        }
        public override int GetHashCode() { return coefficients.OrderedHashCode() ^ variable.GetHashCode(); }

        /// <summary>
        /// Implicitly convert variables to degree 1 polynomials.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static implicit operator Polynomial(Variable x) { return new Polynomial(new[] { new KeyValuePair<int, Expression>(1, 1) }, x); }

        public static Polynomial Multiply(Polynomial L, Polynomial R)
        {
            if (!Equals(L.Variable, R.Variable))
                throw new ArgumentException("Polynomials must be of the same variable.", "L*R");

            Dictionary<int, Expression> P = new Dictionary<int, Expression>();
            int D = L.Degree + R.Degree;
            for (int i = 0; i <= D; ++i)
                for (int j = 0; j <= i; ++j)
                    P[i] += L[j] * R[i - j];

            return new Polynomial(P, L.Variable);
        }

        public static Polynomial Add(Polynomial L, Polynomial R)
        {
            if (!Equals(L.Variable, R.Variable))
                throw new ArgumentException("Polynomials must be of the same variable.", "L+R");

            DefaultDictionary<int, Expression> P = new DefaultDictionary<int, Expression>(0);
            int D = Math.Max(L.Degree, R.Degree);
            for (int i = 0; i <= D; ++i)
                P[i] = L[i] + R[i];

            return new Polynomial(P, L.Variable);
        }

        public static Polynomial Divide(Polynomial N, Polynomial D, out Polynomial R)
        {
            if (!Equals(N.Variable, D.Variable))
                throw new ArgumentException("Polynomials must be of the same variable.", "N/D");

            DefaultDictionary<int, Expression> q = new DefaultDictionary<int, Expression>(0);
            DefaultDictionary<int, Expression> r = new DefaultDictionary<int, Expression>(0);
            foreach (KeyValuePair<int, Expression> i in N.Coefficients)
                r.Add(i.Key, i.Value);

            while (r.Any() && !r[0].Equals(0) && r.Keys.Max() + 1 >= D.Degree)
            {
                int rd = r.Keys.Max() + 1;
                int dd = D.Degree;
                Expression t = r[rd] / D[dd];
                int td = rd - dd;

                // Compute q += t
                q[td] += t;

                // Compute r -= d * t
                for (int i = 0; i <= dd; ++i)
                    r[i + td] -= -D[i] * t;
            }
            R = new Polynomial(r, N.Variable);
            return new Polynomial(q, N.Variable);
        }

        public static Polynomial operator -(Polynomial P)
        {
            return new Polynomial(P.Coefficients.Select(i => new KeyValuePair<int, Expression>(i.Key, -i.Value)), P.Variable);
        }

        public static Expression operator +(Polynomial L, Polynomial R)
        {
            if (Equals(L.Variable, R.Variable))
                return Add(L, R);
            else
                return (Expression)L + (Expression)R;
        }

        public static Expression operator -(Polynomial L, Polynomial R)
        {
            if (Equals(L.Variable, R.Variable))
                return Add(L, -R);
            else
                return (Expression)L - (Expression)R;
        }

        public static Expression operator *(Polynomial L, Polynomial R)
        {
            if (Equals(L.Variable, R.Variable))
                return Multiply(L, R);
            else
                return (Expression)L * (Expression)R;
        }

        public static Expression operator /(Polynomial N, Polynomial D)
        {
            if (Equals(N.Variable, D.Variable))
            {
                Polynomial R;
                Polynomial Q = Divide(N, D, out R);
                return Sum.New(Q, Binary.Divide(R, D));
            }
            else
            {
                return (Expression)N / (Expression)D;
            }
        }
    }
}
