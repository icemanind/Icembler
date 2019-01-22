using System;

namespace Icembler.Exceptions
{
    public class LabelRequiredException : Exception
    {
        public LabelRequiredException()
        {
        }

        public LabelRequiredException(string message)
            : base(message)
        {
        }

        public LabelRequiredException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
