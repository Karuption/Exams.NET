using Exams.NET.Controllers.Modification;
using Exams.NET.Data;
using Exams.NET.Models;
using Exams.NET.Providers;
using Exams.NET.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Exams.NET.Tests.ControllerTests.AdminControllers; 

public class Question {
    private readonly IUserIdProvider _idProvider = Substitute.For<IUserIdProvider>();
    private readonly TestAdministrationContext _testContext;
    public Question() {
        _testContext = EfHelpers.ContextSetup<TestAdministrationContext>();
        _testContext.TestQuestions.AddRange(_testQuestions);
        _testContext.SaveChanges();
    }

    private List<TestQuestion> _testQuestions = new() {
        new MultipleChoiceProblem() { TestQuestionId = 1, CreatedBy = "1" },
        new FreeFormProblem() { TestQuestionId = 2, CreatedBy = "2" },
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
    //post
    [Fact]
    public async Task FreeFormQuestionAddedOnPost() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var count = _testContext.TestQuestions.Count();
        var sut = new QuestionController(_testContext, _idProvider);

        await sut.PostTestQuestion(new FreeFormProblemDto() {});

        Assert.True(_testContext.TestQuestions.Count() == count + 1);
    }
    [Fact]
    public async Task MultipleChoiceQuestionAddedOnPost() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var count = _testContext.TestQuestions.Count();
        var sut = new QuestionController(_testContext, _idProvider);

        await sut.PostTestQuestion(new MultipleChoiceProblemDto() {});

        Assert.True(_testContext.TestQuestions.Count() == count + 1);
    }
    [Fact]
    public async Task MultipleChoiceQuestionWithChoicesAdded() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var count = _testContext.TestQuestions.Count();
        var sut = new QuestionController(_testContext, _idProvider);

        var mcq = new MultipleChoiceProblemDto()
            { 
                Prompt = Guid.NewGuid().ToString(),
                Choices = new List<ChoiceDto>() { 
                    new () { 
                        Description = "null"
                    }, new () {
                        Description = "null"
                    }, new ChoiceDto {
                        Description = "null"
                    }
                }
            };
        
        await sut.PostTestQuestion(mcq);

        var actual = await _testContext.TestQuestions.FirstAsync(x => x.Prompt == mcq.Prompt);

        Assert.IsAssignableFrom<MultipleChoiceProblem>(actual);
        Assert.True(((MultipleChoiceProblem)actual)?.Choices?.Count == mcq.Choices.Count);
    }
    //put{id} Multi and free
    [Fact]
    public async Task IdDefaultErrors() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);
        
        var sut = new QuestionController(_testContext, _idProvider);
        var ffp = await sut.PutTestQuestion(new FreeFormProblem());
        var mcq = await sut.PutTestQuestion(new MultipleChoiceProblem());

        Assert.IsAssignableFrom<BadRequestResult>(ffp);
        Assert.IsAssignableFrom<BadRequestResult>(mcq);
    }

    [Fact]
    public async Task BadTestQuestionIdErrors() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new QuestionController(_testContext, _idProvider);
        var ffp = await sut.PutTestQuestion(new FreeFormProblem() { TestQuestionId = -1 });
        var mcq = await sut.PutTestQuestion(new MultipleChoiceProblem(){TestQuestionId = -1});

        Assert.IsAssignableFrom<NotFoundResult>(ffp);
        Assert.IsAssignableFrom<NotFoundResult>(mcq);
    }
    
    [Fact]
    public async Task BadUserIdErrors() {
        var userId = "-";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);

        var sut = new QuestionController(_testContext, _idProvider);
        var mcq = await _testContext.MultipleChoiceQuestions.FirstAsync();
        var ffp = await _testContext.FreeFormQuestions.FirstAsync();
        
        var ffpResult = await sut.PutTestQuestion(ffp);
        var mcqResult = await sut.PutTestQuestion(mcq);

        Assert.IsAssignableFrom<NotFoundResult>(ffpResult);
        Assert.IsAssignableFrom<NotFoundResult>(mcqResult);
    }
    
    [Fact]
    public async Task TestsUpdated() {
        var mcq = await _testContext.MultipleChoiceQuestions.FirstAsync();
        var ffp = await _testContext.MultipleChoiceQuestions.FirstAsync();

        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(mcq.CreatedBy, ffp.CreatedBy);
        
        var sut = new QuestionController(_testContext, _idProvider);
        
        var mcqResult = await sut.PutTestQuestion(mcq);
        var ffpResult = await sut.PutTestQuestion(ffp);

        Assert.IsAssignableFrom<NoContentResult>(mcqResult);
        Assert.IsAssignableFrom<NoContentResult>(ffpResult);
    }
    //delete{id}
    [Fact]
    public async Task CanDeleteOwnTest() {
        var mcq = await _testContext.MultipleChoiceQuestions.FirstAsync();
        var ffp = await _testContext.FreeFormQuestions.FirstAsync();
        var totalCount = await _testContext.TestQuestions.CountAsync();

        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(mcq.CreatedBy, ffp.CreatedBy);
        
        var sut = new QuestionController(_testContext, _idProvider);
        
        var mcqResult = await sut.DeleteTestQuestion(mcq.TestQuestionId);
        var ffpResult = await sut.DeleteTestQuestion(ffp.TestQuestionId);

        Assert.IsAssignableFrom<NoContentResult>(mcqResult);
        Assert.IsAssignableFrom<NoContentResult>(ffpResult);
        
        Assert.Equal(totalCount - 2, _testContext.TestQuestions.Count());
    }
    
    [Fact]
    public async Task BadQuestionIdReturnsNotFound() {
        var userId = "1";
        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns(userId);
        
        var sut = new QuestionController(_testContext, _idProvider);
        
        var mcqResult = await sut.DeleteTestQuestion(-1);
        var ffpResult = await sut.DeleteTestQuestion(-1);

        Assert.IsAssignableFrom<NotFoundResult>(mcqResult);
        Assert.IsAssignableFrom<NotFoundResult>(ffpResult);
    }
    
    [Fact]
    public async Task BadUserIdReturnsNotFound() {
        var mcq = await _testContext.MultipleChoiceQuestions.FirstAsync();
        var ffp = await _testContext.FreeFormQuestions.FirstAsync();

        _idProvider.GetCurrentUserId(Arg.Any<HttpContext>()).Returns("_");
        
        var sut = new QuestionController(_testContext, _idProvider);
        
        var mcqResult = await sut.DeleteTestQuestion(mcq.TestQuestionId);
        var ffpResult = await sut.DeleteTestQuestion(ffp.TestQuestionId);

        Assert.IsAssignableFrom<NotFoundResult>(mcqResult);
        Assert.IsAssignableFrom<NotFoundResult>(ffpResult);
    }
}