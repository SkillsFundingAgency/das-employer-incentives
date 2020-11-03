using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions
{
    [Serializable]
    public class InvalidPendingPaymentException : DomainException
    {
        public InvalidPendingPaymentException() { }
        public InvalidPendingPaymentException(string message) : base(message) { }
        public InvalidPendingPaymentException(string message, Exception innerException) : base(message, innerException) { }
        protected InvalidPendingPaymentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
