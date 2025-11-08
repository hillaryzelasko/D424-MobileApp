# WGU Term Tracker

This repository contains a .NET MAUI application for tracking WGU terms, courses, and assessments. The solution now includes an NUnit test project that verifies the core validation logic used by the app when saving data.

## Solution Layout

- `WGUTermTracker/` – Main MAUI application.
- `WGUTermTracker.Tests/` – NUnit test project with validation and schema tests.

## Validation Coverage

The validators live under `WGUTermTracker/Validation/` and perform the same checks that appear in the view models:

- `CourseValidator` ensures a course has a title, valid date range, selected status, instructor contact info, and a syntactically valid email address.
- `TermValidator` verifies the term end date does not precede the start date.
- `AssessmentValidator` keeps the assessment start, end, and due dates in a logical order.

## Running the Tests

1. Install the .NET 9 SDK and the MAUI workload for Visual Studio 2022.
2. Restore NuGet packages for the solution.
3. From Visual Studio Test Explorer or the command line, run the tests in `WGUTermTracker.Tests`:
   ```bash
   dotnet test WGUTermTracker.sln
   ```
