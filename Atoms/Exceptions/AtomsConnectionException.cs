using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
    public class AtomsConnectionException : Exception
    {
        public AtomsConnectionException(string? message = null) : base(message){ }

        public AtomsConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
