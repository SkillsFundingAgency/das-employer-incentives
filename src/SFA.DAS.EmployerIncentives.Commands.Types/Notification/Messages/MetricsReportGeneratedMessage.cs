using SFA.DAS.EmployerIncentives.Commands.Types.Notification.Builders;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Messages
{
    public class MetricsReportGeneratedMessage : ISlackNotification
    {
        private readonly CollectionPeriod _collectionPeriod;

        public MetricsReportGeneratedMessage(CollectionPeriod collectionPeriod)
        {
            _collectionPeriod = collectionPeriod;
        }

        public SlackMessage Message => new SlackMessageBuilder()
                .AddSection(
                    new SlackSectionBuilder(block_id: nameof(MetricsReportGenerated))
                        .AddText(new SlackTextBuilder(SlackTextBuilder.Type.Plain, $"The metrics report for R{_collectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')} has generated.")
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
