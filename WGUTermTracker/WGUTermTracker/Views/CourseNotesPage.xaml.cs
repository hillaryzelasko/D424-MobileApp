using Microsoft.Maui.Controls;
using WGUTermTracker.ViewModels;

namespace WGUTermTracker.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
public partial class CourseNotesPage : ContentPage
{
    private CourseDetailViewModel? viewModel;

    public CourseNotesPage()
    {
        InitializeComponent();
    }

    public CourseDetailViewModel? ViewModel
    {
        get => viewModel;
        set
        {
            if (value is null)
            {
                return;
            }

            viewModel = value;
            BindingContext = value;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel?.RefreshNotesDisplay();
    }
}
