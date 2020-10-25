using System.Collections.Generic;
using System.Linq;

namespace ComputerAlgebra
{
    /// <summary>
    /// Namespace implementation composing a set of other namespaces.
    /// </summary>
    public class NamespaceSet : Namespace
    {
        private List<Namespace> set;

        public NamespaceSet(IEnumerable<Namespace> Set) { set = Set.ToList(); }
        public NamespaceSet(params Namespace[] Set) : this(Set.AsEnumerable()) { }

        public override IEnumerable<Expression> LookupName(string Name) { return set.SelectMany(i => i.LookupName(Name)); }
    }
}
