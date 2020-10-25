using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ComputerAlgebra
{
    /// <summary>
    /// Namespace implementation that performs lookups by reflecting a .Net type or object.
    /// </summary>
    public class TypeNamespace : Namespace
    {
        private Type type;

        public TypeNamespace(Type T) { type = T; }

        public override IEnumerable<Expression> LookupName(string Name)
        {
            BindingFlags binding = BindingFlags.Public | BindingFlags.Static;

            foreach (MethodInfo i in type.GetMethods(binding).Where(i => i.Name == Name))
                yield return NativeFunction.New(i);
            foreach (FieldInfo i in type.GetFields(binding).Where(i => i.Name == Name && i.FieldType.IsAssignableFrom(typeof(Expression))))
                yield return (Expression)i.GetValue(null);
        }
    }
}
