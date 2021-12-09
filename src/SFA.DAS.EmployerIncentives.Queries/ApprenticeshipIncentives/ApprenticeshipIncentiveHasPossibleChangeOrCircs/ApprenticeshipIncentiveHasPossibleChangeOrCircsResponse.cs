using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.ApprenticeshipIncentiveHasPossibleChangeOrCircs
{
    public class ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse : IResponseLogWriterWithArgs
    {
        public bool HasPossibleChangeOfCircumstances { get; }


        public ApprenticeshipIncentiveHasPossibleChangeOrCircsResponse(bool hasPossibleChangeOfCircumstances)
        {
            HasPossibleChangeOfCircumstances = hasPossibleChangeOfCircumstances;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLogWithArgs Log
        {
            get
            {
                return new ResponseLogWithArgs
                {
                    OnProcessed = () => new Tuple<string, object[]>("Has possible change of circs = {hasPossibleChangeOfCircs}", new object[] { HasPossibleChangeOfCircumstances })
                };
            }
        }
    }
}
