using Exams.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Data;

public class TestAdministrationContext : DbContext {
    public TestAdministrationContext(DbContextOptions<TestAdministrationContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<MultipleChoiceProblem> MultipleChoiceQuestions { get; set; } = null!;
    public DbSet<FreeFormProblem> FreeFormQuestions { get; set; } = null!;
    public DbSet<TestQuestion> TestQuestions { get; set; } = null!;
    public DbSet<UserTestQuestionAnswer> UserAnswers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Test>()
                    .HasMany<TestQuestion>(x=>x.Problems)
                    .WithOne(x => x.Test)
                    .HasForeignKey(x=>x.TestId)
                    .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<MultipleChoiceProblem>()
                    .HasMany<Choice>()
                    .WithOne(e => e.TestQuestion as MultipleChoiceProblem);

        modelBuilder.Entity<MultipleChoiceProblem>()
                    .Navigation(x => x.Choices)
                    .AutoInclude();
        
        modelBuilder.Entity<TestQuestion>()
                    .HasDiscriminator<string>("Type")
                    .HasValue<MultipleChoiceProblem>("MultipleChoice")
                    .HasValue<FreeFormProblem>("FreeForm");
    }
}