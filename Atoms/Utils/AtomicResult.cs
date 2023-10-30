using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Utils
{
    /// <summary>
    /// AtomicResult represents the result of an atomic operation.
    /// If the operation was successful, the Ok variant is returned.
    /// Otherwise, the Error variant is returned.
    /// AtomicResult makes error checking explicit, as opposed to traditional exception handling.
    /// </summary>
    public abstract class AtomicResult<E> where E : Exception
    {
        public sealed class Error : AtomicResult<E>
        {
            public E Except { get; }

            public Error(E errorObject)
            {
                Except = errorObject;
            }
        }

        public sealed class Ok : AtomicResult<InvalidOperationException> { }
    }

    /// <summary>
    /// AtomicResult represents the result of an atomic operation.
    /// If the operation was successful, the Ok variant is returned with a value of type V.
    /// Otherwise, the Error variant is returned.
    /// AtomicResult makes error checking explicit, as opposed to traditional exception handling.
    /// </summary>
    public abstract class AtomicResult<V, E> where E : Exception
    {
        public sealed class Ok : AtomicResult<V, E>
        {
            public V Value { get; }
            public Ok(V value)
            {
                Value = value;
            }
        }

        public sealed class Error : AtomicResult<V, E>
        {
            public E Except { get; }
            public Error(E errorObject)
            {
                Except = errorObject;
            }
        }
    }
}
