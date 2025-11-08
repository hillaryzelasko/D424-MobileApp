using SQLite;
using System;
using static WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Models
{
    [Table("Assessments")]
    public class Assessment
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [Indexed]
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Type { get; set; } = AssessmentType.Objective.ToString();

        public string Status { get; set; } = "Not Started";

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today;

        public bool StartAlertEnabled { get; set; }
            = false;

        public bool EndAlertEnabled { get; set; }
            = false;

        public DateTime DueDate { get; set; } = DateTime.Today;
    }
}
