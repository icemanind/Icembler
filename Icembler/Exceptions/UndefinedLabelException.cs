using System;

namespace Icembler.Exceptions
{
    public class UndefinedLabelException : Exception
    {
        public UndefinedLabelException()
        {
        }

        public UndefinedLabelException(string message)
            : base(message)
        {
        }

        public UndefinedLabelException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
