using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ComputerAlgebra.LinqCompiler
{
    public static class CompileExtension
    {
        // Compile a function into a CodeGen instance.
        private static CodeGen CodeGenFunction(Function f, Module Module)
        {
            CodeGen code = new CodeGen(Module);
            foreach (Variable i in f.Parameters)
                code.Decl(Scope.Parameter, i);
            if (f is StmtFunction)
            {
                StmtFunction sf = f as StmtFunction;
                code.Compile(sf.Body);
            }
            else
            {
                code.Return(code.Compile(f.Call(f.Parameters)));
            }
            return code;
        }

        /// <summary>
        /// Compile function to a delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Delegate Compile(this Function This, Module Module) { return CodeGenFunction(This, Module).Build().Compile(); }
        public static Delegate Compile(this Function This) { return Compile(This, null); }

        /// <summary>
        /// Compile function to a lambda.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Compile<T>(this Function This, Module Module)
        {
            // Validate the function types match.
            MethodInfo invoke = typeof(T).GetMethod("Invoke");
            if (invoke.GetParameters().Count() != This.Parameters.Count())
                throw new InvalidOperationException("Different parameters for Function '" + This.Name + "' and '" + typeof(T).ToString() + "'");

            // Compile the function.
            return CodeGenFunction(This, Module).Build<T>().Compile();
        }

        public static T Compile<T>(this Function This) { return Compile<T>(This, null); }

        /// <summary>
        /// Compile an expression to a delegate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Module"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static Delegate Compile(this Expression This, Module Module, IEnumerable<Expression> Parameters)
        {
            return ExprFunction.New(
                This.Evaluate(Parameters.Select((i, j) => Arrow.New(i, "_" + j.ToString()))),
                Parameters.Select((i, j) => Variable.New("_" + j.ToString()))).Compile(Module);
        }
        public static Delegate Compile(this Expression This, Module Module, params Expression[] Parameters) { return Compile(This, Module, Parameters.AsEnumerable()); }
        public static Delegate Compile(this Expression This, IEnumerable<Expression> Parameters) { return Compile(This, null, Parameters); }
        public static Delegate Compile(this Expression This, params Expression[] Parameters) { return Compile(This, Parameters.AsEnumerable()); }

        /// <summary>
        /// Compile an expression to a delegate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Module"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public static T Compile<T>(this Expression This, Module Module, IEnumerable<Expression> Parameters)
        {
            return ExprFunction.New(
                This.Evaluate(Parameters.Select((i, j) => Arrow.New(i, "_" + j.ToString()))),
                Parameters.Select((i, j) => Variable.New("_" + j.ToString()))).Compile<T>(Module);
        }
        public static T Compile<T>(this Expression This, Module Module, params Expression[] Parameters) { return Compile<T>(This, Module, Parameters.AsEnumerable()); }
        public static T Compile<T>(this Expression This, IEnumerable<Expression> Parameters) { return Compile<T>(This, null, Parameters); }
        public static T Compile<T>(this Expression This, params Expression[] Parameters) { return Compile<T>(This, Parameters.AsEnumerable()); }

    }
}
