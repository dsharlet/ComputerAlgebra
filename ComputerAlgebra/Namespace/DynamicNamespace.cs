using System.Collections.Generic;

namespace ComputerAlgebra
{
    /// <summary>
    /// Contains functions and values.
    /// </summary>
    public class DynamicNamespace : Namespace
    {
        private Dictionary<string, List<Expression>> members = new Dictionary<string, List<Expression>>();

        /// <summary>
        /// Look up expressions with the given name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public override IEnumerable<Expression> LookupName(string Name)
        {
            List<Expression> lookup;
            if (members.TryGetValue(Name, out lookup))
                return lookup;
            return new Expression[] { };
        }

        /// <summary>
        /// Add a member to the namespace.
        /// </summary>
        /// <param name="f"></param>
        public void Add(string Name, Expression x)
        {
            List<Expression> values;
            if (!members.TryGetValue(Name, out values))
            {
                values = new List<Expression>();
                members[Name] = values;
            }
            values.Add(x);
        }
        public void Add(Function f) { Add(f.Name, f); }
    }
}
