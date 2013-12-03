using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Function yet to be resolved.
    /// </summary>
    public class UnknownFunction : Function
    {
        private IEnumerable<Variable> parameters;
        public override IEnumerable<Variable> Parameters { get { return parameters; } }

        private UnknownFunction(string Name, IEnumerable<Variable> Params) : base(Name) { parameters = Params; }

        public static UnknownFunction New(string Name, IEnumerable<Variable> Params) { return new UnknownFunction(Name, Params.Buffer()); }
        public static UnknownFunction New(string Name, params Variable[] Params) { return New(Name, Params.AsEnumerable()); }

        public override Expression Call(IEnumerable<Expression> Args) { throw new UnresolvedName("Cannot call undefined function '" + Name + "'."); }
        
        public override bool CanCall() { return false; }
    }
}
