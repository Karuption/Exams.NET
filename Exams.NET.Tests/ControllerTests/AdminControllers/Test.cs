using Exams.NET.Controllers.Modification;
using Exams.NET.Data;
using Exams.NET.Providers;
using Exams.NET.Models;
using Exams.NET.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Exams.NET.Tests.ControllerTests.AdminControllers; 

public class Test {
    private readonly TestAdministrationContext _testContext;
    private readonly IUserIdProvider _idProvider = Substitute.For<IUserIdProvider>();
    private TestSharingContext _sharingContext = EfHelpers.ContextSetup<TestSharingContext>();
    public Test() {
        _testContext = EfHelpers.ContextSetup<TestAdministrationContext>();
        _testContext.Tests.AddRange(_tests);
        _testContext.SaveChanges();
    }
    
    private readonly List<Exams.NET.Models.Test> _tests = new() {
        new(){TestId = 1,UserId = "1"}, new(){TestId = 2, UserId = "2"}, new Models.Test() {TestId = 3,UserId = "1"},
    };
    
    //get
    [InlineData("1")]
    [InlineData("2")]
    [Theory]
    public async Task GetsTestsCreatedByUser(string id) {
        // arrange 
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(id);
        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        
        var actual = await sut.GetTests();
        
        //assert
        Assert.True(actual?.Value?.Count() == _testContext.Tests.Count(x => x.UserId == id));
    }
    
    [Fact]
    public async Task ReturnEmptyListIfNotFound() {
        // arrange 
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");
        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        
        var actual = await sut.GetTests();
        
        //assert
        Assert.True(actual?.Value?.Count() == 0);
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundWhenGetByIdFails() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");

        var sut = new TestController(_testContext, _idProvider, _sharingContext);

        var actual = await sut.GetTest(-1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    
    //get{id}
    [Fact]
    public async Task ReturnsTestWithCorrectId() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("1");

        var sut = new TestController(_testContext, _idProvider, _sharingContext);

        var actual = await sut.GetTest(1);

        Assert.True(actual.Value?.TestId == 1);
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundWhenSearchingForAnotherUsersTest() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");

        var sut = new TestController(_testContext, _idProvider, _sharingContext);

        var actual = await sut.GetTest(1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    
    [Fact]
    public async Task ReturnsNotFoundWhenSearchingForANonexistentTest() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("1");

        var sut = new TestController(_testContext, _idProvider, _sharingContext);

        var actual = await sut.GetTest(-1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    //put{id}
    [Fact]
    public async Task UserCanUpdateOwnTests() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
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

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId == userId);
        test.TestId = -1;
        
        var actual = await sut.PutTest(test.TestId, test);
        
        Assert.IsAssignableFrom<NotFoundResult>(actual);
    }
    
    [Fact]
    public async Task CannotEditAnotherUsersTest() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId != userId);
        
        var actual = await sut.PutTest(test.TestId, test);
        
        Assert.IsAssignableFrom<NotFoundResult>(actual);
    }
    
    // add and remove problems with PUT
    
    [Fact]
    public async Task MismatchedIdsReturnBadRequest() {
        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _tests[0];
        
        var actual = await sut.PutTest(-1, test);
        
        Assert.IsAssignableFrom<BadRequestResult>(actual);
    }
    
    [Fact]
    public async Task DefaultIdReturnsBadRequest() {
        var sut = new TestController(_testContext, _idProvider, _sharingContext);
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

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(_ => true);
        var fakeId = Guid.NewGuid().ToString();
        test.UserId = fakeId;
        
        var actual = await sut.PostTest(test.ToCreationDto());

        Assert.IsAssignableFrom<CreatedAtActionResult>(actual.Result);
        Assert.Null(_testContext.Tests.FirstOrDefault(x=>x.UserId == fakeId));
    }
    //delete{id}
    [Fact]
    public async Task CanDeleteOwnTests() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId == userId);

        var actual = await sut.DeleteTest(test.TestId);

        Assert.IsAssignableFrom<NoContentResult>(actual);
    }
    
    [Fact]
    public async Task CannotDeleteOtherTests() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(x=>x.UserId!=userId);

        var actual = await sut.DeleteTest(test.TestId);

        Assert.IsAssignableFrom<NotFoundResult>(actual);
    }

    [Fact]
    public async Task CanDeleteSharedTest() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new TestController(_testContext, _idProvider, _sharingContext);
        var test = _testContext.Tests.AsNoTracking().First(x => x.UserId == userId);

        var sharedTest = new SharedTest() { TestId = test.TestId, Shared = true, OwnerId = userId };
        await _sharingContext.TestShareInfo.AddAsync(sharedTest);
        var userShare = new TestShare() { SharedTest = sharedTest, AllowedUserId = userId, Id = Guid.NewGuid()};
        await _sharingContext.TestShares.AddAsync(userShare);
        await _sharingContext.SaveChangesAsync();
        
        var actual = await sut.DeleteTest(test.TestId);

        Assert.IsAssignableFrom<NoContentResult>(actual);
        Assert.True(!_sharingContext.TestShareInfo.Any(x => x.TestId == sharedTest.TestId));
        Assert.True(!_sharingContext.TestShares.Any(x=>x.Id==userShare.Id));
    }

}