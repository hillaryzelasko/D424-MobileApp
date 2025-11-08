using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WGUTermTracker.Models;
using static WGUTermTracker.Models.Enums;

namespace WGUTermTracker.ViewModels;

public partial class AssessmentEditorViewModel : ObservableObject
{
    private readonly AssessmentType type;
    private readonly Func<AssessmentEditorViewModel, Task> saveAssessmentAsync;
    private readonly Func<AssessmentEditorViewModel, Task>? deleteAssessmentAsync;
    private readonly Func<string?, string> normalizeStatus;

    public AssessmentEditorViewModel(
        AssessmentType type,
        IReadOnlyList<string> statusOptions,
        Func<string?, string> normalizeStatus,
        Func<AssessmentEditorViewModel, Task> saveAssessmentAsync,
        Func<AssessmentEditorViewModel, Task>? deleteAssessmentAsync = null)
    {
        this.type = type;
        StatusOptions = statusOptions;
        this.normalizeStatus = normalizeStatus;
        this.saveAssessmentAsync = saveAssessmentAsync;
        this.deleteAssessmentAsync = deleteAssessmentAsync;
        DisplayName = type == AssessmentType.Objective
            ? "Objective Assessment"
            : "Performance Assessment";

        LoadAssessment(null);
    }

    public Assessment Assessment { get; private set; } = new();

    public IReadOnlyList<string> StatusOptions { get; }

    public string DisplayName { get; }

    public AssessmentType AssessmentType => type;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private DateTime dueDate = DateTime.Today;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today;

    [ObservableProperty]
    private bool startAlertEnabled;

    [ObservableProperty]
    private bool endAlertEnabled;

    [ObservableProperty]
    private string selectedStatus = string.Empty;

    public bool CanDelete => Assessment?.ID > 0;

    public void LoadAssessment(Assessment? assessment)
    {
        Assessment = assessment ?? new Assessment();
        Assessment.Type = type.ToString();

        if (Assessment.StartDate == default)
        {
            Assessment.StartDate = DateTime.Today;
        }

        if (Assessment.EndDate == default)
        {
            Assessment.EndDate = Assessment.DueDate != default
                ? Assessment.DueDate
                : DateTime.Today;
        }

        if (Assessment.DueDate == default)
        {
            Assessment.DueDate = DateTime.Today;
        }

        Title = Assessment.Title ?? string.Empty;
        DueDate = Assessment.DueDate;
        StartDate = Assessment.StartDate;
        EndDate = Assessment.EndDate;
        StartAlertEnabled = Assessment.StartAlertEnabled;
        EndAlertEnabled = Assessment.EndAlertEnabled;
        SelectedStatus = normalizeStatus(Assessment.Status);
        OnPropertyChanged(nameof(CanDelete));
    }

    public void PrepareForSave(int courseId)
    {
        var trimmedTitle = Title?.Trim() ?? string.Empty;
        Title = trimmedTitle;

        var normalizedStatus = normalizeStatus(SelectedStatus);
        SelectedStatus = normalizedStatus;

        Assessment.CourseId = courseId;
        Assessment.Type = type.ToString();
        Assessment.Title = trimmedTitle;
        Assessment.DueDate = DueDate;
        Assessment.StartDate = StartDate;
        Assessment.EndDate = EndDate;
        Assessment.StartAlertEnabled = StartAlertEnabled;
        Assessment.EndAlertEnabled = EndAlertEnabled;
        Assessment.Status = normalizedStatus;
        OnPropertyChanged(nameof(CanDelete));
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        return saveAssessmentAsync?.Invoke(this) ?? Task.CompletedTask;
    }

    [RelayCommand]
    private Task DeleteAsync()
    {
        return deleteAssessmentAsync?.Invoke(this) ?? Task.CompletedTask;
    }
}
