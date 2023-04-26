using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class PaymentApprovalsOrchestrator
    {
        private readonly ILogger<PaymentApprovalsOrchestrator> _logger;
        private readonly IOptions<PaymentProcessSettings> _paymentProcessSettings;

        public PaymentApprovalsOrchestrator(
            ILogger<PaymentApprovalsOrchestrator> logger,
            IOptions<PaymentProcessSettings> paymentProcessSettings)
        {
            _logger = logger;
            _paymentProcessSettings = paymentProcessSettings;
        }

        [FunctionName(nameof(PaymentApprovalsOrchestrator))]
        public async Task<PaymentApprovalsOutput> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var paymentApprovalsInput = context.GetInput<PaymentApprovalsInput>();

            var taskList = new List<Task<PaymentApprovalResult>>();
            foreach (var email in _paymentProcessSettings.Value.MetricsReportEmailList.Where(t => !string.IsNullOrEmpty(t)))
            {
                Task<PaymentApprovalResult> task = context.CallSubOrchestratorAsync<PaymentApprovalResult>(nameof(PaymentApprovalOrchestrator), new PaymentApprovalInput(paymentApprovalsInput.CollectionPeriod, email, paymentApprovalsInput.PaymentOrchestrationId));
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);

            var approvalResults = new List<PaymentApprovalResult>();

            foreach (var approvalTask in taskList)
            {
                approvalResults.Add(new PaymentApprovalResult() { EmailAddress = approvalTask.Result.EmailAddress, PaymentApprovalStatus = approvalTask.Result.PaymentApprovalStatus });
            }
            
            return new PaymentApprovalsOutput(approvalResults);
        }
    }
}
