using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class InvalidMethodCallException : Exception
    {
        public InvalidMethodCallException() { }
        public InvalidMethodCallException(string message) : base(message) { }
        public InvalidMethodCallException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidMethodCallException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
