using System.ComponentModel.Design;
using System.Net;
using Duende.IdentityServer.Extensions;
using Exams.NET.Controllers.Modification;
using Exams.NET.Data;
using Exams.NET.Providers;
using Exams.NET.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace Exams.NET.Tests.ControllerTests.AdminControllers; 

public class Test {
    private TestAdministrationContext _testContext;
    private IUserIdProvider _idProvider;
    public Test() {
        var options = new DbContextOptionsBuilder<TestAdministrationContext>()
                      .UseInMemoryDatabase("fakeDb")
                      .Options;

        _testContext = new TestAdministrationContext(options);
        
        _testContext.Database.EnsureDeleted();
        _testContext.Database.EnsureCreated();
        
        _testContext.Tests.AddRange(_tests);
        _testContext.SaveChanges();
        _idProvider = Substitute.For<IUserIdProvider>();
    }
    
    private List<Exams.NET.Models.Test> _tests = new() {
        new(){TestId = 1,UserId = "1"}, new(){TestId = 2, UserId = "2"}, new Models.Test() {TestId = 3,UserId = "1"},
    };
    
    //get
    [InlineData("1")]
    [InlineData("2")]
    [Theory]
    public async Task GetsTestsCreatedByUser(string id) {
        // arrange 
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(id);
        var sut = new TestController(_testContext, _idProvider);
        
        var actual = await sut.GetTests();
        
        //assert
        Assert.True(actual?.Value?.Count() == _testContext.Tests.Count(x => x.UserId == id));
    }
    
    [Fact]
    public async Task ReturnEmptyListIfNotFound() {
        // arrange 
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");
        var sut = new TestController(_testContext, _idProvider);
        
        var actual = await sut.GetTests();
        
        //assert
        Assert.True(actual?.Value?.Count() == 0);
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundWhenGetByIdFails() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");

        var sut = new TestController(_testContext, _idProvider);

        var actual = await sut.GetTest(-1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    
    //get{id}
    [Fact]
    public async Task ReturnsTestWithCorrectId() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("1");

        var sut = new TestController(_testContext, _idProvider);

        var actual = await sut.GetTest(1);

        Assert.True(actual.Value?.TestId == 1);
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundWhenSearchingForAnotherUsersTest() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");

        var sut = new TestController(_testContext, _idProvider);

        var actual = await sut.GetTest(1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    
    [Fact]
    public async Task ReturnsNotFoundWhenSearchingForANonexistentTest() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("1");

        var sut = new TestController(_testContext, _idProvider);

        var actual = await sut.GetTest(-1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    //put{id}
    [Fact]
    public async Task UserCanUpdateOwnTests() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId == userId);
        var expected = Guid.NewGuid().ToString();
        test.TestDescription = expected;
        
        var returnCode = await sut.PutTest(test.TestId, test);
        var actual = _testContext.Tests.First(x => x.TestId == test.TestId);
        
        
        Assert.IsNotType<NotFoundResult>(returnCode);
        Assert.True(actual.TestDescription == expected);
    }
    
    [Fact]
    public async Task CannotCreateWithPut() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId == userId);
        test.TestId = -1;
        
        var actual = await sut.PutTest(test.TestId, test);
        
        Assert.IsAssignableFrom<NotFoundResult>(actual);
    }
    
    [Fact]
    public async Task CannotEditAnotherUsersTest() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId != userId);
        
        var actual = await sut.PutTest(test.TestId, test);
        
        Assert.IsAssignableFrom<NotFoundResult>(actual);
    }
    
    [Fact]
    public async Task MismatchedIdsReturnBadRequest() {
        var sut = new TestController(_testContext, _idProvider);
        var test = _tests[0];
        
        var actual = await sut.PutTest(-1, test);
        
        Assert.IsAssignableFrom<BadRequestResult>(actual);
    }
    
    [Fact]
    public async Task DefaultIdReturnsBadRequest() {
        var sut = new TestController(_testContext, _idProvider);
        var test = _tests[0];
        test.TestId = 0;
        
        var actual = await sut.PutTest(0, test);
        
        Assert.IsAssignableFrom<BadRequestResult>(actual);
    }
    //post
    [Fact]
    public async Task CannotCreateWithMismatchedUser() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider);
        var test = _testContext.Tests.AsNoTracking().First(_ => true);
        var fakeId = Guid.NewGuid().ToString();
        test.UserId = fakeId;
        
        var actual = await sut.PostTest(test.ToCreationDto());

        Assert.IsAssignableFrom<CreatedAtActionResult>(actual.Result);
        Assert.Null(_testContext.Tests.FirstOrDefault(x=>x.UserId == fakeId));
    }
    //delete{id}
    
}