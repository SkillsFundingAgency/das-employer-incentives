namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CollectionPeriod
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public override string ToString()
        {
            return Year + " - " + Month;
        }
    }
}
