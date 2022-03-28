using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetClawbackLegalEntities
{
    public class GetClawbackLegalEntitiesResponse : IResponseLogWriterWithArgs
    {
        public List<ClawbackLegalEntityDto> ClawbackLegalEntities { get; }

        public GetClawbackLegalEntitiesResponse(List<ClawbackLegalEntityDto> legalEntities)
        {
            ClawbackLegalEntities = legalEntities;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLogWithArgs Log
        {
            get
            {
                return new ResponseLogWithArgs
                {
                    OnProcessed = () => new Tuple<string, object[]>("{unsentClawbacksCount} legal entities returned", new object[] { ClawbackLegalEntities.Count })            
                };
            }
        }
    }
}
