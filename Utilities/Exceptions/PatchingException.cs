using System;

namespace Common.Utilities.Exceptions
{
    public class PatchingException : Exception
    {
        public PatchingException()
        {
        }

        public PatchingException(string message) : base(message)
        {
        }

        public PatchingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public PatchingException(string originalMethod, string newMethod, Exception innerException)
            : this($"An error occurred while patching {originalMethod} with {newMethod}. See inner exception for details.", innerException)
        {
            OriginalMethod = originalMethod;
            NewMethod = newMethod;
        }

        public string? OriginalMethod { get; }
        public string? NewMethod { get; }
    }
}