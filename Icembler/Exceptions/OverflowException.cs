using System;

namespace Icembler.Exceptions
{
    public class OverflowException : Exception
    {
        public OverflowException()
        {
        }

        public OverflowException(string message)
            : base(message)
        {
        }

        public OverflowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
