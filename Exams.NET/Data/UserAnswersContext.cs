using Duende.IdentityServer.EntityFramework.Options;
using Exams.NET.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Exams.NET.Data;

public class UserAnswersContext : DbContext {
    public UserAnswersContext(DbContextOptions<TestAdministrationContext> options)
        : base(options) { }

    public DbSet<ITestQuestionUserAnswer> UserAnswers { get; set; }
}