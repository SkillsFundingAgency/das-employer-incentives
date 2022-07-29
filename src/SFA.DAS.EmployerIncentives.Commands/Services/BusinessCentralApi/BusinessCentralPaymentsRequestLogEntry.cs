using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class BusinessCentralPaymentsRequestLogEntry
    {
        public static IList<BusinessCentralPaymentsRequestLogEntry> Create(IEnumerable<BusinessCentralFinancePaymentRequest> payments)
        {
            return payments.Select(payment => new BusinessCentralPaymentsRequestLogEntry
            {
                ActivityCode = payment.ActivityCode,
                AccountCode = payment.AccountCode,
                AnalysisCode = payment.AnalysisCode,
                DueDate = payment.DueDate,
                CostCentreCode = payment.CostCentreCode,
                RequestorUniquePaymentIdentifier = payment.RequestorUniquePaymentIdentifier,
            }).ToList();
        }

        public string RequestorUniquePaymentIdentifier { get; set; }

        public string CostCentreCode { get; set; }

        public string DueDate { get; set; }

        public string AccountCode { get; set; }

        public string ActivityCode { get; set; }
        public string AnalysisCode { get; set; }
    }
}