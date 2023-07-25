using Exams.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Data;

public class TestAdministrationContext : DbContext {
    public TestAdministrationContext(DbContextOptions<TestAdministrationContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; set; }
    public DbSet<MultipleChoiceProblem> MultipleChoiceQuestions { get; set; }
    public DbSet<FreeFormProblem> FreeFormQuestions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MultipleChoiceProblem>()
                    .ToTable(nameof(MultipleChoiceProblem));

        modelBuilder.Entity<MultipleChoiceProblem>()
                    .HasMany<Choice>()
                    .WithOne(e => e.TestQuestion as MultipleChoiceProblem)
                    .HasForeignKey(x => x.TestQuestionID);
        
        modelBuilder.Entity<FreeFormProblem>()
                    .ToTable(nameof(FreeFormProblem));
    }
}