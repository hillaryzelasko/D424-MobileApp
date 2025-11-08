using System.Linq;
using NUnit.Framework;
using SQLite;
using WGUTermTracker.Models;

namespace WGUTermTracker.Tests.Models;

[TestFixture]
public class CourseSchemaTests
{
    [Test]
    public void TermId_ShouldBeIndexed_ForFasterLookups()
    {
        var termIdProperty = typeof(Course).GetProperty(nameof(Course.TermId));
        Assert.That(termIdProperty, Is.Not.Null);

        var indexAttribute = termIdProperty!
            .GetCustomAttributes(typeof(IndexedAttribute), inherit: false)
            .Cast<IndexedAttribute>()
            .FirstOrDefault();

        Assert.That(indexAttribute, Is.Not.Null, "TermId should be indexed in the database schema.");
    }

    [Test]
    public void Course_ShouldMapToCoursesTable()
    {
        var tableAttribute = typeof(Course)
            .GetCustomAttributes(typeof(TableAttribute), inherit: false)
            .Cast<TableAttribute>()
            .FirstOrDefault();

        Assert.That(tableAttribute, Is.Not.Null, "Course should be mapped to the Courses table.");
        Assert.That(tableAttribute!.Name, Is.EqualTo("Courses"));
    }
}
