using System;

namespace Icembler.Exceptions
{
    public class VirtualDiskFullException : Exception
    {
        public VirtualDiskFullException()
        {
        }

        public VirtualDiskFullException(string message)
            : base(message)
        {
        }

        public VirtualDiskFullException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
