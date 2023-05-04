using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages;
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
            paymentApprovalInput.PaymentApprovalOrchestrationId = context.InstanceId;

            var collectionPeriod = new Domain.ValueObjects.CollectionPeriod(paymentApprovalInput.CollectionPeriod.Period, paymentApprovalInput.CollectionPeriod.Year);

            await context.CallSubOrchestratorAsync(nameof(PaymentApprovalSendEmailOrchestrator), paymentApprovalInput);
            
            using var approvalTmeoutCts = new CancellationTokenSource();

            Task<bool> approvalEvent = context.WaitForExternalEvent<bool>($"PaymentsApproved_{paymentApprovalInput.PaymentApprovalOrchestrationId}_{paymentApprovalInput.CorrelationId}");
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

                    await SendSlackMessage(new ApprovalNotificationReceived(
                        collectionPeriod,
                        paymentApprovalInput.EmailAddress),
                        context);

                    return new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.Approved };
                }               
                else // approvalReminderTask
                {
                    await SendSlackMessage(new ApprovalNotificationTimedOut(
                        collectionPeriod,
                        paymentApprovalInput.EmailAddress),
                        context);

                    return new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.NotApprovedInTime }; 
                }
            }
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

                await SendSlackMessage(new ApprovalNotificationNotReceived(
                    collectionPeriod,
                    paymentApprovalInput.EmailAddress),
                    context);

                remindersSent++;
            }
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
