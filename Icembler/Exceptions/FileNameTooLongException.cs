using System;

namespace Icembler.Exceptions
{
    public class FileNameTooLongException : Exception
    {
        public FileNameTooLongException()
        {
        }

        public FileNameTooLongException(string message)
            : base(message)
        {
        }

        public FileNameTooLongException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
