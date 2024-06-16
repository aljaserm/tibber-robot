using System;

namespace Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a concurrency conflict occurs.
    /// </summary>
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException()
        {
        }

        public ConcurrencyConflictException(string message)
            : base(message)
        {
        }

        public ConcurrencyConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}