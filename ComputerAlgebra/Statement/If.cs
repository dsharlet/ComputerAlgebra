using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    /// <summary>
    /// Represents an if(cond) { true } else { false } statement.
    /// </summary>
    public class If : Statement
    {
        private Expression cond;
        /// <summary>
        /// Condition to test.
        /// </summary>
        public Expression Condition { get { return cond; } }

        private Statement _true, _false;
        /// <summary>
        /// Statements for each branch of the condition.
        /// </summary>
        public Statement True { get { return _true; } }
        public Statement False { get { return _false; } }

        private If(Expression Condition, Statement True, Statement False) { cond = Condition;  _true = True; _false = False; }

        /// <summary>
        /// Create a new if statement.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="True"></param>
        /// <param name="False"></param>
        /// <returns></returns>
        public static If New(Expression Condition, Statement True, Statement False) { return new If(Condition, True, False); }
        public static If New(Expression Condition, Statement True) { return new If(Condition, True, null); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            if (cond.Evaluate(State))
                _true.Execute(State);
            else
                _false.Execute(State);
        }
    }
}
