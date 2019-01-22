using System;

namespace Icembler.Exceptions
{
    public class InvalidRegisterNameException : Exception
    {
        public InvalidRegisterNameException()
        {
        }

        public InvalidRegisterNameException(string message)
            : base(message)
        {
        }

        public InvalidRegisterNameException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
