﻿using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class PendingPaymentValidationResultModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public string Step { get; set; }
        public bool Result { get; set; }
        public AcademicPeriod AcademicPeriod { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
