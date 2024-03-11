using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Mockaco.Common
{
    internal class InvalidMockException : Exception
    {
        public InvalidMockException() : base() { }

        public InvalidMockException(string message) : base(message)
        {
        }

        public InvalidMockException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidMockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
