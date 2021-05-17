using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class UlnAlreadySubmittedException : DomainException
    {
        public UlnAlreadySubmittedException() { }
        public UlnAlreadySubmittedException(string message) : base(message) { }
        public UlnAlreadySubmittedException(string message, Exception innerException) : base(message, innerException) { }
        protected UlnAlreadySubmittedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
