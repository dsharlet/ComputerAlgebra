using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    /// <summary>
    /// Binary operator expression type.
    /// </summary>
    public class Binary : Expression
    {
        /// <summary>
        /// Check if the operator is commuative.
        /// </summary>
        /// <param name="Op"></param>
        /// <returns></returns>
        public static bool IsCommutative(Operator Op)
        {
            switch (Op)
            {
                case Operator.Add:
                case Operator.Multiply:
                case Operator.And:
                case Operator.Or:
                case Operator.Equal:
                case Operator.NotEqual:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if the operator is a logical operator.
        /// </summary>
        /// <param name="Op"></param>
        /// <returns></returns>
        public static bool IsLogic(Operator Op)
        {
            switch (Op)
            {
                case Operator.Equal:
                case Operator.NotEqual:
                case Operator.Greater:
                case Operator.Less:
                case Operator.GreaterEqual:
                case Operator.LessEqual:
                case Operator.ApproxEqual:
                    return true;
                default:
                    return false;
            }
        }

        protected Operator op;
        /// <summary>
        /// Get the operator for this binary expression.
        /// </summary>
        public Operator Operator { get { return op; } }

        protected Expression l, r;
        /// <summary>
        /// Get the left operand of this binary expression.
        /// </summary>
        public Expression Left { get { return l; } }
        /// <summary>
        /// Get the right operand of this binary expression.
        /// </summary>
        public Expression Right { get { return r; } }
        
        /// <summary>
        /// Determine if this operator expression is commutative.
        /// </summary>
        public bool IsCommutativeOp { get { return IsCommutative(op); } }
        /// <summary>
        /// Determine if this operator expression is a logic operation.
        /// </summary>
        public bool IsLogicOp { get { return IsLogic(op); } }
        
        protected Binary(Operator Op, Expression L, Expression R) { op = Op; l = L; r = R; }

        static public Expression Add(Expression L, Expression R) { return ComputerAlgebra.Sum.New(L, R); }
        static public Expression Subtract(Expression L, Expression R) { return ComputerAlgebra.Sum.New(L, Unary.Negate(R)); }
        static public Expression Multiply(Expression L, Expression R) { return ComputerAlgebra.Product.New(L, R); }
        static public Expression Divide(Expression L, Expression R) { return ComputerAlgebra.Product.New(L, Unary.Inverse(R)); }
        static public Expression Power(Expression L, Expression R) { return ComputerAlgebra.Power.New(L, R); }

        static public Binary Arrow(Expression L, Expression R) { return ComputerAlgebra.Arrow.New(L, R); }
        static public Binary Substitute(Expression L, Expression R) { return ComputerAlgebra.Substitute.New(L, R); }
        static public Binary Equal(Expression L, Expression R) { return ComputerAlgebra.Equal.New(L, R); }

        static public Binary And(Expression L, Expression R) { return new Binary(Operator.And, L, R); }
        static public Binary Or(Expression L, Expression R) { return new Binary(Operator.Or, L, R); }

        static public Binary NotEqual(Expression L, Expression R) { return new Binary(Operator.NotEqual, L, R); }
        static public Binary Greater(Expression L, Expression R) { return new Binary(Operator.Greater, L, R); }
        static public Binary Less(Expression L, Expression R) { return new Binary(Operator.Less, L, R); }
        static public Binary GreaterEqual(Expression L, Expression R) { return new Binary(Operator.GreaterEqual, L, R); }
        static public Binary LessEqual(Expression L, Expression R) { return new Binary(Operator.LessEqual, L, R); }
        static public Binary ApproxEqual(Expression L, Expression R) { return new Binary(Operator.ApproxEqual, L, R); }
        /// <summary>
        /// Create a new binary expression.
        /// </summary>
        /// <param name="Op"></param>
        /// <param name="L"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        static public Expression New(Operator Op, Expression L, Expression R)
        {
            switch (Op)
            {
                case Operator.Add: return Add(L, R);
                case Operator.Subtract: return Subtract(L, R);
                case Operator.Multiply: return Multiply(L, R);
                case Operator.Divide: return Divide(L, R);
                case Operator.Power: return Power(L, R);
                case Operator.And: return And(L, R);
                case Operator.Or: return Or(L, R);
                case Operator.Equal: return Equal(L, R);
                case Operator.NotEqual: return NotEqual(L, R);
                case Operator.Greater: return Greater(L, R);
                case Operator.Less: return Less(L, R);
                case Operator.GreaterEqual: return GreaterEqual(L, R);
                case Operator.LessEqual: return LessEqual(L, R);
                case Operator.ApproxEqual: return ApproxEqual(L, R);
                case Operator.Arrow: return Arrow(L, R);
                case Operator.Substitute: return Substitute(L, R);
                default: return new Binary(Op, L, R);
            }
        }

        public static string ToString(Operator o)
        {
            switch (o)
            {
                case Operator.Add: return " + ";
                case Operator.Subtract: return " - ";
                case Operator.Multiply: return "*";
                case Operator.Divide: return "/";
                case Operator.Power: return "^";

                case Operator.And: return "&";
                case Operator.Or: return "|";

                case Operator.Equal: return "==";
                case Operator.NotEqual: return "!=";
                case Operator.Greater: return ">";
                case Operator.Less: return "<";
                case Operator.GreaterEqual: return ">=";
                case Operator.LessEqual: return "<=";
                case Operator.ApproxEqual: return "~=";

                case Operator.Arrow: return "->";
                case Operator.Substitute: return ":";
                default: return "<unknown>";
            }
        }

        public override bool Matches(Expression E, MatchContext Matched)
        {
            Binary BE = E as Binary;
            if (!ReferenceEquals(BE, null) && BE.Operator == Operator)
                return Matched.TryMatch(() => Left.Matches(BE.Left, Matched) && Right.Matches(BE.Right, Matched));

            return false;
        }

        // object interface.
        public override string ToString() 
        {
            int pr = Parser.Precedence(Operator);
            return Left.ToString(pr) + ToString(Operator) + Right.ToString(pr);
        }
        public override int GetHashCode() { return Operator.GetHashCode() ^ Left.GetHashCode() ^ Right.GetHashCode(); }

        // Expression interface.
        public override IEnumerable<Atom> Atoms { get { return Left.Atoms.Concat(Right.Atoms); } }
        public override bool Equals(Expression E)
        {
            Binary B = E as Binary;
            if (ReferenceEquals(B, null)) return base.Equals(E);

            return Operator.Equals(B.Operator) &&
                Left.Equals(B.Left) &&
                Right.Equals(B.Right);
        }
        public override int CompareTo(Expression R)
        {
            Binary RB = R as Binary;
            if (!ReferenceEquals(RB, null))
                return LexicalCompareTo(
                    () => Operator.CompareTo(RB.Operator),
                    () => Left.CompareTo(RB.Left),
                    () => Right.CompareTo(RB.Right));

            return base.CompareTo(R);
        }
    }
}
