using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Builders;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages
{
    public class ApprovalEmailSent : ISlackNotification
    {
        private readonly CollectionPeriod _collectionPeriod;
        private readonly string _approverEmailAddress;

        public ApprovalEmailSent(
            CollectionPeriod collectionPeriod,
            string approverEmailAddress)
        {
            _collectionPeriod = collectionPeriod;
            _approverEmailAddress = approverEmailAddress;
        }

        public SlackMessage Message => new SlackMessageBuilder()
                .AddSection(
                    new SlackSectionBuilder(block_id: nameof(MetricsReportGenerated))
                        .AddText(new SlackTextBuilder(SlackTextBuilder.Type.Plain, $"Payment approval email sent to {_approverEmailAddress} for R{_collectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}.")
                     ))
                .AddSection(
                    new SlackSectionBuilder()
                        .AddText(new SlackTextBuilder(SlackTextBuilder.Type.Plain, "Thanks")
                     ))
                .AddSection(
                    new SlackSectionBuilder()
                        .AddText(new SlackTextBuilder(SlackTextBuilder.Type.Plain, "Payments BAU")
                     ))
                .Build();
    }
}
