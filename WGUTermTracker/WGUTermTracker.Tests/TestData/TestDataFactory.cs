using System;
using WGUTermTracker.Models;

namespace WGUTermTracker.Tests.TestData;

public static class TestDataFactory
{
    public static Course CreateValidCourse()
    {
        return new Course
        {
            TermId = 1,
            CourseName = "Software Design",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 3, 1),
            Status = "In Progress",
            InstructorName = "Casey Jones",
            InstructorPhone = "555-0100",
            InstructorEmail = "casey.jones@example.edu"
        };
    }

    public static Course CreateCourseWithInvalidEmail()
    {
        var course = CreateValidCourse();
        course.InstructorEmail = "no-at-symbol";
        return course;
    }

    public static Course CreateCourseWithEndDateBeforeStart()
    {
        var course = CreateValidCourse();
        course.EndDate = course.StartDate.AddDays(-1);
        return course;
    }

    public static Term CreateValidTerm()
    {
        return new Term
        {
            Termname = "Spring 2025",
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 6, 1)
        };
    }

    public static Term CreateTermWithEndDateBeforeStart()
    {
        var term = CreateValidTerm();
        term.EndDate = term.StartDate.AddDays(-5);
        return term;
    }
}
