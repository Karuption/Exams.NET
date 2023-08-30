using System.ComponentModel.Design;
using System.Net;
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
        Assert.True(actual?.Value?.Count() == _tests.Count(x => x.UserId == id));
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

        var actual = sut.GetTest(-1);

        Assert.IsAssignableFrom<NotFoundResult>(actual?.Result?.Result);
    }
    
    //get{id}
    [Fact]
    public async Task ReturnsTestWithCorrectId() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("1");

        var sut = new TestController(_testContext, _idProvider);

        var actual = sut.GetTest(1);

        Assert.True(actual.Result?.Value?.TestId == 1);
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundWhenSearchingForAnotherUsersTest() {
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("-");

        var sut = new TestController(_testContext, _idProvider);

        var actual = sut.GetTest(1);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result?.Result);
    }
    //put{id}
    //put
    //post
    //delete{id}
    
}