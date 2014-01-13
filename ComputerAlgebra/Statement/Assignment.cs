using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception thrown when the compiler encounters an undefined variable.
    /// </summary>
    public class UndefinedVariable : UnresolvedName
    {
        public UndefinedVariable(string Name) : base(Name) { }
    }
    
    /// <summary>
    /// Assignment statement base.
    /// </summary>
    public abstract class BaseAssignment : Statement
    {
        private Expression assign;
        /// <summary>
        /// Expression to assign to.
        /// </summary>
        public Expression Assign { get { return assign; } }

        protected BaseAssignment(Expression Assign) { assign = Assign; }
    }

    /// <summary>
    /// Represents the assignment of a variable to an expression.
    /// </summary>
    public class Assignment : BaseAssignment
    {
        private Operator op;
        /// <summary>
        /// Operator to use for assignment.
        /// </summary>
        public Operator Operator { get { return op; } }

        private Expression value;
        /// <summary>
        /// Value to assign.
        /// </summary>
        public Expression Value { get { return value; } }

        private Assignment(Expression Assign, Operator Operator, Expression Value) : base(Assign) { op = Operator; value = Value; }

        /// <summary>
        /// Create a new assignment.
        /// </summary>
        /// <param name="Assign"></param>
        /// <param name="Operator"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Assignment New(Expression Assign, Operator Operator, Expression Value) { return new Assignment(Assign, Operator, Value); }
        public static Assignment New(Expression Assign, Expression Value) { return new Assignment(Assign, Operator.Equal, Value); }
        /// <summary>
        /// Create a new assignment from an arrow expression. The left side of the arrow must be a variable.
        /// </summary>
        /// <param name="Assign"></param>
        /// <returns></returns>
        public static Assignment New(Arrow Assign) { return New(Assign.Left, Assign.Right); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            Expression value = Value.Evaluate(State);

            try
            {
                switch (op)
                {
                    case Operator.Add: State[Assign] += value; break;
                    case Operator.Subtract: State[Assign] -= value; break;
                    case Operator.Multiply: State[Assign] *= value; break;
                    case Operator.Divide: State[Assign] /= value; break;
                    case Operator.Power: State[Assign] ^= value; break;
                    case Operator.And: State[Assign] &= value; break;
                    case Operator.Or: State[Assign] |= value; break;
                    case Operator.Equal: State[Assign] = value; break;

                    default: throw new NotImplementedException("Operator not implemented for assignment.");
                }
            }
            catch (KeyNotFoundException) { throw new UndefinedVariable(Assign.ToString()); }
        }
    }

    /// <summary>
    /// Represents incrementing and assigning a variable.
    /// </summary>
    public class Increment : BaseAssignment
    {
        private Increment(Expression Assign) : base(Assign) { }

        public static Increment New(Expression Assign) { return new Increment(Assign); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            State[Assign] += 1;
        }
    }

    /// <summary>
    /// Represents decrementing and assigning a variable.
    /// </summary>
    public class Decrement : BaseAssignment
    {
        private Decrement(Expression Assign) : base(Assign) { }

        public static Decrement New(Expression Assign) { return new Decrement(Assign); }

        public override void Execute(Dictionary<Expression, Expression> State)
        {
            State[Assign] += 1;
        }
    }
}
