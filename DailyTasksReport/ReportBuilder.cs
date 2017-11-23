using System.Collections.Generic;
using System.Text;
using DailyTasksReport.Tasks;

namespace DailyTasksReport
{
    internal class ReportBuilder
    {
        private readonly ModEntry _parent;

        private readonly List<Task> _tasks = new List<Task>();

        public ReportBuilder(ModEntry parent)
        {
            _parent = parent;

            _tasks.Add(new CropsTask(_parent.Config));
            _tasks.Add(new PetTask(_parent.Config));
            _tasks.Add(new AnimalsTask(_parent.Config));
            _tasks.Add(new FarmCaveTask(_parent.Config));
            _tasks.Add(new ObjectsTask(_parent.Config));
        }

        public string GetReportText()
        {
            var stringBuilder = new StringBuilder();
            var count = 0;

            foreach (var task in _tasks)
            {
                stringBuilder.Append(task.GeneralInfo(out var linesUsed));
                count += linesUsed;
            }

            if (count == 0)
            {
                stringBuilder.Append("All done!");
                return stringBuilder.ToString();
            }

            Task.NextPage(ref stringBuilder, ref count);

            if (!_parent.Config.ShowDetailedInfo)
                return stringBuilder.ToString();

            foreach (var task in _tasks)
                stringBuilder.Append(task.DetailedInfo());

            return stringBuilder.ToString();
        }
    }
}