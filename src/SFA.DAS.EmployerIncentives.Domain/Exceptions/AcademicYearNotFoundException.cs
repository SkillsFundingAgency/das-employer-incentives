using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Runtime.Serialization;

namespace SFA.DAS.EmployerIncentives.Domain.Exceptions
{
    [Serializable]
    public class AcademicYearNotFoundException : DomainException
    {
        public AcademicYearNotFoundException() { }
        public AcademicYearNotFoundException(string message) : base(message) { }
        public AcademicYearNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        protected AcademicYearNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
