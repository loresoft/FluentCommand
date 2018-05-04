using System;

namespace FluentCommand.Batch.Validation
{
    public class DuplicateException : Exception
    {
        public DuplicateException(string message)
            : base(message)
        {
        }

        public DuplicateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
