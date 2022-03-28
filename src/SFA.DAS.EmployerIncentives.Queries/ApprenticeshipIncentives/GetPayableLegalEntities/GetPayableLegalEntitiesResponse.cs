using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities
{
    public class GetPayableLegalEntitiesResponse : IResponseLogWriterWithArgs
    {
        public List<PayableLegalEntityDto> PayableLegalEntities { get; }

        public GetPayableLegalEntitiesResponse(List<PayableLegalEntityDto> legalEntities)
        {
            PayableLegalEntities = legalEntities;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLogWithArgs Log
        {
            get
            {
                return new ResponseLogWithArgs
                {
                    OnProcessed = () => new System.Tuple<string, object[]>("{payableLegalEntitiesCount} payable legal entities returned", new object[] { PayableLegalEntities.Count })
                };
            }
        }
    }
}
