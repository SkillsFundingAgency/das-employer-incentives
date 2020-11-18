using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class TrainingDto
    {
        public string Reference { get; set; }

        public ICollection<PriceEpisodeDto> Training { get; set; }
    }
}
