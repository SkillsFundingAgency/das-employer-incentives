using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public interface IEarningsCalculationService
    {
        List<EarningCalculation> GenerateEarningsForApprenticeship(Apprenticeship apprenticeshipIncentive, IncentiveType incentiveType);
    }
}
