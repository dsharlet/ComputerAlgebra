using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

namespace ComputerAlgebra
{
    public enum Operator
    {
        // Binary arithmetic.
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        
        // Binary logic.
        And,
        Or,

        // Unary arithmetic.
        Negate,
        Inverse,

        // Unary prime operator.
        Prime,

        // Unary logic.
        Not,

        // Binary logic.
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        ApproxEqual,

        // Substitutions
        Arrow,
        Substitute,
    }

    [TypeConverter(typeof(ExpressionConverter))]
    public abstract class Expression : IComparable<Expression>, IEquatable<Expression>
    {
        /// <summary>
        /// Try matching this pattern against an expression such that if Matches(Expr, Matched) is true, Substitute(Matched) is equal to Expr.
        /// </summary>
        /// <param name="Expr">Expression to match.</param>
        /// <param name="Matched">A context to store matched variables.</param>
        /// <returns>true if Equals(Expr.Substitute(Matched)) is true.</returns>
        public virtual bool Matches(Expression Expr, MatchContext Matched) { return Equals(Expr); }
        public MatchContext Matches(Expression Expr, IEnumerable<Arrow> PreMatch)
        {
            MatchContext Matched = new MatchContext(Expr, PreMatch);
            if (!Matches(Expr, Matched))
                return null;
            
            return Matched;
        }
        public MatchContext Matches(Expression Expr, params Arrow[] PreMatch)
        {
            return Matches(Expr, PreMatch.AsEnumerable());
        }
        
        public virtual bool EqualsZero() { return false; }
        public virtual bool EqualsOne() { return false; }
        public virtual bool IsFalse() { return EqualsZero(); }
        public virtual bool IsTrue() { return this is Constant && !IsFalse(); }
        public virtual bool IsInteger() { return false; }

        /// <summary>
        /// Parse an expression from a string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static Parser parser = new Parser(Namespace.Global);
        public static Expression Parse(string s) { return parser.Parse(s); }

        // Expression operators.
        public static Expression operator +(Expression L, Expression R) { return Sum.New(L, R).Evaluate(); }
        public static Expression operator -(Expression L, Expression R) { return L + Unary.Negate(R); }
        public static Expression operator *(Expression L, Expression R) { return Product.New(L, R).Evaluate(); }
        public static Expression operator /(Expression L, Expression R) { return L * Power.New(R, -1); }
        public static Expression operator ^(Expression L, Expression R) { return Binary.Power(L, R).Evaluate(); }
        public static Expression operator -(Expression O) { return -1 * O; }

        public static Expression operator &(Expression L, Expression R) { return Binary.And(L, R).Evaluate(); }
        public static Expression operator |(Expression L, Expression R) { return Binary.Or(L, R).Evaluate(); }
        public static Expression operator !(Expression O) { return Unary.Not(O).Evaluate(); }

        public static Expression operator ==(Expression L, Expression R) 
        {
            if (ReferenceEquals(L, null) || ReferenceEquals(R, null))
                return Constant.New(ReferenceEquals(L, R));
            return Binary.Equal(L, R).Evaluate();
        }
        public static Expression operator !=(Expression L, Expression R)
        {
            if (ReferenceEquals(L, null) || ReferenceEquals(R, null))
                return Constant.New(!ReferenceEquals(L, R));
            return Binary.NotEqual(L, R).Evaluate();
        }
        public static Expression operator <(Expression L, Expression R) { return Binary.Less(L, R).Evaluate(); }
        public static Expression operator <=(Expression L, Expression R) { return Binary.LessEqual(L, R).Evaluate(); }
        public static Expression operator >(Expression L, Expression R) { return Binary.Greater(L, R).Evaluate(); }
        public static Expression operator >=(Expression L, Expression R) { return Binary.GreaterEqual(L, R).Evaluate(); }

        public static implicit operator Expression(Real x) { return Constant.New(x); }
        public static implicit operator Expression(BigRational x) { return Constant.New(x); }
        public static implicit operator Expression(decimal x) { return Constant.New(x); }
        public static implicit operator Expression(double x) { return Constant.New(x); }
        public static implicit operator Expression(int x) { return Constant.New(x); }
        public static implicit operator Expression(string x) { return Parse(x); }
        public static implicit operator bool(Expression x) { return x.IsTrue(); }
        public static explicit operator Real(Expression x) { return (Constant)x; }
        public static explicit operator double(Expression x) { return (double)(Real)x; }

        public static bool operator true(Expression x) { return x.IsTrue(); }
        public static bool operator false(Expression x) { return x.IsFalse(); }

        public string ToString(int Precedence)
        {
            string r = ToString();
            if (Parser.Precedence(this) < Precedence)
                r = "(" + r + ")";
            return r;
        }

        // object interface.
        public virtual bool Equals(Expression E) 
        {
            if (!(this is Call) && E is Call)
                return E.Equals(this);
            return false;
        }
        public sealed override bool Equals(object obj)
        {
            Expression E = obj as Expression;
            return ReferenceEquals(E, null) ? false : Equals(E);
        }
        public abstract override int GetHashCode();
        
        /// <summary>
        /// Get an ordered list of the atomic Expression elements in this expression.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Atom> Atoms { get; }
        public virtual int CompareTo(Expression R) 
        {
            if (ReferenceEquals(this, R))
                return 0;
            return Atoms.LexicalCompareTo(R.Atoms); 
        }

        /// <summary>
        /// Given a list of CompareTo functions, reduce to a CompareTo via lexical ordering.
        /// </summary>
        /// <param name="Compares"></param>
        /// <returns></returns>
        protected static int LexicalCompareTo(params Func<int>[] Compares)
        {
            foreach (Func<int> i in Compares)
            {
                int compare = i();
                if (compare != 0)
                    return compare;
            }
            return 0;
        }
        
        public static readonly ReferenceEqualityComparer<Expression> RefComparer = new ReferenceEqualityComparer<Expression>();
    }
}
