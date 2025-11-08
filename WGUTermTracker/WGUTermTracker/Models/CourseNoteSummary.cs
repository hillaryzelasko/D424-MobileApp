namespace WGUTermTracker.Models
{
    public class CourseNoteSummary
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string TermName { get; set; } = string.Empty;

        public string TermDisplayName => string.IsNullOrWhiteSpace(TermName)
            ? "No term assigned"
            : TermName;

        public string DisplayNotes => string.IsNullOrWhiteSpace(Notes)
            ? "No notes added yet."
            : Notes;
    }
}
