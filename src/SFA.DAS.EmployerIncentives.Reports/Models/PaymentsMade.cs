﻿namespace SFA.DAS.EmployerIncentives.Reports.Models
{
    public class PaymentsMade
    {
        public string Year { get; set; }
        public byte Period { get; set; }
        public int Number { get; set; }
        public double Amount { get; set; }
        public int Order { get; set; }
    }
}
