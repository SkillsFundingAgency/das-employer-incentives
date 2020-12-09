using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions
{
    [Serializable]
    public class DeleteIncentiveException : DomainException
    {
        public DeleteIncentiveException() { }
        public DeleteIncentiveException(string message) : base(message) { }
        public DeleteIncentiveException(string message, Exception innerException) : base(message, innerException) { }
        protected DeleteIncentiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
