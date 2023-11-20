using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Utils
{
    /// <summary>
    /// AtomicOption represents an object returned from an atomic operation that may or may not have a value.
    /// If a value exists, it will be contained within the returned Exists variant.
    /// Otherwise, the Empty variant is returned.
    /// As opposed to plain old null checks, this type makes null checks explicit. 
    /// </summary>
    public abstract class AtomicOption<V>
    {
        public sealed class Exists : AtomicOption<V>
        {
            public V Value { get; }
            public Exists(V value)
            {
                Value = value;
            }
        }

        public sealed class Empty : AtomicOption<V> { }
    }
}
