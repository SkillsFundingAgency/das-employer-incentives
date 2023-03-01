using System.Dynamic;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Notification.Builders
{
    public class SlackSectionBuilder
    {
        readonly dynamic _section;

        public SlackSectionBuilder(string block_id = "")
        {
            _section = new ExpandoObject();
            _section.type = "section";
            if (!string.IsNullOrEmpty(block_id))
            {
                _section.block_id = block_id;
            }
        }

        public SlackSectionBuilder AddText(SlackTextBuilder textBuilder)
        {
            _section.text = textBuilder.Build();
            return this;
        }

        public dynamic Build()
        {
            return _section;
        }
    }
}
