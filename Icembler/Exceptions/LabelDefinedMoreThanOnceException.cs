using System;

namespace Icembler.Exceptions
{
    public class LabelDefinedMoreThanOnceException : Exception
    {
        public LabelDefinedMoreThanOnceException()
        {
        }

        public LabelDefinedMoreThanOnceException(string message)
            : base(message)
        {
        }

        public LabelDefinedMoreThanOnceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
