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
    /// Visitor to generate LINQ expressions for Statement objects.
    /// </summary>
    class CompileStatement : StatementVisitor<object>
    {
        private class Loop
        {
            private LinqExpr _break, _continue;
            public LinqExpr Break { get { return _break; } }
            public LinqExpr Continue { get { return _continue; } }

            public Loop(LinqExpr Break, LinqExpr Continue) { _break = Break; _continue = Continue; }
        }
        private Stack<Loop> loops = new Stack<Loop>();

        private CodeGen target;
        
        public CompileStatement(CodeGen Target) { target = Target; }

        protected override object VisitAssignment(Assignment A)
        {
            LinqExpr value = target.Compile(A.Value);
            switch (A.Operator)
            {
                case Operator.Add: target.Add(LinqExpr.AddAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Subtract: target.Add(LinqExpr.SubtractAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Multiply: target.Add(LinqExpr.MultiplyAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Divide: target.Add(LinqExpr.DivideAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Power: target.Add(LinqExpr.PowerAssign(target.LookUp(A.Assign), value)); break;
                case Operator.And: target.Add(LinqExpr.AndAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Or: target.Add(LinqExpr.OrAssign(target.LookUp(A.Assign), value)); break;
                case Operator.Equal:
                    LinqExpr x = target.LookUp(A.Assign);
                    if (x == null)
                        x = target.DeclInit(A.Assign, A.Value);
                    target.Add(LinqExpr.Assign(x, value));
                    break;

                default: throw new NotImplementedException("Operator not implemented for assignment.");
            }
            return null;
        }

        protected override object VisitBlock(Block B)
        {
            foreach (Statement i in B.Statements)
                Visit(i);
            return null;
        }

        protected override object VisitFor(For F)
        {
            target.For(
                () => Visit(F.Init),
                target.Compile(F.Condition),
                () => Visit(F.Next),
                (Break, Continue) => 
            {
                loops.Push(new Loop(Break, Continue)); 
                Visit(F.Body);
                loops.Pop();
            });
            return null;
        }

        protected override object VisitIf(If I)
        {
            target.If(
                target.Compile(I.Condition), 
                () => Visit(I.True), 
                () => Visit(I.False));
            return null;
        }

        protected override object VisitWhile(While W)
        {
            target.While(
                target.Compile(W.Condition),
                (Break, Continue) =>
            {
                loops.Push(new Loop(Break, Continue));
                Visit(W.Body);
                loops.Pop();
            });
            return null;
        }

        protected override object VisitDoWhile(DoWhile W)
        {
            target.DoWhile((Break, Continue) =>
            {
                loops.Push(new Loop(Break, Continue));
                Visit(W.Body);
                loops.Pop();
            }, target.Compile(W.Condition));
            return null;
        }
        
        protected override object VisitIncrement(Increment A)
        {
            target.Add(LinqExpr.PreDecrementAssign(target.LookUp(A.Assign)));
            return null;
        }

        protected override object VisitReturn(Return R)
        {
            target.Return(target.Compile(R.Value));
            return null;
        }

        protected override object VisitBreak(Break B)
        {
            if (loops.Empty())
                throw new InvalidOperationException("Break outside of loop.");
            target.Add(loops.Peek().Break);
            return null;
        }

        protected override object VisitContinue(Continue C)
        {
            if (loops.Empty())
                throw new InvalidOperationException("Continue outside of loop.");
            target.Add(loops.Peek().Continue);
            return null;
        }

        protected override object VisitUnknown(Statement S)
        {
            throw new NotImplementedException("Unsupported statement type");
        }
    }
}