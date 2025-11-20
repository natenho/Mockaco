using System;

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
    }
}
