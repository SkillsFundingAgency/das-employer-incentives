﻿namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives
{
    public class CollectionPeriod
    {
        public byte CollectionPeriodNumber { get; set; }
        public short CollectionYear { get; set; }
        public bool IsInProgress { get; set; }

        public override string ToString()
        {
            return CollectionYear + " - " + CollectionPeriodNumber;
        }
    }
}
