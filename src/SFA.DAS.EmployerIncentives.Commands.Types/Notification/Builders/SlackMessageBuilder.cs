using System.Collections.Generic;
using System.Text.Json;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Builders
{
    public class SlackMessageBuilder
    {
        readonly dynamic _message;

        public SlackMessageBuilder()
        {
            _message = new
            {
                blocks = new List<dynamic> { }
            };
        }

        public SlackMessageBuilder AddSection(SlackSectionBuilder sectionBuilder)
        {
            _message.blocks.Add(
                sectionBuilder.Build()
               );

            return this;
        }

        public SlackMessage Build()
        {
            return new SlackMessage(
                JsonSerializer.Serialize(
                    _message,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
        }
    }
}
