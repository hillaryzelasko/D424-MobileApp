using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WGUTermTracker.Data;
using WGUTermTracker.Models;
using WGUTermTracker.Views;
using WGUTermTracker.Validation;
using static WGUTermTracker.Models.Enums;

namespace WGUTermTracker.ViewModels;

public partial class CourseDetailViewModel : ObservableObject
{
    private readonly AppDatabase database;

    [ObservableProperty]
    private Course course = new();

    /* COURSE STATUS SELECTION */
    private static readonly IReadOnlyDictionary<CourseStatus, string> StatusDisplayNames =
    new Dictionary<CourseStatus, string>
    {
        [CourseStatus.InProgress] = "In Progress",
        [CourseStatus.Completed] = "Completed",
        [CourseStatus.Dropped] = "Dropped",
        [CourseStatus.PlanToTake] = "Plan to Take",
    };

    private static readonly IReadOnlyDictionary<string, CourseStatus> StatusLookup =
        CreateStatusLookup();

    public IReadOnlyList<string> StatusOptions { get; } = StatusDisplayNames
        .Select(kvp => kvp.Value)
        .ToArray();

    [ObservableProperty]
    private string? selectedStatus;

    private static readonly IReadOnlyDictionary<AssessmentStatus, string> AssessmentStatusDisplayNames =
    new Dictionary<AssessmentStatus, string>
    {
        [AssessmentStatus.NotStarted] = "Not Started",
        [AssessmentStatus.Scheduled] = "Scheduled",
        [AssessmentStatus.Submitted] = "Submitted",
        [AssessmentStatus.Passed] = "Passed",
        [AssessmentStatus.Failed] = "Failed",
    };

    private static readonly IReadOnlyDictionary<string, AssessmentStatus> AssessmentStatusLookup =
        CreateAssessmentStatusLookup();

    public IReadOnlyList<string> AssessmentStatusOptions { get; } = AssessmentStatusDisplayNames
        .Select(kvp => kvp.Value)
        .ToArray();

    public AssessmentEditorViewModel ObjectiveAssessmentEditor { get; }
    public AssessmentEditorViewModel PerformanceAssessmentEditor { get; }

    [ObservableProperty]
    private string courseNotes = string.Empty;

    private const int StartNotificationOffset = 1;
    private const int EndNotificationOffset = 2;
    private const int AssessmentStartNotificationOffset = 1;
    private const int AssessmentEndNotificationOffset = 2;

    public CourseDetailViewModel(AppDatabase database)
    {
        this.database = database;
        ObjectiveAssessmentEditor = new AssessmentEditorViewModel(
            AssessmentType.Objective,
            AssessmentStatusOptions,
            NormalizeAssessmentStatus,
            SaveAssessmentFromEditorAsync,
            DeleteAssessmentFromEditorAsync);

        PerformanceAssessmentEditor = new AssessmentEditorViewModel(
            AssessmentType.Performance,
            AssessmentStatusOptions,
            NormalizeAssessmentStatus,
            SaveAssessmentFromEditorAsync,
            DeleteAssessmentFromEditorAsync);
    }

    public async Task InitializeAsync(Course course)
    {
        Course = course;
        EnsureDefaults();
        await LoadAssessmentsAsync();
    }

    /* SAVE COURSE TO DB */

    private const string ObjectiveAssessmentDefaultTitle = "Objective Assessment";
    private const string PerformanceAssessmentDefaultTitle = "Performance Assessment";

    [RelayCommand]
    private async Task SaveCourseAsync()
    {
        EnsureDefaults();

        if (Course.ID == 0)
        {
            EnsureDefaultAssessmentTitle(ObjectiveAssessmentEditor, ObjectiveAssessmentDefaultTitle);
            EnsureDefaultAssessmentTitle(PerformanceAssessmentEditor, PerformanceAssessmentDefaultTitle);
        }

        if (Course.TermId == 0)
        {
            await DisplayAlert("Term Required", "Please save the term before adding courses.");
            return;
        }

        if (!CourseValidator.TryValidate(Course, SelectedStatus, out var errorMessage))
        {
            await DisplayAlert("Course Validation", errorMessage);
            return;
        }

        Course.CourseName = Course.CourseName.Trim();
        Course.Status = SelectedStatus;
        Course.InstructorName = Course.InstructorName.Trim();
        Course.InstructorPhone = Course.InstructorPhone.Trim();
        Course.InstructorEmail = Course.InstructorEmail.Trim();
        CourseNotes = CourseNotes?.Trim() ?? string.Empty;


        if (!await ValidateAssessmentBeforeSaveAsync(ObjectiveAssessmentEditor)
            || !await ValidateAssessmentBeforeSaveAsync(PerformanceAssessmentEditor))
        {
            return;
        }

        await database.SaveCourseAsync(Course);
        await SaveAssessmentsAsync();
        await UpdateNotificationsAsync();
        RefreshNotesDisplay();
        await Shell.Current.GoToAsync("..");
    }

