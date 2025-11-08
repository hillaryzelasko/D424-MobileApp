using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WGUTermTracker.Data;
using WGUTermTracker.Models;
using WGUTermTracker.Views;
using WGUTermTracker.Validation;

namespace WGUTermTracker.ViewModels;

public partial class TermDetailViewModel : ObservableObject
{
    /* 6 COURSES PER TERM */
    private const int MaxCoursesPerTerm = 6;

    private readonly AppDatabase database;

    public ObservableCollection<Course> Courses { get; } = new();

    [ObservableProperty]
    private Term? term;

    private bool isLoadingCourses;

    public TermDetailViewModel(AppDatabase database)
    {
        this.database = database;
        Courses.CollectionChanged += OnCoursesCollectionChanged;
    }

    public bool CanDeleteTerm => Term?.ID > 0;

    /* WARNINGS FOR COURSE QTY */
    public bool ShowCourseLimitWarning => Term?.ID > 0 && Courses.Count >= MaxCoursesPerTerm;

    public bool CanAddAnotherCourse => Term?.ID > 0 && Courses.Count < MaxCoursesPerTerm;

    public async Task InitializeAsync(Term term)
    {
        var editableTerm = new Term
        {
            ID = term.ID,
            Termname = term.Termname,
            StartDate = term.StartDate,
            EndDate = term.EndDate
        };

        Term = editableTerm;
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (isLoadingCourses || Term is null)
        {
            return;
        }

        try
        {
            isLoadingCourses = true;

            if (Term.ID > 0)
            {
                var courses = await database.GetCoursesForTermAsync(Term.ID);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Courses.Clear();
                    foreach (var course in courses.OrderBy(c => c.StartDate))
                    {
                        Courses.Add(course);
                    }
                });
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() => Courses.Clear());
            }
        }
        finally
        {
            isLoadingCourses = false;
            UpdateCourseLimitState();
        }
    }

    /* SAVE TERM */
    [RelayCommand]
    private async Task SaveTermAsync()
    {
        if (Term is null)
        {
            return;
        }

        /* TERM NAME !NULL */
        if (string.IsNullOrWhiteSpace(Term.Termname))
        {
            await DisplayAlert("Name Required", "Please enter a name for the term.");
            return;
        }

        /* TERM CANNOT END BEFORE START DATE */
        if (!TermValidator.TryValidateDates(Term, out var errorMessage))
        {
            await DisplayAlert("Invalid Dates", errorMessage);
            return;
        }

        /* CLEAN DATA FOR DATA SECURITY */
        Term.Termname = Term.Termname.Trim();

        await database.SaveTermAsync(Term);
        await Shell.Current.GoToAsync("..");
    }

    /* DELETE TERM */
    [RelayCommand]
    private async Task DeleteTermAsync()
    {
        if (Term?.ID <= 0)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Delete Term", $"Delete {Term.Termname}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        await database.DeleteTermAsync(Term);
        await Shell.Current.GoToAsync("..");
    }

    /* CANCEL CREATE/EDIT TERM */
    [RelayCommand]
    private Task CancelAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    /* ADDING COURSE COMMAND */
    [RelayCommand]
    private async Task AddCourseAsync()
    {
        if (Term is null)
        {
            return;
        }

        /* COURSE IS INDEXED WITH TERM */
        if (Term.ID == 0)
        {
            await DisplayAlert("Save Term", "Please save the term before adding courses.");
            return;
        }

        /* LIMIT CREATION CT */
        if (Courses.Count >= MaxCoursesPerTerm)
        {
            await DisplayAlert("Course Limit Reached", "You can only add up to six courses per term.");
            return;
        }

        /* COURSE CREATE */
        var course = new Course
        {
            TermId = Term.ID,
            CourseName = string.Empty,
            StartDate = Term.StartDate,
            EndDate = Term.EndDate,

        };

        await Shell.Current.GoToAsync(nameof(CoursePage), true, new Dictionary<string, object>
        {
            ["Course"] = course
        });
    }

    /* EDIT COURSE */
    [RelayCommand]
    private async Task EditCourseAsync(Course? course)
    {
        if (course is null)
        {
            return;
        }

        var editableCourse = new Course
        {
            ID = course.ID,
            TermId = course.TermId,
            CourseName = course.CourseName,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Status = course.Status,
            InstructorName = course.InstructorName,
            InstructorPhone = course.InstructorPhone,
            InstructorEmail = course.InstructorEmail,
            Notes = course.Notes,
            StartAlertEnabled = course.StartAlertEnabled,
            EndAlertEnabled = course.EndAlertEnabled
        };

        await Shell.Current.GoToAsync(nameof(CoursePage), true, new Dictionary<string, object>
        {
            ["Course"] = editableCourse
        });
    }

    /* DELETE COURSE */
    [RelayCommand]
    private async Task DeleteCourseAsync(Course? course)
    {
        if (course is null)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlert("Delete Course", $"Remove {course.CourseName}?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        await database.DeleteCourseAsync(course);
        await RefreshAsync();
    }

    /* TERM DELETE CASCADES TO COURSE DELETE */
    partial void OnTermChanged(Term? value)
    {
        OnPropertyChanged(nameof(CanDeleteTerm));
        UpdateCourseLimitState();
    }

    private void OnCoursesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCourseLimitState();
    }

    /* LIMIT ADDING UP TO 6 COURSES */
    private void UpdateCourseLimitState()
    {
        OnPropertyChanged(nameof(ShowCourseLimitWarning));
        OnPropertyChanged(nameof(CanAddAnotherCourse));
    }

    private static Task DisplayAlert(string title, string message)
    {
        return Shell.Current?.DisplayAlert(title, message, "OK") ?? Task.CompletedTask;
    }
}