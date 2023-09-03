using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Exams.NET.Models;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
    [Key]
    public Guid Id { get; set; }
    public int TestId { get; set; }
    [JsonIgnore]
    public string OwnerId { get; set; }
    public bool Shared { get; set; }
}