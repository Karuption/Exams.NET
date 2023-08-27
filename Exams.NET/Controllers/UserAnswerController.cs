using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers; 

/// <summary>
/// This controller is used to mutate user answers to test questions.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserAnswerController : Controller {
    private readonly TestAdministrationContext _context;
    /// <summary>
    /// This controller is used to mutate user answers to test questions.
    /// </summary>
    /// <param name="context">Db context with access to tests and user answers</param>
    public UserAnswerController(TestAdministrationContext context) {
        _context = context;
    }

    /// <summary>
    /// Get all of the users answers
    /// Method: GET
    /// Route: api/UserAnswer
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with a list of user's test questions answers</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserTestQuestionAnswer>>> GetAnswers(CancellationToken ct = default) {
        return await _context.UserAnswers.Where(x => x.UserId == GetCurrentUserId())
                       .ToListAsync(cancellationToken: ct);
    }
    
    /// <summary>
    /// Get the user's answer to a specific question.
    /// Method: GET
    /// Route: api/UserAnswer/{id}
    /// </summary>
    /// <param name="id">id of the question to get</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>The user answer for question {id} or not found</returns>
    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserTestQuestionAnswer>> GetAnswer(Guid id,CancellationToken ct = default) {
        var userTestQuestionAnswer = await _context.UserAnswers.FirstOrDefaultAsync(x => x.UserId == GetCurrentUserId()
                            && x.Id == id, cancellationToken: ct);
        if (userTestQuestionAnswer is null)
            return NotFound();
        return userTestQuestionAnswer;
    }
    
    /// <summary>
    /// Get users' answer to all of the questions in a specific test.
    /// Method: Get
    /// Route: api/UserAnswer/{testId}
    /// </summary>
    /// <param name="testId">id of the test to get answers of</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Not found when the test isn't found, 200 OK with a list of users' answers</returns>
    /// <remarks>If the user has not answered the question, the answer and user answer id will be default. This allows for a single call to be able to fill out an entire test with the answers that they have already made and have yet to make.</remarks>
    [HttpGet]
    [Route("{testId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<UserTestQuestionAnswer>>> GetAnswers(int testId, CancellationToken ct = default) {
        var test = await _context.Tests.AsNoTracking()
                                 .Include(test => test.Problems)
                                 .FirstOrDefaultAsync(x => x.TestId == testId, cancellationToken: ct);
        if (test is null) 
            return NotFound();

        var testQuestionAnswers = new List<UserTestQuestionAnswer>(test.Problems?.Count ?? 0);
        foreach (var question in test.Problems??Array.Empty<TestQuestion>()) {
            var questionUserAnswer = await _context.UserAnswers
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(x => x.QuestionId == question.TestQuestionId, cancellationToken: ct)
                                     ?? new UserTestQuestionAnswer(){QuestionId = question.TestQuestionId,UserId = GetCurrentUserId(), Answer = ""};
            
            testQuestionAnswers.Add(questionUserAnswer);
        }

        return testQuestionAnswers;
    }
    
    /// <summary>
    /// Update a user answer by the id of the answer.
    /// Method: PUT
    /// Route: api/UserAnswer/{id}
    /// </summary>
    /// <param name="id">id of the question to update</param>
    /// <param name="userTestQuestionAnswer">Question object to update it with</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>204 No Content, 400 Bad Request, or 404 Not found</returns>
    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PutAnswer(Guid id, UserTestQuestionAnswer userTestQuestionAnswer, CancellationToken ct = default) {
        if (id == default || id != userTestQuestionAnswer.Id || userTestQuestionAnswer.UserId != GetCurrentUserId())
            return BadRequest();

        var orig = await _context.UserAnswers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
        if (orig is null || userTestQuestionAnswer.UserId != orig.UserId)
            return NotFound();
        
        _context.Entry(orig).CurrentValues.SetValues(userTestQuestionAnswer);

        try {
            await _context.SaveChangesAsync(ct);
        }
        catch(DbUpdateConcurrencyException) {
            if (!_context.UserAnswers.Any(x => x.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Create a user answer.
    /// Method: POST
    /// Route: api/UserAnswer
    /// </summary>
    /// <param name="userTestQuestionAnswerCreationDto">Object to create the answer with</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with the object created, bad request or internal server error</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserTestQuestionAnswer>> PostUserAnswer(
        [FromBody] UserTestQuestionAnswerCreationDto userTestQuestionAnswerCreationDto, CancellationToken ct = default) {
        if ((userTestQuestionAnswerCreationDto.Id ?? default) != default || 
                userTestQuestionAnswerCreationDto.QuestionId == default)
            return BadRequest();

        userTestQuestionAnswerCreationDto.UserId = GetCurrentUserId();
        var userTestQuestionAnswer = userTestQuestionAnswerCreationDto.ToEntity();
        if (userTestQuestionAnswer is null)
            return BadRequest();

        await _context.UserAnswers.AddAsync(userTestQuestionAnswer, ct);

        try {
            await _context.SaveChangesAsync(ct);
        }
        catch {
            return Problem();
        }

        return CreatedAtAction("GetAnswer", new { id = userTestQuestionAnswer.Id }, userTestQuestionAnswer);
    }
    
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }

}