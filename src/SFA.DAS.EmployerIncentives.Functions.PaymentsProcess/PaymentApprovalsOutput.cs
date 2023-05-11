using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PaymentApprovalsOutput
    {
        public PaymentApprovalsOutput(List<PaymentApprovalResult> paymentApprovalResults)
        {
            PaymentApprovalResults = paymentApprovalResults;
        }

        public List<PaymentApprovalResult> PaymentApprovalResults { get; }
    }

    public class PaymentApprovalResult
    {
        public string EmailAddress { get; set; }
        public PaymentApprovalStatus PaymentApprovalStatus { get; set; }

        public PaymentApprovalResult()
        {
            PaymentApprovalStatus = PaymentApprovalStatus.Waiting;
        }
    }

    public enum PaymentApprovalStatus
    {
        Waiting,
        Approved,
        NotApprovedInTime,
    }
}
