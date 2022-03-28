using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityResponse : IResponseLogWriterWithArgs
    {
        public List<PendingPaymentDto> PendingPayments { get; }

        public GetPendingPaymentsForAccountLegalEntityResponse(List<PendingPaymentDto> pendingPayments)
        {
            PendingPayments = pendingPayments;
        }

        [Newtonsoft.Json.JsonIgnore]
        public ResponseLogWithArgs Log
        {
            get
            {
                return new ResponseLogWithArgs
                {
                    OnProcessed = () => new System.Tuple<string, object[]>("{PendingPayments} pending payments retrieved", new object[] { PendingPayments.Count })
                };
            }
        }
    }
}
