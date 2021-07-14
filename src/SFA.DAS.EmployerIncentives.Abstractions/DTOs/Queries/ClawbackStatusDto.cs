using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class ClawbackStatusDto
    {
        public decimal ClawbackAmount { get; set; }
        public DateTime ClawbackDate { get; set; }
    }
}
