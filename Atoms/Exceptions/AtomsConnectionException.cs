using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atoms.Exceptions
{
    /// <summary>
    /// This exception occurs when Atoms fails to connect to the database instance.
    /// </summary>
    public class AtomsConnectionException : AtomsException
    {
        public AtomsConnectionException(string? message = null) : base(message){ }

        public AtomsConnectionException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
