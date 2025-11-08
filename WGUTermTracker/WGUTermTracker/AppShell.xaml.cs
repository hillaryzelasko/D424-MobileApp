using WGUTermTracker.Views;

namespace WGUTermTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(TermPage), typeof(TermPage));
            Routing.RegisterRoute(nameof(CoursePage), typeof(CoursePage));
            Routing.RegisterRoute(nameof(AssessmentDetailPage), typeof(AssessmentDetailPage));
            Routing.RegisterRoute(nameof(CourseNotesPage), typeof(CourseNotesPage));
            Routing.RegisterRoute(nameof(CourseReportPage), typeof(CourseReportPage));
        }
    }
}
