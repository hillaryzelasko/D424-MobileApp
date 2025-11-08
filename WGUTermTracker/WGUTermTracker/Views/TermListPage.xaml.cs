using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using WGUTermTracker.Models;
using WGUTermTracker.ViewModels;

namespace WGUTermTracker.Views;

public partial class TermListPage : ContentPage
{
    private readonly TermListViewModel viewModel;
    public TermListPage(TermListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Term term)
        {
            return;
        }

        await viewModel.SelectCommand.ExecuteAsync(term);
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }
}