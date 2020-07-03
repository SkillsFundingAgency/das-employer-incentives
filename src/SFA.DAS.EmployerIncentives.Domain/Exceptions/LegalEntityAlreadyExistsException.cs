using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class LegalEntityAlreadyExistsException : DomainException
    {
        public LegalEntityAlreadyExistsException() { }
        public LegalEntityAlreadyExistsException(string message) : base(message) { }
        public LegalEntityAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
        protected LegalEntityAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
