using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ComputerAlgebra.LinqCompiler
{
    public enum Scope
    {
        Global,
        Local,
        Parameter,
        Intermediate,
    }

    /// <summary>
    /// Represents the global context of code generation.
    /// </summary>
    public class Module : DeclContext
    {
        private Dictionary<Function, MethodInfo> functions = new Dictionary<Function, MethodInfo>();
        private IEnumerable<Type> libraries;

        public Module() : this(null) { }

        public Module(IEnumerable<Type> Libraries)
        {
            if (Libraries == null)
                libraries = new Type[] { typeof(StandardMath) };
            else
                libraries = Libraries.ToArray();
        }

        /// <summary>
        /// Declare a new global in this module mapped to expression Map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Map"></param>
        /// <returns></returns>
        public GlobalExpr<T> Declare<T>(Expression Map)
        {
            GlobalExpr<T> decl = new GlobalExpr<T>();
            base.Declare(Map, decl);
            return decl;
        }
        public GlobalExpr<T> Declare<T>(Expression Map, T Init)
        {
            GlobalExpr<T> decl = new GlobalExpr<T>(Init);
            base.Declare(Map, decl);
            return decl;
        }

        private MethodInfo LookupFunction(string Name, params Type[] ArgTypes)
        {
            MethodInfo method = null;

            // Search the libraries for a matching function.
            foreach (Type i in libraries)
            {
                // If the method is not found, check the base type.
                for (Type t = i; t != null; t = t.BaseType)
                {
                    MethodInfo m = t.GetMethod(Name, BindingFlags.Static | BindingFlags.Public, null, ArgTypes, null);
                    if (m != null)
                    {
                        // If we already found a method, throw ambiguous.
                        if (method != null)
                            throw new AmbiguousMatchException(Name);
                        method = m;
                        break;
                    }
                }
            }

            return method;
        }

        /// <summary>
        /// Get a function from the type libraries provided to the module.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="ArgTypes"></param>
        /// <returns></returns>
        public MethodInfo GetFunction(string Name, params Type[] ArgTypes)
        {
            MethodInfo method = LookupFunction(Name, ArgTypes);
            if (method != null)
                return method;
            throw new NotImplementedException("Function '" + Name + "' not found.");
        }

        /// <summary>
        /// Compile the function into this module.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="ArgTypes"></param>
        /// <returns></returns>
        public MethodInfo Compile(Function f, params Type[] ArgTypes)
        {
            MethodInfo method = null;

            // Search the libraries for a matching function.
            method = LookupFunction(f.Name, ArgTypes);
            if (method != null)
                return method;

            // Try getting the method from the already compiled functions.
            if (functions.TryGetValue(f, out method))
                return method;

            // Try compiling f to a new method.
            Delegate d = f.Compile(this);
            functions[f] = d.Method;
            return d.Method;
        }

        public MethodInfo Compile(Function f) { return Compile(f, f.Parameters.Select(i => typeof(double)).ToArray()); }
    }
}
