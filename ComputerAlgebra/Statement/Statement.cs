using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    public abstract class Statement
    {
        public abstract void Execute(Dictionary<Expression, Expression> State);
        public void Execute(IEnumerable<Arrow> State) { Execute(State.ToDictionary(i => i.Left, i => i.Right)); }

        //public static implicit operator Statement(Expression Expr)
        //{

        //}
    }
}
