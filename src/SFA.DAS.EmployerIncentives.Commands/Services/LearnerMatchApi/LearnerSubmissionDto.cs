using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerSubmissionDto
    {
        public DateTime StartDate { get; set; }
        public DateTime IlrSubmissionDate { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public int AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public string RawJson { get; set; }
        public LearnerDto Learner { get; set; }

        public ICollection<TrainingDto> Training { get; set; }
    }
}
