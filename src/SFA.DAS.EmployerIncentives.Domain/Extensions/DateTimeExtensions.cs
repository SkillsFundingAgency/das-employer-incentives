using System;

namespace SFA.DAS.EmployerIncentives.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        public static int AgeOnThisDay(this DateTime dob, DateTime day)
        {
            var age = day.Year - dob.Year;
            if (day.Month < dob.Month) 
            {
                return --age;
            }

            if (day.Month == dob.Month && day.Day < dob.Day)
            {
                return --age;
            }
            return age;
        }
    }
}
