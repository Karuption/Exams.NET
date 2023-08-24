using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers.Modification; 

[Route("api/admin/[controller]")]
[ApiController]
[Authorize]
public class QuestionController : ControllerBase {
    private readonly TestAdministrationContext _context;

    public QuestionController(TestAdministrationContext context) {
        _context = context;
    }

    // GET: api/Question
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetTestQuestions() {
        return await _context.TestQuestions
                .Where(x => x.CreatedBy == GetCurrentUserId())
                .ToListAsync();
    }
    
    [HttpGet]
    [Route("Unassigned")]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetUnassignedTestQuestions() {
        return await _context.TestQuestions
                             .Where(x => x.CreatedBy == GetCurrentUserId() && default == (x.TestId ?? default))
                             .ToListAsync();
    }

    // GET: api/Question/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TestQuestion>> GetTestQuestion(int id) {
        var question = await _context.TestQuestions.FirstOrDefaultAsync(x=>x.TestQuestionId == id && x.CreatedBy == GetCurrentUserId());
        if (question is null) 
            return NotFound();

        return question;
    }
    
    [HttpPut]
    [Route("MultipleChoice")]
    public async Task<IActionResult> PutTestQuestion([FromBody]MultipleChoiceProblem testQuestion) {
        return await UpdateQuestion(testQuestion);
    }
    
    [HttpPut]
    [Route("FreeForm")]
    public async Task<IActionResult> PutTestQuestion([FromBody]FreeFormProblem testQuestion) {
        return await UpdateQuestion(testQuestion);
    }

    // POST: api/Question
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Route("MultipleChoice")]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(MultipleChoiceProblemDto testQuestion) {
        testQuestion.TestQuestionId = default;
        testQuestion.CreatedBy = GetCurrentUserId();

         var mapped = testQuestion.ToEntity();

         foreach (var choice in mapped?.Choices ?? Array.Empty<Choice>()) {
             choice.TestQuestion = mapped!;
         }
        
        _context.MultipleChoiceQuestions.Add(mapped!);
        
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }
    
    [HttpPost]
    [Route("FreeForm")]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(FreeFormProblemDto testQuestion) {
        testQuestion.TestQuestionId = default;
        testQuestion.CreatedBy = GetCurrentUserId();

        var ffp = testQuestion.ToEntity();
        if (ffp is null)
            return BadRequest();
        
        _context.FreeFormQuestions.Add(ffp);
        try {
            await _context.SaveChangesAsync();
        }
        catch {
            BadRequest();
        }

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }

    // DELETE: api/Question/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTestQuestion(int id) {
        var testQuestion = await _context.TestQuestions.FindAsync(id);
        if (testQuestion == null)
            return NotFound();

        _context.TestQuestions.Remove(testQuestion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult> UpdateQuestion(TestQuestion testQuestion) {
        if (testQuestion.TestQuestionId == default)
            return BadRequest();
        
        var orig = await _context.TestQuestions.FirstOrDefaultAsync(x=> x.TestQuestionId==testQuestion.TestQuestionId);
        if (orig is null)
            return NotFound();
        
        _context.Entry(orig).CurrentValues.SetValues(testQuestion);

        try { 
            await _context.SaveChangesAsync();
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
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}