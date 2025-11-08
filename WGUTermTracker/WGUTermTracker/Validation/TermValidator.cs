using WGUTermTracker.Models;

namespace WGUTermTracker.Validation;

public static class TermValidator
{
    public static bool TryValidateDates(Term? term, out string errorMessage)
    {
        if (term is null)
        {
            errorMessage = "Term data must be provided before saving.";
            return false;
        }

        if (term.EndDate < term.StartDate)
        {
            errorMessage = "The term end date must be on or after the start date.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
