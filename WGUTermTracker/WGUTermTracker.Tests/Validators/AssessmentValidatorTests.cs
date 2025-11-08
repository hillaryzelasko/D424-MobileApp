using System;
using NUnit.Framework;
using WGUTermTracker.Validation;

namespace WGUTermTracker.Tests.Validators;

[TestFixture]
public class AssessmentValidatorTests
{
    private DateTime start;
    private DateTime end;

    [SetUp]
    public void SetUp()
    {
        start = new DateTime(2025, 2, 1);
        end = start.AddDays(14);
    }

    [Test]
    public void TryValidateDates_WithValidValues_ReturnsTrue()
    {
        var due = start.AddDays(7);

        var result = AssessmentValidator.TryValidateDates(start, end, due, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(error, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void TryValidateDates_WithDueBeforeStart_ReturnsFalse()
    {
        var due = start.AddDays(-1);

        var result = AssessmentValidator.TryValidateDates(start, end, due, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("due date"));
        });
    }

    [Test]
    public void TryValidateDates_WithDueAfterEnd_ReturnsFalse()
    {
        var due = end.AddDays(1);

        var result = AssessmentValidator.TryValidateDates(start, end, due, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("anticipated end"));
        });
    }

    [Test]
    public void TryValidateDates_WithEndBeforeStart_ReturnsFalse()
    {
        var invalidEnd = start.AddDays(-1);
        var due = start;

        var result = AssessmentValidator.TryValidateDates(start, invalidEnd, due, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("end date"));
        });
    }
}
