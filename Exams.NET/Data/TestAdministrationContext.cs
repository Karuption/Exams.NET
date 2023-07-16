using Exams.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Data;

public class TestAdministrationContext : DbContext {
    public TestAdministrationContext(DbContextOptions<TestAdministrationContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; set; }
    public DbSet<TestQuestion> TestQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestQuestion>()
                    .HasKey(x => x.TestQuestionId);
        
        modelBuilder.Entity<TestQuestion>()
                    .ToTable("Test Questions")
                    .HasDiscriminator<string>(nameof(TestQuestion) + "type")
                    .HasValue<MultipleChoiceProblem>(nameof(MultipleChoiceProblem))
                    .HasValue<FreeFormProblem>(nameof(FreeFormProblem));
    }
}