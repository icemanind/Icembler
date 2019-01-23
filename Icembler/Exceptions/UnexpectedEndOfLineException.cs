using System;

namespace Icembler.Exceptions
{
    public class UnexpectedEndOfLineException : Exception
    {
        public UnexpectedEndOfLineException()
        {
        }

        public UnexpectedEndOfLineException(string message)
            : base(message)
        {
        }

        public UnexpectedEndOfLineException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
