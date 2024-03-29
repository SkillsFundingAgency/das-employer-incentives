﻿namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CollectionPeriod
    {
        public short Year { get; set; }
        public byte Period { get; set; }
        public bool IsInProgress { get; set; }

        public override string ToString()
        {
            return Year + " - " + Period;
        }
    }
}
