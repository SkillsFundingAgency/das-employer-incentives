using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions
{
    [Serializable]
    public class MissingPaymentProfileException : DomainException
    {
        public MissingPaymentProfileException() { }
        public MissingPaymentProfileException(string message) : base(message) { }
        public MissingPaymentProfileException(string message, Exception innerException) : base(message, innerException) { }
        protected MissingPaymentProfileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
