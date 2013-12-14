using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents a unary operator expression.
    /// </summary>
    public class Unary : Expression
    {
        /// <summary>
        /// Determine if Op is a unary logic operator.
        /// </summary>
        /// <param name="Op"></param>
        /// <returns></returns>
        public static bool IsLogic(Operator Op)
        {
            switch (Op)
            {
                case Operator.Not:
                    return true;
                default:
                    return false;
            }
        }

        protected Operator op;
        /// <summary>
        /// Get the operator type of this unary expression.
        /// </summary>
        public Operator Operator { get { return op; } }

        protected Expression o;
        /// <summary>
        /// Get the operand of this unary expression.
        /// </summary>
        public Expression Operand { get { return o; } }

        /// <summary>
        /// Determine if this unary expression operator is a logical operation.
        /// </summary>
        public bool IsLogicOp { get { return IsLogic(op); } }

        protected Unary(Operator Op, Expression Operand) { op = Op; o = Operand; }

        /// <summary>
        /// Create a new prime unary expression. Prime(x) -> x'.
        /// </summary>
        /// <param name="Operand"></param>
        /// <returns></returns>
        public static Expression Prime(Expression Operand) 
        { 
            Call f = Operand as Call;
            if (f != null && f.Arguments.Count() == 1)
                return Call.D(f, f.Arguments.First());
            return new Unary(Operator.Prime, Operand); 
        }
        /// <summary>
        /// Create a new negation unary expression. Negate(x) -> -x.
        /// </summary>
        /// <param name="Operand"></param>
        /// <returns></returns>
        public static Expression Negate(Expression Operand) { return Product.New(-1, Operand); }

        /// <summary>
        /// Create a new inverse unary expression. Inverse(x) -> 1/x.
        /// </summary>
        /// <param name="Operand"></param>
        /// <returns></returns>
        public static Expression Inverse(Expression Operand) { return Binary.Power(Operand, -1); }

        /// <summary>
        /// Create a new logic not unary expression. Not(x) -> !x.
        /// </summary>
        /// <param name="Operand"></param>
        /// <returns></returns>
        public static Expression Not(Expression Operand) { return new Unary(Operator.Not, Operand); }

        /// <summary>
        /// Create a new unary operator expression.
        /// </summary>
        /// <param name="Op"></param>
        /// <param name="Operand"></param>
        /// <returns></returns>
        public static Expression New(Operator Op, Expression Operand)
        {
            switch (Op)
            {
                case Operator.Prime: return Prime(Operand);
                case Operator.Negate: return Negate(Operand);
                case Operator.Inverse: return Inverse(Operand);
                case Operator.Not: return Not(Operand);
                default: return new Unary(Op, Operand);
            }
        }

        public static string ToStringPrefix(Operator o)
        {
            switch (o)
            {
                case Operator.Prime: return "";
                case Operator.Negate: return "-";
                case Operator.Inverse: return "";
                case Operator.Not: return "!";
                default: return "<unary op>";
            }
        }

        public static string ToStringPostfix(Operator o)
        {
            switch (o)
            {
                case Operator.Prime: return "'";
                case Operator.Negate: return "";
                case Operator.Inverse: return "^-1";
                case Operator.Not: return "";
                default: return "<unary op>";
            }
        }

        // object interface.
        public override string ToString() { return ToStringPrefix(Operator) + Operand.ToString(Parser.Precedence(Operator)) + ToStringPostfix(Operator); }
        public override int GetHashCode() { return Operator.GetHashCode() ^ Operand.GetHashCode(); }
        public override bool Equals(Expression E)
        {
            Unary U = E as Unary;
            if (ReferenceEquals(U, null)) return base.Equals(E);
            
            return Operator.Equals(U.Operator) && Operand.Equals(U.Operand);
        }

        // Expression interface.
        public override IEnumerable<Atom> Atoms { get { return Operand.Atoms; } }
        public override int CompareTo(Expression R)
        {
            Unary RU = R as Unary;
            if (!ReferenceEquals(RU, null))
                return LexicalCompareTo(
                    () => Operator.CompareTo(RU.Operator),
                    () => Operand.CompareTo(RU.Operand));

            return base.CompareTo(R);
        }
    }
}
