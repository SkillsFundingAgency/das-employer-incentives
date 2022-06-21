using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class InvalidEmploymentCheckErrorTypeException : DomainException
    {
        public InvalidEmploymentCheckErrorTypeException() { }
        public InvalidEmploymentCheckErrorTypeException(string message) : base(message) { }
        public InvalidEmploymentCheckErrorTypeException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidEmploymentCheckErrorTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
