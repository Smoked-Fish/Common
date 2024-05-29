using System;

namespace Common.Utilities.Exceptions
{
    public class PatchingException : Exception
    {
        public PatchingException() { }

        public PatchingException(string message) : base(message) { }

        private PatchingException(string message, Exception innerException) : base(message, innerException) { }

        public PatchingException(string originalMethod, string newMethod, Exception innerException)
            : this($"An error occurred while patching {originalMethod} with {newMethod}. See inner exception for details.", innerException) { }
    }
}