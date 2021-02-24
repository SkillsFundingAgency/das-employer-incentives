﻿using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models
{
    [Dapper.Contrib.Extensions.Table("archive.PendingPayment")]
    [Table("PendingPayment", Schema = "archive")]
    public partial class ArchivedPendingPayment
    {
        [Dapper.Contrib.Extensions.ExplicitKey]
        [Key]
        public Guid PendingPaymentId { get; set; }
        public long AccountId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime? PaymentMadeDate { get; set; }
        public byte? PeriodNumber { get; set; }
        public short? PaymentYear { get; set; }
        public long AccountLegalEntityId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public EarningType EarningType { get; set; }
        public bool ClawedBack { get; set; }
        public DateTime ArchivedDateUtc { get; set; } = DateTime.UtcNow;
    }
}
