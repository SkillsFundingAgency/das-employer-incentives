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

            paymentApprovalInput.PaymentApprovalOrchestrationId = context.InstanceId;

            var approvalLink = GenerateApprovalUrl(paymentApprovalInput);

            await context.CallActivityAsync(
                nameof(SendMetricsReportEmail),
                new SendMetricsReportEmailInput
                {
                    CollectionPeriod = paymentApprovalInput.CollectionPeriod,
                    EmailAddress = paymentApprovalInput.EmailAddress,
                    ApprovalLink = approvalLink
                });
            paymentApprovalInput.EmailSent = true;

            PaymentApprovalResult paymentApprovalResult;
            Task<bool> approvalEvent = context.WaitForExternalEvent<bool>($"PaymentsApproved_{paymentApprovalInput.PaymentApprovalOrchestrationId}");

            while (true)
            {
                using var timeoutCts = new CancellationTokenSource();

                DateTime dueTime = context.CurrentUtcDateTime.AddSeconds(_paymentProcessSettings.Value.ApprovalReminderPeriodSecs);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    await SendSlackNotification(new ApprovalNotificationReceived(
                        new Domain.ValueObjects.CollectionPeriod(paymentApprovalInput.CollectionPeriod.Period, paymentApprovalInput.CollectionPeriod.Year),
                        paymentApprovalInput.EmailAddress),
                        context);

                    paymentApprovalResult = new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.Approved };
                    break;
                }
                else
                {
                    if (paymentApprovalInput.RemindersSent >= _paymentProcessSettings.Value.ApprovalReminderRetryAttempts)
                    {
                        paymentApprovalResult = new PaymentApprovalResult() { EmailAddress = paymentApprovalInput.EmailAddress, PaymentApprovalStatus = PaymentApprovalStatus.NotApprovedInTime };
                        break;
                    }

                    await SendSlackNotification(new ApprovalNotificationNotReceived(
                        new Domain.ValueObjects.CollectionPeriod(paymentApprovalInput.CollectionPeriod.Period, paymentApprovalInput.CollectionPeriod.Year),
                        paymentApprovalInput.EmailAddress),
                        context);

                    paymentApprovalInput.RemindersSent++;
                }
            }

            return paymentApprovalResult;
        }

        private string GenerateApprovalUrl(PaymentApprovalInput paymentApprovalInput)
        {
            var host = _paymentProcessSettings.Value.AuthorisationBaseUrl;
            if (!host.EndsWith("/"))
            {
                host = $"{host}/";
            }

            return  $"{host}orchestrators/approvePayments/{paymentApprovalInput.PaymentOrchestrationId}_{paymentApprovalInput.PaymentApprovalOrchestrationId}";
        }

        private Task SendSlackNotification<T>(
            T notification,
            IDurableOrchestrationContext context) where T : ISlackNotification
        {
            return context.CallActivityAsync(
                         nameof(SendSlackNotification),
                         new SendSlackNotificationInput(notification.Message));
        }
    }
}
