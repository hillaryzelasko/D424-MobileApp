using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using WGUTermTracker.Data;
using WGUTermTracker.Models;

namespace WGUTermTracker.ViewModels;

public partial class CourseNotesListViewModel : ObservableObject
{
    private readonly AppDatabase database;
    private bool isLoading;
    private readonly List<CourseNoteSummary> allNotes = new();

    public ObservableCollection<CourseNoteSummary> CourseNotes { get; } = new();

    public CourseNotesListViewModel(AppDatabase database)
    {
        this.database = database;
        CourseNotes.CollectionChanged += OnCourseNotesCollectionChanged;
    }

    public bool HasNotes => CourseNotes.Count > 0;

    public string EmptyMessage => string.IsNullOrWhiteSpace(SearchText)
        ? "No course notes available yet."
        : "No notes match your search.";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (isLoading)
        {
            return;
        }

        try
        {
            isLoading = true;
            IsRefreshing = true;

            var notes = await database.GetAllCourseNotesAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                allNotes.Clear();
                allNotes.AddRange(notes);
                ApplyFilter();
            });
        }
        finally
        {
            IsRefreshing = false;
            isLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchText?.Trim() ?? string.Empty;

        IEnumerable<CourseNoteSummary> filteredNotes = string.IsNullOrEmpty(query)
            ? allNotes
            : allNotes.Where(note => note.Notes.Contains(query, StringComparison.OrdinalIgnoreCase));

        CourseNotes.Clear();
        foreach (var note in filteredNotes)
        {
            CourseNotes.Add(note);
        }
    }

    private void OnCourseNotesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(EmptyMessage));
    }
}
