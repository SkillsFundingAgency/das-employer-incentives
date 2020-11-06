using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class PriceEpisodeDto
    {
        public DateTime StartDate { get; set; }
        public ICollection<PeriodDto> Periods { get; set; }
    }
}
