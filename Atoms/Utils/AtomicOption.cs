using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils
{
    /// <summary>
    /// AtomicOption represents an object that may or may not have a value.
    /// As opposed to plain old null, this type makes null checks explicit. If a value exists,
    /// it will be the Defined variant. Otherwise, it will be the Null variant.
    /// </summary>
    public abstract class AtomicOption<V>
    {
        public sealed class Defined : AtomicOption<V>
        {
            public V Value { get; }
            public Defined(V value)
            {
                Value = value;
            }
        }

        public sealed class Empty : AtomicOption<string> { }
    }
}
