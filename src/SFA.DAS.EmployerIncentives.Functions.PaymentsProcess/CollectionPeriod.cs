namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CollectionPeriod
    {
        public short Year { get; set; }
        public short Month { get; set; }

        public override string ToString()
        {
            return Year + " - " + Month;
        }
    }
}