    /* COURSE ADD CANCEL */
    [RelayCommand]
    private Task CancelAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    /* MSG BOX POP UP FOR ERRORS */
    private static Task DisplayAlert(string title, string message)
    {
        return Shell.Current?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;
    }

    /* COURSE DELETE */
    public bool CanDeleteCourse => Course.ID > 0;

    [RelayCommand]
    private async Task DeleteCourseAsync()
    {
        if (Course.ID == 0)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Delete Course", $"Delete {Course.CourseName}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        var assessments = await database.GetAssessmentsForCourseAsync(Course.ID);
        foreach (var assessment in assessments)
        {
            CancelAssessmentNotifications(assessment);
        }

        LocalNotificationCenter.Current.Cancel(GetNotificationId(Course.ID, StartNotificationOffset));
        LocalNotificationCenter.Current.Cancel(GetNotificationId(Course.ID, EndNotificationOffset));
        await database.DeleteCourseAsync(Course);
        await Shell.Current.GoToAsync("..");
    }

    partial void OnCourseChanged(Course value)
    {
        OnPropertyChanged(nameof(CanDeleteCourse));
        EnsureDefaults();
    }

    partial void OnSelectedStatusChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            Course.Status = value;
        }
    }

    [RelayCommand]
    private Task EditAssessmentAsync(AssessmentEditorViewModel? editor)
    {
        if (editor is null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current?.GoToAsync(nameof(AssessmentDetailPage), new Dictionary<string, object>
        {
            ["Editor"] = editor
        }) ?? Task.CompletedTask;
    }

    [RelayCommand]
    private Task EditNotesAsync()
    {
        return Shell.Current?.GoToAsync(nameof(CourseNotesPage), new Dictionary<string, object>
        {
            ["ViewModel"] = this
        }) ?? Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveNotesAsync()
    {
        if (Course is null)
        {
            return;
        }

        CourseNotes = CourseNotes?.Trim() ?? string.Empty;

        if (Course.ID > 0)
        {
            await database.UpdateCourseNotesAsync(Course.ID, CourseNotes);
        }

        RefreshNotesDisplay();
        await Shell.Current.GoToAsync("..");
    }

    public void RefreshNotesDisplay()
    {
        CourseNotes = Course?.Notes ?? string.Empty;
    }

    partial void OnCourseNotesChanged(string value)
    {
        if (Course is not null)
        {
            Course.Notes = value;
        }
    }

    [RelayCommand]
    private async Task ShareNotesAsync()
    {
        if (Course is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CourseNotes))
        {
            await DisplayAlert("Share Notes", "Add notes to share them.");
            return;
        }

        await Share.RequestAsync(new ShareTextRequest
        {
            Text = CourseNotes,
            Title = $"Notes for {Course.CourseName}"
        });
    }

    private void EnsureDefaults()
    {
        if (Course is null)
        {
            Course = new Course();
        }

        Course.InstructorName ??= string.Empty;
        Course.InstructorPhone ??= string.Empty;
        Course.InstructorEmail ??= string.Empty;
        Course.Notes ??= string.Empty;

        var normalizedStatus = NormalizeStatus(Course.Status);
        Course.Status = normalizedStatus;
        SelectedStatus = normalizedStatus;

        RefreshNotesDisplay();
    }

    private static string NormalizeStatus(string? status)
    {
        if (TryParseStatus(status, out var parsedStatus))
        {
            return StatusDisplayNames[parsedStatus];
        }

        return StatusDisplayNames[CourseStatus.InProgress];
    }

    private static string NormalizeAssessmentStatus(string? status)
    {
        if (TryParseAssessmentStatus(status, out var parsedStatus))
        {
            return AssessmentStatusDisplayNames[parsedStatus];
        }

        return AssessmentStatusDisplayNames[AssessmentStatus.NotStarted];
    }

    private static bool TryParseStatus(string? statusText, out CourseStatus status)
    {
        if (!string.IsNullOrWhiteSpace(statusText))
        {
            var trimmed = statusText.Trim();

            if (StatusLookup.TryGetValue(trimmed, out status))
            {
                return true;
            }
        }

        status = CourseStatus.InProgress;
        return false;
    }

    private static bool TryParseAssessmentStatus(string? statusText, out AssessmentStatus status)
    {
        if (!string.IsNullOrWhiteSpace(statusText))
        {
            var trimmed = statusText.Trim();

            if (AssessmentStatusLookup.TryGetValue(trimmed, out status))
            {
                return true;
            }
        }

        status = AssessmentStatus.NotStarted;
        return false;
    }

    private static IReadOnlyDictionary<string, CourseStatus> CreateStatusLookup()
    {
        var lookup = new Dictionary<string, CourseStatus>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in StatusDisplayNames)
        {
            lookup[kvp.Value] = kvp.Key;
            lookup[kvp.Key.ToString()] = kvp.Key;
        }

        return lookup;
    }

    private static IReadOnlyDictionary<string, AssessmentStatus> CreateAssessmentStatusLookup()
    {
        var lookup = new Dictionary<string, AssessmentStatus>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in AssessmentStatusDisplayNames)
        {
            lookup[kvp.Value] = kvp.Key;
            lookup[kvp.Key.ToString()] = kvp.Key;
        }

        return lookup;
    }

    private static Assessment CreateAssessment(AssessmentType type)
    {
        return new Assessment
        {
            Type = type.ToString(),
            Status = AssessmentStatusDisplayNames[AssessmentStatus.NotStarted],
            DueDate = DateTime.Today,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today,
            StartAlertEnabled = false,
            EndAlertEnabled = false
        };
    }

    private static void EnsureAssessmentDefaults(Assessment assessment, AssessmentType type)
    {
        assessment.Type = type.ToString();
        assessment.Title ??= string.Empty;
        assessment.Status = NormalizeAssessmentStatus(assessment.Status);
        if (assessment.DueDate == default)
        {
            assessment.DueDate = DateTime.Today;
        }

        if (assessment.StartDate == default)
        {
            assessment.StartDate = assessment.DueDate != default
                ? assessment.DueDate
                : DateTime.Today;
        }

        if (assessment.EndDate == default)
        {
            assessment.EndDate = assessment.DueDate != default
                ? assessment.DueDate
                : assessment.StartDate;
        }

        if (assessment.EndDate < assessment.StartDate)
        {
            assessment.EndDate = assessment.StartDate;
        }
    }

    private static void EnsureDefaultAssessmentTitle(AssessmentEditorViewModel editor, string defaultTitle)
    {
        if (string.IsNullOrWhiteSpace(editor.Title))
        {
            editor.Title = defaultTitle;
        }
    }

    private async Task LoadAssessmentsAsync()
    {
        if (Course?.ID <= 0)
        {
            ObjectiveAssessmentEditor.LoadAssessment(CreateAssessment(AssessmentType.Objective));
            PerformanceAssessmentEditor.LoadAssessment(CreateAssessment(AssessmentType.Performance));
            return;
        }

        var assessments = await database.GetAssessmentsForCourseAsync(Course.ID);

        var objectiveAssessment = assessments
            .FirstOrDefault(a => TryParseAssessmentType(a.Type) == AssessmentType.Objective)
            ?? CreateAssessment(AssessmentType.Objective);

        var performanceAssessment = assessments
            .FirstOrDefault(a => TryParseAssessmentType(a.Type) == AssessmentType.Performance)
            ?? CreateAssessment(AssessmentType.Performance);

        objectiveAssessment.CourseId = Course.ID;
        performanceAssessment.CourseId = Course.ID;
        EnsureAssessmentDefaults(objectiveAssessment, AssessmentType.Objective);
        EnsureAssessmentDefaults(performanceAssessment, AssessmentType.Performance);

        ObjectiveAssessmentEditor.LoadAssessment(objectiveAssessment);
        PerformanceAssessmentEditor.LoadAssessment(performanceAssessment);
    }

    private async Task SaveAssessmentsAsync()
    {
        if (Course is null)
        {
            return;
        }

        ObjectiveAssessmentEditor.PrepareForSave(Course.ID);
        PerformanceAssessmentEditor.PrepareForSave(Course.ID);

        await database.SaveAssessmentAsync(ObjectiveAssessmentEditor.Assessment);
        await database.SaveAssessmentAsync(PerformanceAssessmentEditor.Assessment);
        ObjectiveAssessmentEditor.LoadAssessment(ObjectiveAssessmentEditor.Assessment);
        PerformanceAssessmentEditor.LoadAssessment(PerformanceAssessmentEditor.Assessment);
        await UpdateAssessmentNotificationsAsync(ObjectiveAssessmentEditor);
        await UpdateAssessmentNotificationsAsync(PerformanceAssessmentEditor);
    }

    private async Task SaveAssessmentFromEditorAsync(AssessmentEditorViewModel editor)
    {
        if (Course?.ID <= 0)
        {
            await DisplayAlert("Save Course", "Save the course before updating assessments.");
            return;
        }

        if (string.IsNullOrWhiteSpace(editor.Title))
        {
            await DisplayAlert(editor.DisplayName, $"Enter a title for the {editor.DisplayName.ToLowerInvariant()} before saving.");
            return;
        }

        if (!await ValidateAssessmentBeforeSaveAsync(editor))
        {
            return;
        }

        editor.PrepareForSave(Course.ID);
        await database.SaveAssessmentAsync(editor.Assessment);
        editor.LoadAssessment(editor.Assessment);
        await UpdateAssessmentNotificationsAsync(editor);
        await Shell.Current.GoToAsync("..");
    }

    private async Task<bool> ValidateAssessmentBeforeSaveAsync(AssessmentEditorViewModel editor)
    {
        if (!AssessmentValidator.TryValidateDates(editor.StartDate, editor.EndDate, editor.DueDate, out var errorMessage))
        {
            await DisplayAlert(editor.DisplayName, errorMessage);
            return false;
        }

        return true;
    }

    private async Task DeleteAssessmentFromEditorAsync(AssessmentEditorViewModel editor)
    {
        if (editor is null)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Delete Assessment", $"Delete {editor.DisplayName}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        if (editor.Assessment.ID > 0)
        {
            CancelAssessmentNotifications(editor.Assessment);
            await database.DeleteAssessmentAsync(editor.Assessment);
        }

        var resetAssessment = CreateAssessment(editor.AssessmentType);
        if (Course?.ID > 0)
        {
            resetAssessment.CourseId = Course.ID;
        }

        editor.LoadAssessment(resetAssessment);
        await Shell.Current.GoToAsync("..");
    }

    private async Task UpdateNotificationsAsync()
    {
        if (Course?.ID <= 0)
        {
            return;
        }

        var startId = GetNotificationId(Course.ID, StartNotificationOffset);
        var endId = GetNotificationId(Course.ID, EndNotificationOffset);

        if (Course.StartAlertEnabled)
        {
            await ScheduleNotificationAsync(startId, "Course Starts", $"{Course.CourseName} begins today.", Course.StartDate);
        }
        else
        {
            LocalNotificationCenter.Current.Cancel(startId);
        }

        if (Course.EndAlertEnabled)
        {
            await ScheduleNotificationAsync(endId, "Course Ends", $"{Course.CourseName} ends today.", Course.EndDate);
        }
        else
        {
            LocalNotificationCenter.Current.Cancel(endId);
        }
    }

    private async Task UpdateAssessmentNotificationsAsync(AssessmentEditorViewModel editor)
    {
        if (Course?.ID <= 0 || editor?.Assessment?.ID <= 0)
        {
            return;
        }

        var assessmentTitle = string.IsNullOrWhiteSpace(editor.Title)
            ? editor.DisplayName
            : editor.Title;
        var courseName = string.IsNullOrWhiteSpace(Course.CourseName)
            ? "this course"
            : Course.CourseName;

        var startId = GetAssessmentNotificationId(editor.Assessment.ID, AssessmentStartNotificationOffset);
        var endId = GetAssessmentNotificationId(editor.Assessment.ID, AssessmentEndNotificationOffset);

        if (editor.StartAlertEnabled)
        {
            await ScheduleNotificationAsync(
                startId,
                $"{assessmentTitle} Starts",
                $"{assessmentTitle} for {courseName} starts today.",
                editor.StartDate);
        }
        else
        {
            LocalNotificationCenter.Current.Cancel(startId);
        }

        if (editor.EndAlertEnabled)
        {
            await ScheduleNotificationAsync(
                endId,
                $"{assessmentTitle} Ends",
                $"{assessmentTitle} for {courseName} ends today.",
                editor.EndDate);
        }
        else
        {
            LocalNotificationCenter.Current.Cancel(endId);
        }
    }

    private static AssessmentType TryParseAssessmentType(string? type)
    {
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse(type, out AssessmentType parsed))
        {
            return parsed;
        }

        return AssessmentType.Objective;
    }

    private static void CancelAssessmentNotifications(Assessment assessment)
    {
        if (assessment?.ID <= 0)
        {
            return;
        }

        LocalNotificationCenter.Current.Cancel(GetAssessmentNotificationId(assessment.ID, AssessmentStartNotificationOffset));
        LocalNotificationCenter.Current.Cancel(GetAssessmentNotificationId(assessment.ID, AssessmentEndNotificationOffset));
    }

    private static int GetNotificationId(int courseId, int offset) => (courseId * 10) + offset;

    private static int GetAssessmentNotificationId(int assessmentId, int offset) => (assessmentId * 10) + offset + 100000;

    private static async Task ScheduleNotificationAsync(int notificationId, string title, string message, DateTime date)
    {
        var notifyTime = date.Date.AddHours(9);

        if (notifyTime < DateTime.Now)
        {
            return;
        }

        var request = new NotificationRequest
        {
            NotificationId = notificationId,
            Title = title,
            Description = message,
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = notifyTime
            }
        };

        await LocalNotificationCenter.Current.Show(request);
    }
}
