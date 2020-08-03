using System;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs
{
    public class ApprenticeshipIncentiveAmountDto
    {
        public Guid Id { get; set; }
        public int ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public double TotalIncentiveAmount { get; set; }
    }
}
