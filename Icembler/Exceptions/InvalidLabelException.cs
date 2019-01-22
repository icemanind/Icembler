using System;

namespace Icembler.Exceptions
{
    public class InvalidLabelException : Exception
    {
        public InvalidLabelException()
        {
        }

        public InvalidLabelException(string message)
            : base(message)
        {
        }

        public InvalidLabelException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
