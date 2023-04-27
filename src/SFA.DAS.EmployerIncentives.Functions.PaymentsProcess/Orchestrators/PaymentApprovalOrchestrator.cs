using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class PaymentApprovalOrchestrator
    {
        private readonly ILogger<PaymentApprovalOrchestrator> _logger;
        private readonly IOptions<PaymentProcessSettings> _paymentProcessSettings;        

        public PaymentApprovalOrchestrator(
            ILogger<PaymentApprovalOrchestrator> logger,
            IOptions<PaymentProcessSettings> paymentProcessSettings)
        {
            _paymentProcessSettings = paymentProcessSettings;
            _logger = logger;
        }

        [FunctionName(nameof(PaymentApprovalOrchestrator))]
        public async Task<PaymentApprovalResult> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var paymentApprovalInput = context.GetInput<PaymentApprovalInput>();
     
            var collectionPeriod = new Domain.ValueObjects.CollectionPeriod(paymentApprovalInput.CollectionPeriod.Period, paymentApprovalInput.CollectionPeriod.Year);

             await context.CallActivityAsync(
                nameof(SendMetricsReportEmail),
                new SendMetricsReportEmailInput
                {
                    CollectionPeriod = paymentApprovalInput.CollectionPeriod,
                    EmailAddress = paymentApprovalInput.EmailAddress,
                    ApprovalLink = GenerateApprovalUrl(paymentApprovalInput, context)
                });

            using var approvalTmeoutCts = new CancellationTokenSource();

            PaymentApprovalResult paymentApprovalResult;
            Task<bool> approvalEvent = context.WaitForExternalEvent<bool>($"PaymentsApproved_{context.InstanceId}");
            Task approvalReminderTask = ApprovalReminderTask(
                context, 
                _paymentProcessSettings.Value, 
                paymentApprovalInput, 
                collectionPeriod,
                approvalTmeoutCts.Token);

            while (true)
            {
                if (approvalEvent == await Task.WhenAny(approvalEvent, approvalReminderTask))
                {
                    approvalTmeoutCts.Cancel();

                    await SendSlackNotification(new ApprovalNotificationReceived(
                        collectionPeriod,
                        paymentApprovalInput.EmailAddress),
                        context);

                    paymentApprovalResult = new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.Approved };
                    break;
                }
                else
                {
                    await SendSlackNotification(new ApprovalNotificationTimedOut(
                        collectionPeriod,
                        paymentApprovalInput.EmailAddress),
                        context);

                    return new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.NotApprovedInTime }; 
                }
            }

            return paymentApprovalResult;
        }

        private async Task ApprovalReminderTask(
            IDurableOrchestrationContext context,
            PaymentProcessSettings paymentProcessSettings,
            PaymentApprovalInput paymentApprovalInput,
            Domain.ValueObjects.CollectionPeriod collectionPeriod,
            CancellationToken cancellationToken)
        {
            int remindersSent = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                using var timeoutCts = new CancellationTokenSource();

                DateTime dueTime = context.CurrentUtcDateTime.AddSeconds(paymentProcessSettings.ApprovalReminderPeriodSecs);
                await context.CreateTimer(dueTime, timeoutCts.Token);

                cancellationToken.ThrowIfCancellationRequested();

                if (remindersSent >= paymentProcessSettings.ApprovalReminderRetryAttempts)
                {
                    break;
                }

                await SendSlackNotification(new ApprovalNotificationNotReceived(
                    collectionPeriod,
                    paymentApprovalInput.EmailAddress),
                    context);

                remindersSent++;
            }
        }

        private string GenerateApprovalUrl(
            PaymentApprovalInput paymentApprovalInput,
            IDurableOrchestrationContext context)
        {
            var host = _paymentProcessSettings.Value.AuthorisationBaseUrl;
            if (!host.EndsWith("/"))
            {
                host = $"{host}/";
            }

            return  $"{host}orchestrators/approvePayments/{paymentApprovalInput.PaymentOrchestrationId}_{context.InstanceId}";
        }

        private Task SendSlackNotification<T>(
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
