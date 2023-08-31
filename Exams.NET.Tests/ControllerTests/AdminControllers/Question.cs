using Exams.NET.Controllers.Modification;
using Exams.NET.Data;
using Exams.NET.Models;
using Exams.NET.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Exams.NET.Tests.ControllerTests.AdminControllers; 

public class Question {
    private readonly IUserIdProvider _idProvider;
    private readonly TestAdministrationContext _testContext;
    public Question() {
        _idProvider = Substitute.For<IUserIdProvider>();

        var options = new DbContextOptionsBuilder<TestAdministrationContext>()
            .UseInMemoryDatabase("fakeDb")
            .EnableSensitiveDataLogging()
            .Options;
        
        _testContext = new TestAdministrationContext(options);
        _testContext.Database.EnsureDeleted();
        _testContext.Database.EnsureCreated();
        _testContext.TestQuestions.AddRange(_testQuestions);
        _testContext.SaveChanges();
    }

    private List<TestQuestion> _testQuestions = new() {
        new TestQuestion() { TestQuestionId = 1, CreatedBy = "1" },
        new() { TestQuestionId = 2, CreatedBy = "2" },
        new() { TestQuestionId = 3, CreatedBy = "1" },
        new() { TestQuestionId = 4, CreatedBy = "3" },
    };
    //get
    [Fact]
    public async Task GetsAllOfYourTasks() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var expected = _testQuestions.Where(x => x.CreatedBy == userId);
        var sut = new QuestionController(_testContext, _idProvider);

        var actual = await sut.GetTestQuestions();
        
        Assert.True(expected.Count() == actual?.Value?.Count());
    }
    [Fact]
    public async Task ReturnsEmptyIfUserIdNotFound() {
        var userId = "-";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var expected = _testQuestions.Where(x => x.CreatedBy == userId);
        var sut = new QuestionController(_testContext, _idProvider);

        var actual = await sut.GetTestQuestions();
        
        Assert.True(expected.Count() == actual?.Value?.Count());
    }
    //get{id}
    [Fact]
    public async Task ReturnsNotFoundIfIdIsntTheirs() {
        var userId = "-";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new QuestionController(_testContext, _idProvider);
        var actual = await sut.GetTestQuestion(1);
        
        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    //get{id}
    [Fact]
    public async Task ReturnsCorrectId() {
        var test = _testContext.TestQuestions.First();
        var userId = test.CreatedBy;
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new QuestionController(_testContext, _idProvider);
        var actual = await sut.GetTestQuestion(test.TestQuestionId);
        
        Assert.Equal(test.TestQuestionId, actual.Value?.TestQuestionId);
    }
    [Fact]
    public async Task LimitQuestionsToTheOwner() {
        var test = _testContext.TestQuestions.First();
        var userId = test.CreatedBy + Guid.NewGuid();
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new QuestionController(_testContext, _idProvider);
        var actual = await sut.GetTestQuestion(test.TestQuestionId);

        Assert.IsAssignableFrom<NotFoundResult>(actual.Result);
    }
    //get/unassigned
    [Fact]
    public async Task GetsAllUnassignedQuestions() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var expected = _testQuestions.Where(x => x.CreatedBy == userId).ToArray();
        var sut = new QuestionController(_testContext, _idProvider);

        var oldQuestions = await sut.GetUnassignedTestQuestions();
        
        Assert.True(expected.Count() == oldQuestions?.Value?.Count());

        await _testContext.Tests.AddAsync(new Models.Test() { UserId = userId });
        await _testContext.TestQuestions.AddAsync(new TestQuestion() { CreatedBy = userId, TestId = 1 });
        await _testContext.SaveChangesAsync();

        var newQuestions = await sut.GetUnassignedTestQuestions();
        
        Assert.True(expected.Length == newQuestions?.Value?.Count());
    }
    
    [Fact]
    public async Task ReturnsEmptyListWhenAllQsAssigned() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);
        
        await _testContext.Tests.AddAsync(new Models.Test() { TestId = 1, UserId = userId });
        var allQuestions = await _testContext.TestQuestions.ToListAsync();
        _ = allQuestions.Select(x => x.TestId = 1).ToList();
        await _testContext.SaveChangesAsync();
        
        var sut = new QuestionController(_testContext, _idProvider);
        var newQuestions = await sut.GetUnassignedTestQuestions();
        
        Assert.True(newQuestions?.Value?.Count() == 0);
    }
    //post multiple choice
    //post free-answer
    //put{id} Multi
    //delete{id}
}