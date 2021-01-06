using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class PausePaymentsException : DomainException
    {
        public PausePaymentsException() { }
        public PausePaymentsException(string message) : base(message) { }
        public PausePaymentsException(string message, Exception innerException) : base(message, innerException) { }
        protected PausePaymentsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
