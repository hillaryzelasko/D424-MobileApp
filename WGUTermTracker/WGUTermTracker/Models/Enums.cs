using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WGUTermTracker.Models
{
    public class Enums
    {
        public enum CourseStatus { InProgress, Completed, Dropped, PlanToTake }
        public enum AssessmentType { Objective, Performance }
        public enum AssessmentStatus { NotStarted, Scheduled, Submitted, Passed, Failed }
    }
}
