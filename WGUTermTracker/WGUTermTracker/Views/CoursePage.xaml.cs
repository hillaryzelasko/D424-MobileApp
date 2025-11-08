using Microsoft.Maui.Controls;
using WGUTermTracker.Models;
using WGUTermTracker.ViewModels;

namespace WGUTermTracker.Views;

[QueryProperty(nameof(Course), "Course")]
public partial class CoursePage : ContentPage
{
    private readonly CourseDetailViewModel viewModel;

    public CoursePage(CourseDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public Course? Course
    {
        get => viewModel.Course;
        set
        {
            if (value is null)
            {
                return;
            }

            _ = viewModel.InitializeAsync(value);
        }
    }
}
