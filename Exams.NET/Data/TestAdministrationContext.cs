using Exams.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Data;

public class TestAdministrationContext : DbContext {
    public TestAdministrationContext(DbContextOptions<TestAdministrationContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; set; }
    public DbSet<TestQuestion> TestQuestions { get; set; }
}