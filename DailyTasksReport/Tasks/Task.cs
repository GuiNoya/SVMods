using System.Text;

namespace DailyTasksReport.Tasks
{
    public abstract class Task
    {
        internal bool CanDrawBubble = false;

        public abstract string GeneralInfo(out int usedLines);
        public abstract string DetailedInfo();
        public abstract void Clear();

        internal static void NextPage(ref StringBuilder stringBuilder, ref int count)
        {
            var i = 11 - count % 11;
            stringBuilder.Append('^', i);
            count += i;
        }
    }
}