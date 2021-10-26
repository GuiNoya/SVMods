using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTasksReport
{
    public class ReportReturnItem
    {
        public string Label { get; set; }
        public string Details { get; set; }
        public Tuple<string, int, int> WarpTo { get; set; }
        public override string ToString()
        {
            return Label + (string.IsNullOrEmpty(Details)?"":  ": " + Details);
        }
    }
}
