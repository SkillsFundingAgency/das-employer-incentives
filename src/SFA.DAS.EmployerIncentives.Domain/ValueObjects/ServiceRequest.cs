using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class ServiceRequest : ValueObject
    {
        public string TaskId { get; }
        public string DecisionReference { get; }
        public DateTime Created { get; }

        public ServiceRequest(
            string taskId,
            string decisionReference,
            DateTime created)
        {
            TaskId = taskId;
            DecisionReference = decisionReference;
            Created = created;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return TaskId;
            yield return DecisionReference;
            yield return Created;
        }
    }
}
