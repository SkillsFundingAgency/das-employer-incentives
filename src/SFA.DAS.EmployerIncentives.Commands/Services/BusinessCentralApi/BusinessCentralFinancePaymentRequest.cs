using System;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{

    public class PaymentRequestContainer
    {
        public BusinessCentralFinancePaymentRequest[] PaymentRequests { get; set; }
    }

    public class BusinessCentralFinancePaymentRequest
    {
        public string RequestorUniquePaymentIdentifier { get; set; }
        public string Requestor { get; set; }
        public FundingStream FundingStream { get; set; }
        public string DueDate { get; set; }
        public string VendorNo { get; set; }
        public string AccountCode { get; set; }
        public string ActivityCode { get; set; }
        public string AnalysisCode { get; set; }
        public string CostCentreCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public ExternalReference ExternalReference { get; set; }
        public string PaymentLineDescription { get; set; }
        public string Approver { get; set; }
    }
}