using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerAlgebra
{
    /// <summary>
    /// Visits a statement.
    /// </summary>
    /// <typeparam name="T">Result type of the visitor.</typeparam>
    public abstract class StatementVisitor<T>
    {
        protected abstract T VisitUnknown(Statement S);

        protected virtual T VisitAssignment(Assignment A) { return VisitUnknown(A); }
        protected virtual T VisitIncrement(Increment A) { return VisitUnknown(A); }
        protected virtual T VisitDecrement(Decrement D) { return VisitUnknown(D); }
        protected virtual T VisitBlock(Block B) { return VisitUnknown(B); }
        protected virtual T VisitFor(For F) { return VisitUnknown(F); }
        protected virtual T VisitIf(If I) { return VisitUnknown(I); }
        protected virtual T VisitWhile(While W) { return VisitUnknown(W); }
        protected virtual T VisitDoWhile(DoWhile W) { return VisitUnknown(W); }
        protected virtual T VisitReturn(Return R) { return VisitUnknown(R); }
        protected virtual T VisitBreak(Break B) { return VisitUnknown(B); }
        protected virtual T VisitContinue(Continue C) { return VisitUnknown(C); }

        public virtual T Visit(Statement S)
        {
            if (S is Assignment) return VisitAssignment(S as Assignment);
            if (S is Increment) return VisitIncrement(S as Increment);
            if (S is Decrement) return VisitDecrement(S as Decrement);
            if (S is Block) return VisitBlock(S as Block);
            if (S is For) return VisitFor(S as For);
            if (S is If) return VisitIf(S as If);
            if (S is While) return VisitWhile(S as While);
            if (S is DoWhile) return VisitDoWhile(S as DoWhile);
            if (S is Return) return VisitReturn(S as Return);
            if (S is Break) return VisitBreak(S as Break);
            if (S is Continue) return VisitContinue(S as Continue);
            return VisitUnknown(S);
        }
    }
}
