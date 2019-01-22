using System;

namespace Icembler.Exceptions
{
    public class InvalidOperandException : Exception
    {
        public InvalidOperandException()
        {
        }

        public InvalidOperandException(string message)
            : base(message)
        {
        }

        public InvalidOperandException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
