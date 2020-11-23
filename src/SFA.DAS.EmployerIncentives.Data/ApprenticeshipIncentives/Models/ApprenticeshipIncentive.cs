using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.ApprenticeshipIncentive")]
    [Table("ApprenticeshipIncentive", Schema = "incentives")]
    public partial class ApprenticeshipIncentive
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long ULN { get; set; }
        public long? UKPRN { get; set; }
        public ApprenticeshipEmployerType EmployerType { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public Guid IncentiveApplicationApprenticeshipId { get; set; }
        public long? AccountLegalEntityId { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public bool RefreshedLearnerForEarnings { get; set; }

        [Dapper.Contrib.Extensions.Write(false)]
        public ICollection<PendingPayment> PendingPayments { get; set; }
        [Dapper.Contrib.Extensions.Write(false)]
        public ICollection<Payment> Payments { get; set; }

        public ApprenticeshipIncentive()
        {
            PendingPayments = new List<PendingPayment>();
            Payments = new List<Payment>();
        }
    }
}
