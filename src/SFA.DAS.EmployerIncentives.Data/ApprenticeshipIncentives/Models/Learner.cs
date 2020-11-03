﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("incentives.Learner")]
    [Table("Learner", Schema = "incentives")]
    public partial class Learner
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long Ukprn { get; set; }
        public long ULN { get; set; }
        public bool SubmissionFound { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public bool? LearningFound { get; set; }
        public bool? HasDataLock { get; set; }
        public DateTime? StartDate { get; set; }
        public int? DaysInLearning { get; set; }
        public bool? InLearning { get; set; }
        public string RawJSON { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
