using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using WGUTermTracker.Data;
using WGUTermTracker.ViewModels;
using WGUTermTracker.Views;

namespace WGUTermTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

           /* BLDG PAGES AND VIEWS */
            builder.Services.AddSingleton<TermListViewModel>();
            builder.Services.AddSingleton<CourseNotesListViewModel>();
            builder.Services.AddTransient<CourseReportViewModel>();
            builder.Services.AddTransient<TermDetailViewModel>();
            builder.Services.AddTransient<CourseDetailViewModel>();
            builder.Services.AddTransient<AssessmentEditorViewModel>();

            builder.Services.AddSingleton<TermListPage>();
            builder.Services.AddSingleton<CourseNotesListPage>();
            builder.Services.AddTransient<CourseReportPage>();
            builder.Services.AddTransient<TermPage>();
            builder.Services.AddTransient<CoursePage>();
            builder.Services.AddSingleton<AppDatabase>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
