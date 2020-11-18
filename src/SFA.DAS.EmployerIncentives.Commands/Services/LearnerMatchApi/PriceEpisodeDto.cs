using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class PriceEpisodeDto
    {
        public ICollection<PeriodDto> Periods { get; set; }
    }
}
