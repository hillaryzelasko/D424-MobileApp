using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WGUTermTracker.Models;
using static WGUTermTracker.Models.Enums;

namespace WGUTermTracker.Data
{
    public class AppDatabase
    {
        SQLiteAsyncConnection database;

        async Task Init()
        {
            if (database is not null)
                return;

            database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await database.CreateTableAsync<Term>();
            await database.CreateTableAsync<Course>();
            await database.CreateTableAsync<Assessment>();
            await EnsureCourseSchemaAsync();
            await EnsureAssessmentSchemaAsync();
        }


        public async Task CreateEvaluationDataAsync()
        {
            await Init();

            const string evaluationTermName = "Term 4";
            const string evaluationCourseName = "Mobile Application Development Using C# - C971";

            var termStart = new DateTime(2025, 1, 8);
            var termEnd = new DateTime(2025, 6, 30);

            var term = await database.Table<Term>()
                .Where(t => t.Termname == evaluationTermName)
                .FirstOrDefaultAsync();

            if (term is null)
            {
                term = new Term
                {
                    Termname = evaluationTermName,
                    StartDate = termStart,
                    EndDate = termEnd
                };

                await database.InsertAsync(term);
            }
            else
            {
                term.Termname = evaluationTermName;
                term.StartDate = termStart;
                term.EndDate = termEnd;
                await database.UpdateAsync(term);
            }

            var courseStart = termStart.AddDays(7);
            var courseEnd = termEnd.AddDays(-7);

            var course = await database.Table<Course>()
                .Where(c => c.TermId == term.ID && c.CourseName == evaluationCourseName)
                .FirstOrDefaultAsync();

            if (course is null)
            {
                course = new Course
                {
                    TermId = term.ID,
                    CourseName = evaluationCourseName,
                    StartDate = courseStart,
                    EndDate = courseEnd,
                    Status = "In Progress",
                    InstructorName = "Anika Patel",
                    InstructorPhone = "555-123-4567",
                    InstructorEmail = "anika.patel@strimeuniversity.edu",
                    Notes = "Test Note",
                    EndAlertEnabled = false
                };

                await database.InsertAsync(course);
            }
            else
            {
                course.CourseName = evaluationCourseName;
                course.StartDate = courseStart;
                course.EndDate = courseEnd;
                course.Status = "In Progress";
                course.InstructorName = "Anika Patel";
                course.InstructorPhone = "555-123-4567";
                course.InstructorEmail = "anika.patel@strimeuniversity.edu";
                course.Notes = "Test Note";
                course.StartAlertEnabled = false;
                course.EndAlertEnabled = false;
                await database.UpdateAsync(course);
            }

            await EnsureAssessmentAsync(
                course.ID,
                "Mobile App Objective Assessment",
                AssessmentType.Objective.ToString(),
                "Scheduled",
                courseStart.AddMonths(2),
                courseStart.AddMonths(2),
                courseStart.AddMonths(2));

            await EnsureAssessmentAsync(
                course.ID,
                "Mobile App Performance Assessment",
                AssessmentType.Performance.ToString(),
                "Scheduled",
                courseEnd.AddMonths(-1),
                courseEnd.AddMonths(-1),
                courseEnd.AddMonths(-1));
        }

        private async Task EnsureAssessmentAsync(
            int courseId,
            string title,
            string type,
            string status,
            DateTime dueDate,
            DateTime startDate,
            DateTime endDate)
        {
            var assessment = await database.Table<Assessment>()
                .Where(a => a.CourseId == courseId && a.Type == type)
                .FirstOrDefaultAsync();

            if (assessment is null)
            {
                assessment = new Assessment
                {
                    CourseId = courseId,
                    Title = title,
                    Type = type,
                    Status = status,
                    DueDate = dueDate,
                    StartDate = startDate,
                    EndDate = endDate,
                    StartAlertEnabled = false,
                    EndAlertEnabled = false
                };

                await database.InsertAsync(assessment);
            }
            else
            {
                assessment.Title = title;
                assessment.Type = type;
                assessment.Status = status;
                assessment.DueDate = dueDate;
                assessment.StartDate = startDate;
                assessment.EndDate = endDate;
                assessment.StartAlertEnabled = false;
                assessment.EndAlertEnabled = false;
                await database.UpdateAsync(assessment);
            }
        }


        /* TERM DB DATA MANIP */
        public async Task<List<Term>> GetTermAsync()
        {
            await Init();
            return await database.Table<Term>().ToListAsync();
        }

