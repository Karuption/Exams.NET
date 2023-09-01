using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Exams.NET.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers.Modification; 

/// <summary>
/// This is a controller for managing test questions
/// Route: /api/admin/question
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class QuestionController : ControllerBase {
    private readonly TestAdministrationContext _context;
    private readonly IUserIdProvider _idProvider;

    /// <summary>
    /// This controller requires the Db context for managing questions
    /// </summary>
    /// <param name="context">Question management Db context</param>
    /// <param name="idProvider">This is to provide code with the userID. This abstraction is meant to make testing easier</param>
    public QuestionController(TestAdministrationContext context, IUserIdProvider idProvider) {
        _context = context;
        _idProvider = idProvider;
    }
    
    /// <summary>
    /// Gets all the questions that the current user has created
    /// Method: GET
    /// Route: api/question
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with a list of questions</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetTestQuestions(CancellationToken ct = default) {
        return await _context.TestQuestions
                .Where(x => x.CreatedBy == GetCurrentUserId())
                .ToListAsync(cancellationToken: ct);
    }
    
    /// <summary>
    /// Gets all of the test questions that do not have any tests associated with them
    /// Method: GET
    /// Route: api/question/{id}
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with a list of questions</returns>
    [HttpGet]
    [Route("Unassigned")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetUnassignedTestQuestions(CancellationToken ct = default) {
        return await _context.TestQuestions
                             .Where(x => x.CreatedBy == GetCurrentUserId() && default == (x.TestId ?? default))
                             .ToListAsync(cancellationToken: ct);
    }

    /// <summary>
    /// Get a question by the id.
    /// Method: GET
    /// Route: api/question/{id}
    /// </summary>
    /// <param name="id">Id of the question to get</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK Question with the associated id or not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestQuestion>> GetTestQuestion(int id, CancellationToken ct = default) {
        var question = await _context.TestQuestions.FirstOrDefaultAsync(x=>x.TestQuestionId == id && x.CreatedBy == GetCurrentUserId(), cancellationToken: ct);
        if (question is null) 
            return NotFound();

        return question;
    }


    /// <summary>
    /// Updates a multiple choice question.
    /// Method: PUT
    /// Route: api/question/MultipleChoice
    /// </summary>
    /// <param name="testQuestion">Question to update</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>201 created with location and question JSON or 500 Internal Server Error</returns>
    [HttpPut]
    [Route("MultipleChoice")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutTestQuestion([FromBody]MultipleChoiceProblem testQuestion, CancellationToken ct = default) {
        return await UpdateQuestion(testQuestion, ct);
    }

    /// <summary>
    /// Updates a multiple choice question.
    /// Method: PUT
    /// Route: api/question/FreeForm
    /// </summary>
    /// <param name="testQuestion">Question to update</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>201 created with location and question JSON or 500 Internal Server Error</returns>
    [HttpPut]
    [Route("FreeForm")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutTestQuestion([FromBody]FreeFormProblem testQuestion, CancellationToken ct = default) {
        return await UpdateQuestion(testQuestion, ct);
    }
    
    /// <summary>
    /// Creates a multiple choice question
    /// Method: POST
    /// Route: api/question/MultipleChoice
    /// </summary>
    /// <param name="testQuestion">Test question to create</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>201 created with location and JSON question or Internal Server Error</returns>
    [HttpPost]
    [Route("MultipleChoice")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(MultipleChoiceProblemDto testQuestion, CancellationToken ct = default) {
        testQuestion.TestQuestionId = default;
        testQuestion.CreatedBy = GetCurrentUserId();

         var mapped = testQuestion.ToEntity();

         foreach (var choice in mapped?.Choices ?? Array.Empty<Choice>()) {
             choice.TestQuestion = mapped!;
         }
        
        _context.MultipleChoiceQuestions.Add(mapped!);

        try {
            await _context.SaveChangesAsync(ct);
        }
        catch  {
            return Problem();
        }

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }
    
    /// <summary>
    /// Creates a Free Form question
    /// Method: POST
    /// Route: api/question/FreeForm
    /// </summary>
    /// <param name="testQuestion">Test question to create</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>201 created with location and JSON question, bad request or Internal Server Error</returns>
    [HttpPost]
    [Route("FreeForm")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(FreeFormProblemDto testQuestion, CancellationToken ct = default) {
        testQuestion.TestQuestionId = default;
        testQuestion.CreatedBy = GetCurrentUserId();

        var ffp = testQuestion.ToEntity();
        if (ffp is null)
            return BadRequest();
        
        _context.FreeFormQuestions.Add(ffp);
        try {
            await _context.SaveChangesAsync(ct);
        }
        catch {
            Problem();
        }

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }
    
    /// <summary>
    /// Deletes the question with {id}
    /// Method: DELETE
    /// Route: api/question/{id}
    /// </summary>
    /// <param name="id">id of the question to delete</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>No content if it has been deleted, Not found or Internal Error</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTestQuestion(int id, CancellationToken ct = default) {
        var testQuestion = await _context.TestQuestions.FirstOrDefaultAsync(x=>x.TestQuestionId == id && x.CreatedBy == GetCurrentUserId(), ct);
        if (testQuestion == null)
            return NotFound();

        _context.TestQuestions.Remove(testQuestion);
        try {
            await _context.SaveChangesAsync(ct);
        }
        catch {
            Problem();
        }

        return NoContent();
    }

    private async Task<IActionResult> UpdateQuestion(TestQuestion testQuestion,CancellationToken ct = default) {
        if (testQuestion.TestQuestionId == default)
            return BadRequest();
        
        var orig = await _context.TestQuestions.FirstOrDefaultAsync(x=> x.TestQuestionId==testQuestion.TestQuestionId && 
                                                                        x.CreatedBy == GetCurrentUserId(), cancellationToken: ct);
        if (orig is null)
            return NotFound();
        
        _context.Entry(orig).CurrentValues.SetValues(testQuestion);

        try { 
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException) {
            if (!TestQuestionExists(testQuestion.TestQuestionId))
                return NotFound();
            throw;
        }
        
        return NoContent();
    }

    private bool TestQuestionExists(int id) {
        return _context.TestQuestions.Any(e => e.TestQuestionId == id);
    }
    
    private string GetCurrentUserId() {
        return _idProvider.GetCurrentUserId(this.HttpContext);
    }
}