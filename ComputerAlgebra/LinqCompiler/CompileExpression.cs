using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using LinqExprs = System.Linq.Expressions;
using LinqExpr = System.Linq.Expressions.Expression;

namespace ComputerAlgebra.LinqCompiler
{
    /// <summary>
    /// Visitor to generate LINQ expressions for our Expressions.
    /// </summary>
    class CompileExpression : ExpressionVisitor<LinqExpr>
    {
        private CodeGen target;
        
        public CompileExpression(CodeGen Target) { target = Target; }
        
        public override LinqExpr Visit(Expression E)
        {
            // Check if this expression has already been compiled to an intermediate expression or otherwise.
            LinqExpr ret = target.LookUp(E);
            if (ret != null)
                return ret;

            return base.Visit(E);
        }

        protected override LinqExpr VisitSum(Sum A)
        {
            LinqExpr _int = ForArithmetic(Visit(A.Terms.First()));
            foreach (Expression i in A.Terms.Skip(1))
            {
                if (IsNegative(i))
                    _int = LinqExpr.Subtract(_int, ForArithmetic(Visit(-i)));
                else
                    _int = LinqExpr.Add(_int, ForArithmetic(Visit(i)));
            }
            return Int(A, _int);
        }

        protected override LinqExpr VisitProduct(Product M)
        {
            List<Expression> terms = M.Terms.Where(i => !i.Equals(-1)).ToList();

            LinqExpr _int = ForArithmetic(Visit(terms.First()));
            foreach (Expression i in terms.Skip(1))
                _int = LinqExpr.Multiply(_int, ForArithmetic(Visit(i)));
            return Int(M, (M.Terms.Count() - terms.Count) % 2 != 0 ? LinqExpr.Negate(_int) : _int);
        }

        protected override LinqExpr VisitUnary(Unary U)
        {
            LinqExpr o = Visit(U.Operand);
            switch (U.Operator)
            {
                case Operator.Negate: return Int(U, LinqExpr.Negate(ForArithmetic(o)));
                case Operator.Inverse: return Int(U, LinqExpr.Divide(LinqExpr.Constant(1.0), ForArithmetic(o)));
                case Operator.Not: return Int(U, LinqExpr.Not(ForLogic(o)));

                default: throw new NotSupportedException("Unsupported unary operator '" + U.Operator.ToString() + "'.");
            }
        }

        protected override LinqExpr VisitBinary(Binary B)
        {
            LinqExpr l = Visit(B.Left);
            LinqExpr r = Visit(B.Right);
            switch (B.Operator)
            {
                case Operator.Add: return Int(B, LinqExpr.Add(ForArithmetic(l), ForArithmetic(r)));
                case Operator.Subtract: return Int(B, LinqExpr.Subtract(ForArithmetic(l), ForArithmetic(r)));
                case Operator.Multiply: return Int(B, LinqExpr.Multiply(ForArithmetic(l), ForArithmetic(r)));
                case Operator.Divide: return Int(B, LinqExpr.Divide(ForArithmetic(l), ForArithmetic(r)));

                case Operator.And: return Int(B, LinqExpr.And(ForLogic(l), ForLogic(r)));
                case Operator.Or: return Int(B, LinqExpr.Or(ForLogic(l), ForLogic(r)));

                case Operator.Equal: return Int(B, LinqExpr.Equal(l, r));
                case Operator.NotEqual: return Int(B, LinqExpr.NotEqual(l, r));
                case Operator.Greater: return Int(B, LinqExpr.GreaterThan(l, r));
                case Operator.GreaterEqual: return Int(B, LinqExpr.GreaterThanOrEqual(l, r));
                case Operator.Less: return Int(B, LinqExpr.LessThan(l, r));
                case Operator.LessEqual: return Int(B, LinqExpr.LessThanOrEqual(l, r));

                default: throw new NotSupportedException("Unsupported binary operator '" + B.Operator.ToString() + "'.");
            }
        }

        protected override LinqExpr VisitPower(Power P)
        {
            // Handle some special cases.
            if (P.Right.Equals(0.5))
                return Visit(Call.Sqrt(P.Left));
            else if (P.Right.Equals(-0.5))
                return Int(P, Reciprocal(Visit(Call.Sqrt(P.Left))));

            LinqExpr l = ForArithmetic(Visit(P.Left));
            // More special cases.
            if (P.Right.Equals(2))
                return Int(P, LinqExpr.Multiply(l, l));
            else if (P.Right.Equals(-1))
                return Int(P, Reciprocal(l));
            else if (P.Right.Equals(-2))
                return Int(P, Reciprocal(LinqExpr.Multiply(l, l)));

            LinqExpr r = ForArithmetic(Visit(P.Right));
            return Int(P, LinqExpr.Call(
                target.Module.GetFunction("Pow", l.Type, r.Type),
                l, r));
        }

        protected override LinqExpr VisitCall(Call C)
        {
            LinqExpr[] args = C.Arguments.Select(i => Visit(i)).ToArray();
            return Int(C, LinqExpr.Call(
                target.Module.Compile(C.Target, args.Select(i => i.Type).ToArray()), 
                args));
        }

        protected override LinqExpr VisitConstant(Constant C) { return LinqExpr.Constant((double)C); }

        protected override LinqExpr VisitVariable(Variable V) { throw new UndefinedVariable("Undefined variable '" + V.Name + "'."); }
        protected override LinqExpr VisitUnknown(Expression E) { throw new NotSupportedException("Unsupported expression type '" + E.GetType().FullName + "'."); }

        private LinqExpr ForArithmetic(LinqExpr x)
        {
            if (x.Type == typeof(double) || x.Type == typeof(float))
                return x;

            return LinqExpr.Convert(x, typeof(double));
        }

        private LinqExpr ForLogic(LinqExpr x)
        {
            return x;
        }

        // Generate an intermediate expression.
        private LinqExpr Int(Expression For, LinqExpr x)
        {
            LinqExpr _int = target.Decl(Scope.Intermediate, x.Type);
            target.Map(Scope.Intermediate, For, _int);
            target.Add(LinqExpr.Assign(_int, x));
            return _int;
        }

        private static bool IsNegative(Expression x)
        {
            Constant C = Product.TermsOf(x).FirstOrDefault(i => i is Constant) as Constant;
            if (C != null)
                return C.Value < 0;
            return false;
        }

        private static LinqExpr Reciprocal(LinqExpr x)
        {
            return LinqExpr.Divide(LinqExpr.Constant(1.0), x);
        }
    }

}