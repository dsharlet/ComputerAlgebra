using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using LinqExprs = System.Linq.Expressions;
using LinqExpr = System.Linq.Expressions.Expression;
using ParamExpr = System.Linq.Expressions.ParameterExpression;

namespace ComputerAlgebra.LinqCompiler
{
    // Holds an instance of T and a LinqExpression that maps to the instance.
    public class GlobalExpr<T>
    {
        private T x;
        public T Value { get { return x; } set { x = value; } }

        // A Linq Expression to refer to the voltage at this node.
        private LinqExpr expr;
        public LinqExpr Expr { get { return expr; } }

        public static implicit operator LinqExpr(GlobalExpr<T> G) { return G.expr; }

        public GlobalExpr()
        {
            expr = LinqExpr.Field(LinqExpr.Constant(this), typeof(GlobalExpr<T>), "x");
        }
        public GlobalExpr(T Init) : this() { x = Init; }
    }
}
