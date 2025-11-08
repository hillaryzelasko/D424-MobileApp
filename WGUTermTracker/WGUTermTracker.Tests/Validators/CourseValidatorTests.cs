using NUnit.Framework;
using WGUTermTracker.Tests.TestData;
using WGUTermTracker.Validation;

namespace WGUTermTracker.Tests.Validators;

[TestFixture]
public class CourseValidatorTests
{
    private string? statusOption;

    [SetUp]
    public void SetUp()
    {
        // Pretend the UI already picked the "In Progress" option.
        statusOption = "In Progress";
    }

    [Test]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        var course = TestDataFactory.CreateValidCourse();

        var result = CourseValidator.TryValidate(course, statusOption, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(error, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void IsValid_WithInvalidEmail_ShouldReturnFalse()
    {
        var course = TestDataFactory.CreateCourseWithInvalidEmail();

        var result = CourseValidator.TryValidate(course, statusOption, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("valid email"));
        });
    }

    [Test]
    public void IsValid_WithEndDateEarlierThanStartDate_ShouldReturnFalse()
    {
        var course = TestDataFactory.CreateCourseWithEndDateBeforeStart();

        var result = CourseValidator.TryValidate(course, statusOption, out var error); 

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("end date"));
        });
    }

    [Test]
    public void IsValid_WithNullCourse_ShouldReturnFalse()
    {
        var result = CourseValidator.TryValidate(course: null, selectedStatus: statusOption, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("Course data"));
        });
    }

    [Test]
    public void IsValid_WithMissingStatus_ShouldReturnFalse()
    {
        var course = TestDataFactory.CreateValidCourse();

        var result = CourseValidator.TryValidate(course, selectedStatus: string.Empty, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(error, Does.Contain("status"));
        });
    }
}
