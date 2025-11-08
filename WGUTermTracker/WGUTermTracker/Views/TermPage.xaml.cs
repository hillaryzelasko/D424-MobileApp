using Microsoft.Maui.Controls;
using WGUTermTracker.ViewModels;
using WGUTermTracker.Models;

namespace WGUTermTracker.Views;

[QueryProperty(nameof(Term), "Term")]
public partial class TermPage : ContentPage
{
    private readonly TermDetailViewModel viewModel;

    public TermPage(TermDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    public Term? Term
    {
        get => viewModel.Term;
        set
        {
            if (value is null)
            {
                return;
            }

            _ = viewModel.InitializeAsync(value);
        }
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await viewModel.RefreshAsync();
    }


}