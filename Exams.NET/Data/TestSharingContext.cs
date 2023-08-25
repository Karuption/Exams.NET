using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Data; 

public class TestSharingContext : DbContext {
    public DbSet<TestShare> TestShares { get; set; }
    public DbSet<SharedTest> TestShareInfo { get; set; }

    public TestSharingContext(DbContextOptions<TestSharingContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
    }
}

public class TestShare {
    public Guid Id { get; set; }
    public string AllowedUserId { get; set; }
    public SharedTest SharedTest { get; set; }
}

public class SharedTest {
    public Guid Id { get; set; }
    public int testId { get; set; }
    public string OwnerId { get; set; }
    public bool shared { get; set; }
}