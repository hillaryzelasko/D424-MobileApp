using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using WGUTermTracker.ViewModels;

namespace WGUTermTracker.Views;

public partial class CourseReportPage : ContentPage
{
    private readonly CourseReportViewModel viewModel;

    public CourseReportPage(CourseReportViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await viewModel.LoadCommand.ExecuteAsync(null);
    }
}
