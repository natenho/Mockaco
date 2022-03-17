using System;

namespace Mockaco
{
    internal class Error
    {
        public string Message { get; }

        public Exception Exception { get; }

        public Error(string message)
        {
            Message = message;
        }

        public Error(Exception exception)
        {
            Exception = exception;
        }

        public Error(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}