        public async Task<Term> GetTermAsync(int id)
        {
            await Init();
            return await database.Table<Term>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTermAsync(Term term)
        {
            await Init();
            if (term.ID != 0)
            {
                return await database.UpdateAsync(term);
            }
            else
            {
                return await database.InsertAsync(term);
            }
        }

        public async Task<int> DeleteTermAsync(Term term)
        {
            await Init();
            await database.ExecuteAsync("DELETE FROM COURSES WHERE TermId =?", term.ID);
            return await database.DeleteAsync(term);
        }


        /* COURSE DB DATA MANIP */
        public async Task<List<Course>> GetCoursesForTermAsync(int termId)
        {
            await Init();
            return await database.Table<Course>().Where(c => c.TermId == termId).ToListAsync();
        }

        public async Task<List<CourseNoteSummary>> GetAllCourseNotesAsync()
        {
            await Init();

            const string query = @"SELECT c.ID AS CourseId,
                                            c.CourseName,
                                            c.Notes,
                                            IFNULL(t.Termname, '') AS TermName
                                     FROM Courses c
                                     LEFT JOIN Terms t ON c.TermId = t.ID
                                     ORDER BY t.StartDate, c.CourseName";

            return await database.QueryAsync<CourseNoteSummary>(query);
        }

        public async Task<List<CourseReportEntry>> GetCourseReportEntriesAsync()
        {
            await Init();

            const string query = @"SELECT IFNULL(t.Termname, '') AS TermName,
                                            c.CourseName,
                                            c.Status,
                                            c.StartDate,
                                            c.EndDate,
                                            IFNULL(c.Notes, '') AS Notes
                                     FROM Courses c
                                     LEFT JOIN Terms t ON c.TermId = t.ID
                                     ORDER BY t.StartDate, c.CourseName";

            return await database.QueryAsync<CourseReportEntry>(query);
        }
   
        public async Task<int> SaveCourseAsync(Course course)
        {
            await Init();
            if (course.ID != 0)
            {
                return await database.UpdateAsync(course);
            }

            return await database.InsertAsync(course);
        }

        public async Task<int> UpdateCourseNotesAsync(int courseId, string notes)
        {
            await Init();
            return await database.ExecuteAsync("UPDATE Courses SET Notes = ? WHERE ID = ?", notes, courseId);
        }

        public async Task<int> DeleteCourseAsync(Course course)
        {
            await Init();
            await database.ExecuteAsync("DELETE FROM Assessments WHERE CourseId = ?", course.ID);
            return await database.DeleteAsync(course);
        }

        /* ASSESSMENT DB DATA MANIP */
        public async Task<List<Assessment>> GetAssessmentsForCourseAsync(int courseId)
        {
            await Init();
            return await database.Table<Assessment>().Where(a => a.CourseId == courseId).ToListAsync();
        }

        public async Task<int> SaveAssessmentAsync(Assessment assessment)
        {
            await Init();
            if (assessment.ID != 0)
            {
                return await database.UpdateAsync(assessment);
            }

            return await database.InsertAsync(assessment);
        }

        public async Task<int> DeleteAssessmentAsync(Assessment assessment)
        {
            await Init();
            return await database.DeleteAsync(assessment);
        }

        /* COURSE FIELD VALIDATION CHECKS */
        private async Task EnsureCourseSchemaAsync()
        {
            var columns = await database.GetTableInfoAsync("Courses");

            async Task EnsureColumnAsync(string columnName, string columnDefinition)
            {
                if (columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                await database.ExecuteAsync($"ALTER TABLE Courses ADD COLUMN {columnName} {columnDefinition}");
                columns = await database.GetTableInfoAsync("Courses");
            }

            await EnsureColumnAsync(nameof(Course.Status), "TEXT NOT NULL DEFAULT 'In Progress'");
            await EnsureColumnAsync(nameof(Course.InstructorName), "TEXT NOT NULL DEFAULT ''");
            await EnsureColumnAsync(nameof(Course.InstructorPhone), "TEXT NOT NULL DEFAULT ''");
            await EnsureColumnAsync(nameof(Course.InstructorEmail), "TEXT NOT NULL DEFAULT ''");
            await EnsureColumnAsync(nameof(Course.Notes), "TEXT NOT NULL DEFAULT ''");
            await EnsureColumnAsync(nameof(Course.StartAlertEnabled), "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync(nameof(Course.EndAlertEnabled), "INTEGER NOT NULL DEFAULT 0");
        }

        private async Task EnsureAssessmentSchemaAsync()
        {
            var columns = await database.GetTableInfoAsync("Assessments");

            async Task EnsureColumnAsync(string columnName, string columnDefinition)
            {
                if (columns.Any(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                await database.ExecuteAsync($"ALTER TABLE Assessments ADD COLUMN {columnName} {columnDefinition}");
                columns = await database.GetTableInfoAsync("Assessments");
            }

            await EnsureColumnAsync(nameof(Assessment.Type), "TEXT NOT NULL DEFAULT 'Objective'");
            await EnsureColumnAsync(nameof(Assessment.Status), "TEXT NOT NULL DEFAULT 'Not Started'");
            await EnsureColumnAsync(nameof(Assessment.DueDate), "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00'");
            await EnsureColumnAsync(nameof(Assessment.StartDate), "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00'");
            await EnsureColumnAsync(nameof(Assessment.EndDate), "TEXT NOT NULL DEFAULT '0001-01-01T00:00:00'");
            await EnsureColumnAsync(nameof(Assessment.StartAlertEnabled), "INTEGER NOT NULL DEFAULT 0");
            await EnsureColumnAsync(nameof(Assessment.EndAlertEnabled), "INTEGER NOT NULL DEFAULT 0");
        }
    }
}
