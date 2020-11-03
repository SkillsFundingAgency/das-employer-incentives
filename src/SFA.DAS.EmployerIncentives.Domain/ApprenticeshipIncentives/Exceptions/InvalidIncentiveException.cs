using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions
{
    [Serializable]
    public class InvalidIncentiveException : DomainException
    {
        public InvalidIncentiveException() { }
        public InvalidIncentiveException(string message) : base(message) { }
        public InvalidIncentiveException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidIncentiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
