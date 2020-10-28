using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Exception for bad function/variable lookups.
    /// </summary>
    public class UnresolvedName : Exception
    {
        public UnresolvedName(string Name) : base("Unresolved name '" + Name + "'.") { }
    }

    /// <summary>
    /// Contains functions and values.
    /// </summary>
    public abstract class Namespace
    {
        /// <summary>
        /// Look up expressions with the given name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public abstract IEnumerable<Expression> LookupName(string Name);

        /// <summary>
        /// Lookup functions with the given name and matching parameters.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public IEnumerable<Function> LookupFunction(string Name, IEnumerable<Expression> Params) { return LookupName(Name).OfType<Function>().Where(i => i.CanCall(Params)); }
        public IEnumerable<Function> LookupFunction(string Name, params Expression[] Params) { return LookupFunction(Name, Params.AsEnumerable()); }

        /// <summary>
        /// Resolve a name to an expression.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public Expression Resolve(string Name)
        {
            Expression resolved = LookupName(Name).SingleOrDefault();
            if (!(resolved is null))
                return resolved;
            throw new UnresolvedName(Name);
        }
        /// <summary>
        /// Resolve a name with arguments to a function.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public Function Resolve(string Name, IEnumerable<Expression> Params)
        {
            Function resolved = LookupFunction(Name, Params).SingleOrDefault();
            if (!(resolved is null))
                return resolved;
            throw new UnresolvedName(Name);
        }
        public Function Resolve(string Name, params Expression[] Params) { return Resolve(Name, Params.AsEnumerable()); }

        private static GlobalNamespace global = new GlobalNamespace();
        /// <summary>
        /// Get the global namespace.
        /// </summary>
        public static Namespace Global { get { return global; } }
    }
}
