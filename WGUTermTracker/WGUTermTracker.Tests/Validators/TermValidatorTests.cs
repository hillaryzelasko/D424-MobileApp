using NUnit.Framework;
using WGUTermTracker.Tests.TestData;
using WGUTermTracker.Validation;

namespace WGUTermTracker.Tests.Validators;

[TestFixture]
public class TermValidatorTests
{
    [Test]
    public void TryValidateDates_WithValidRange_ReturnsTrue()
    {
        var term = TestDataFactory.CreateValidTerm();

        var result = TermValidator.TryValidateDates(term, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(error, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void TryValidateDates_WithEndDateBeforeStart_ReturnsFalse()
    {
        var term = TestDataFactory.CreateTermWithEndDateBeforeStart();

        var result = TermValidator.TryValidateDates(term, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("end date"));
        });
    }

    [Test]
    public void TryValidateDates_WithNullTerm_ReturnsFalse()
    {
        var result = TermValidator.TryValidateDates(null, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("Term data"));
        });
    }
}
