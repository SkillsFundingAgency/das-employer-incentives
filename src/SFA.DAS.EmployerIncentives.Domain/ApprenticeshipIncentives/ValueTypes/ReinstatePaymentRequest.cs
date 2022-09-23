using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class ReinstatePaymentRequest : ServiceRequest
    {
        public string Process { get; }

        public ReinstatePaymentRequest(string taskId, string decisionReference, DateTime created, string process) 
            : base(taskId, decisionReference, created)
        {
            Process = process;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return TaskId;
            yield return DecisionReference;
            yield return Created;
            yield return Process;
        }
    }
}
