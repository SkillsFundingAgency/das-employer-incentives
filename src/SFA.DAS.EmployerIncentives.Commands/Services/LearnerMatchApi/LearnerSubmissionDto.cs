using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class LearnerSubmissionDto
    {
        public DateTime StartDate { get; set; }
        public DateTime IlrSubmissionDate { get; set; }
        public int IlrSubmissionWindowPeriod { get; set; }
        public string AcademicYear { get; set; }
        public long Ukprn { get; set; }
        public long Uln { get; set; }
        public ICollection<TrainingDto> Training { get; set; }
    }
}
