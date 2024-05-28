using System;

namespace Common.Utilities.Exceptions
{
    public class InstanceNullException : Exception
    {
        public InstanceNullException() { }
        public InstanceNullException(string message) : base(message) { }
        public InstanceNullException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}