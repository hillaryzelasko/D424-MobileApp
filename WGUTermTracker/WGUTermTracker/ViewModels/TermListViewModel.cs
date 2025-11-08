using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using WGUTermTracker.Data;
using WGUTermTracker.Models;
using WGUTermTracker.Views;

namespace WGUTermTracker.ViewModels;

public partial class TermListViewModel : ObservableObject
{
    private readonly AppDatabase database;

    public ObservableCollection<Term> Terms { get; } = new();

    public TermListViewModel(AppDatabase database)
    {
        this.database = database;
    }

    /* LOAD TERMS ORDER BY STARTDATE */
    [RelayCommand]
    private async Task LoadAsync()
    {
        var terms = await database.GetTermAsync();

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Terms.Clear();
            foreach (var term in terms.OrderBy(t => t.StartDate))
            {
                Terms.Add(term);
            }
        });
    }

    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        var entries = await database.GetCourseReportEntriesAsync();

        if (entries.Count == 0)
        {
            await DisplayAlertAsync("Course Report", "Add at least one class to generate a report.");
            return;
        }

        if (Shell.Current is null)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync(nameof(CourseReportPage));
        });
    }

    /* ADD NEW TERM */
    [RelayCommand]
    private Task AddAsync()
    {
        return Shell.Current.GoToAsync(nameof(TermPage), true, new Dictionary<string, object>
        {
            ["Term"] = new Term()
        });
    }

    /* SELECT TERM TO VIEW DETAILS */
    [RelayCommand]
    private Task SelectAsync(Term? term)
    {
        if (term is null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync(nameof(TermPage), true, new Dictionary<string, object>
        {
            ["Term"] = term
        });
    }

    private static Task DisplayAlertAsync(string title, string message)
    {
        if (Shell.Current is null)
        {
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.DisplayAlert(title, message, "OK");
        });
    }

}
