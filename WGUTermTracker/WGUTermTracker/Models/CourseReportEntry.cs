using System;

namespace WGUTermTracker.Models;

public class CourseReportEntry
{
    public string TermName { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public string DisplayCourseName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CourseName))
            {
                return "-";
            }

            const int maxLength = 40;
            return CourseName.Length <= maxLength
                ? CourseName
                : string.Concat(CourseName.AsSpan(0, maxLength), "…");
        }
    }

    public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string DisplayTermName => string.IsNullOrWhiteSpace(TermName)
        ? "Unassigned Term"
        : TermName;

    public string SanitizedNotes => string.IsNullOrWhiteSpace(Notes)
        ? "-"
        : Notes
            .Replace("\r\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ");
}
