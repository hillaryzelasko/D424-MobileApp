using System;
using System.Text.RegularExpressions;
using WGUTermTracker.Models;

namespace WGUTermTracker.Validation;

public static class CourseValidator
{
    private static readonly Regex EmailRegex = new(
        "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool TryValidate(Course? course, string? selectedStatus, out string errorMessage)
    {
        if (course is null)
        {
            errorMessage = "Course data must be provided before saving.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(course.CourseName))
        {
            errorMessage = "Enter a course name before saving.";
            return false;
        }

        if (course.EndDate < course.StartDate)
        {
            errorMessage = "The course end date must be on or after the start date.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(selectedStatus))
        {
            errorMessage = "Select a course status before saving.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(course.InstructorName))
        {
            errorMessage = "Enter the course instructor's name before saving.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(course.InstructorPhone))
        {
            errorMessage = "Enter the course instructor's phone number before saving.";
            return false;
        }

        if (!IsValidEmail(course.InstructorEmail))
        {
            errorMessage = "Enter a valid email address for the instructor before saving.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return EmailRegex.IsMatch(email);
    }
}
