using SQLite;
using System;
using static WGUTermTracker.Models.Enums;


namespace WGUTermTracker.Models
{
    [SQLite.Table("Courses")]
    public class Course
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }

        [Indexed]
        public int TermId { get; set; }
        public string CourseName { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);

        public string Status { get; set; } = "In Progress";

        public string InstructorName { get; set; } = string.Empty;

        public string InstructorPhone { get; set; } = string.Empty;

        public string InstructorEmail { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public bool StartAlertEnabled { get; set; }

        public bool EndAlertEnabled { get; set; }


    }
}
