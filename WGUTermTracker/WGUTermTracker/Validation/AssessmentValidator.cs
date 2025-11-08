using System;

namespace WGUTermTracker.Validation;

public static class AssessmentValidator
{
    public static bool TryValidateDates(DateTime start, DateTime end, DateTime due, out string errorMessage)
    {
        if (end < start)
        {
            errorMessage = "The anticipated end date must be on or after the anticipated start date.";
            return false;
        }

        if (due < start)
        {
            errorMessage = "The due date must be on or after the anticipated start date.";
            return false;
        }

        if (due > end)
        {
            errorMessage = "The due date must be on or before the anticipated end date.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
