using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Numerics;

namespace ComputerAlgebra
{
    /// <summary>
    /// Namespace implementation that performs lookups by reflecting a .Net type or object.
    /// </summary>
    public class ObjectNamespace : Namespace
    {
        private object _this;

        public ObjectNamespace(object This) { _this = This; }

        public override IEnumerable<Expression> LookupName(string Name)
        {
            BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

            Type type = _this.GetType();
            foreach (MethodInfo i in type.GetMethods(binding).Where(i => i.Name == Name))
                yield return NativeFunction.New(_this, i);
            foreach (FieldInfo i in type.GetFields(binding).Where(i => i.Name == Name && i.FieldType.IsAssignableFrom(typeof(Expression))))
                yield return (Expression)i.GetValue(_this);
        }
    }
}
