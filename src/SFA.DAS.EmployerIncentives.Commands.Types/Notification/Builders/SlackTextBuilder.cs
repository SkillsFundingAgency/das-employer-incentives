using System.Dynamic;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Builders
{
    public class SlackTextBuilder
    {
        public enum Type
        {
            Plain,
            Markdown
        }
        readonly dynamic _textItem = new ExpandoObject();

        public SlackTextBuilder(Type type, string text)
        {
            _textItem.type = type switch
            {
                Type.Plain => "plain_text",
                Type.Markdown => "mrkdwn",
                _ => throw new System.ArgumentException("Invalid type"),
            };

            _textItem.text = text;
        }


        public dynamic Build()
        {
            return _textItem;
        }
    }
}
