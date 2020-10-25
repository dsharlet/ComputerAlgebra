using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    public abstract class Statement
    {
        public abstract void Execute(Dictionary<Expression, Expression> State);
        public void Execute(IEnumerable<Arrow> State) { Execute(State.ToDictionary(i => i.Left, i => i.Right)); }

        public static implicit operator Statement(Arrow Assign) { return Assignment.New(Assign); }
    }
}
