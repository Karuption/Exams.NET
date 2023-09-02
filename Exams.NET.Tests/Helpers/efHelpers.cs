using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Tests.Helpers; 

public static class EfHelpers {
    public static T ContextSetup<T>() where T: DbContext {
        var options = new DbContextOptionsBuilder<T>()
                          .UseInMemoryDatabase(Guid.NewGuid().ToString())
                          .Options;
        
        var dbContext = Activator.CreateInstance(typeof(T),options) as T;

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        dbContext.SaveChanges();

        return dbContext;
    }
}