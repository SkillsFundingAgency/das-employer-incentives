using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class PaymentApprovalSendEmailOrchestrator
    {
        private readonly ILogger<PaymentApprovalSendEmailOrchestrator> _logger;
        private readonly IOptions<PaymentProcessSettings> _paymentProcessSettings;

        public PaymentApprovalSendEmailOrchestrator(
            ILogger<PaymentApprovalSendEmailOrchestrator> logger,
            IOptions<PaymentProcessSettings> paymentProcessSettings)
        {
            _paymentProcessSettings = paymentProcessSettings;
            _logger = logger;
        }

        [FunctionName(nameof(PaymentApprovalSendEmailOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var paymentApprovalInput = context.GetInput<PaymentApprovalInput>();
            
            var collectionPeriod = new Domain.ValueObjects.CollectionPeriod(paymentApprovalInput.CollectionPeriod.Period, paymentApprovalInput.CollectionPeriod.Year);

            context.SetCustomStatus("Sending MetricsReport Email");

            await context.CallActivityAsync(
               nameof(SendMetricsReportEmail),
               new SendMetricsReportEmailInput
               {
                   CollectionPeriod = paymentApprovalInput.CollectionPeriod,
                   EmailAddress = paymentApprovalInput.EmailAddress,
                   ApprovalLink = GenerateApprovalUrl(paymentApprovalInput)
               });

            context.SetCustomStatus("MetricsReport Email sent");

            if (paymentApprovalInput.IsResend)
            {

                await SendSlackMessage(new ApprovalEmailResent(
                       collectionPeriod,
                       paymentApprovalInput.EmailAddress),
                       context);
            }
            else
            {
                await SendSlackMessage(new ApprovalEmailSent(
                   collectionPeriod,
                   paymentApprovalInput.EmailAddress),
                   context);
            }
        }


        private string GenerateApprovalUrl(PaymentApprovalInput paymentApprovalInput)
        {
            var host = _paymentProcessSettings.Value.AuthorisationBaseUrl;
            if (!host.EndsWith("/"))
            {
                host = $"{host}/";
            }

            return  $"{host}orchestrators/approvePayments/{paymentApprovalInput.PaymentApprovalOrchestrationId}_{paymentApprovalInput.CorrelationId}";
        }

        private Task SendSlackMessage<T>(
            T notification,
            IDurableOrchestrationContext context) where T : ISlackNotification
        {
            try
            {
                 return context.CallActivityAsync(
                                 nameof(SendSlackNotification),
                                 new SendSlackNotificationInput(notification.Message));
            }
            catch(Exception ex)
            {
                _logger.LogError("Unable to send Slack notification", ex);

                return Task.CompletedTask;
            }
        }
    }
}
