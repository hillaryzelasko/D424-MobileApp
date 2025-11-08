using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using WGUTermTracker.Data;
using WGUTermTracker.Models;

namespace WGUTermTracker.ViewModels;

public partial class CourseReportViewModel : ObservableObject
{
    private readonly AppDatabase database;

    public CourseReportViewModel(AppDatabase database)
    {
        this.database = database;
    }

    public ObservableCollection<CourseReportEntry> Entries { get; } = new();

    public string Title => "Class Schedule Report";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GeneratedAtDisplay))]
    private DateTime generatedAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool hasEntries;

    public string GeneratedAtDisplay => $"Generated: {GeneratedAt:G}";

    public bool IsEmpty => !HasEntries;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var entries = await database.GetCourseReportEntriesAsync();

        GeneratedAt = DateTime.Now;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Entries.Clear();

            foreach (var entry in entries)
            {
                Entries.Add(entry);
            }

            HasEntries = Entries.Count > 0;
        });
    }
}
