﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public class PriceEpisodeDto
    {
        public string AcademicYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ICollection<PeriodDto> Periods { get; set; }
    }
}
