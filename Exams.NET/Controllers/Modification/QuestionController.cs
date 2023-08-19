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
        if (_context?.MultipleChoiceQuestions is null || _context?.FreeFormQuestions is null)
            return await Task.FromResult<ActionResult<IEnumerable<TestQuestion>>>(NotFound());


        return await _context.TestQuestions
                .Where(x => x.CreatedBy == GetCurrentUserId())
                .ToListAsync();
    }
    
    [HttpGet]
    [Route("Unassigned")]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetUnassignedTestQuestions() {
        if (_context?.MultipleChoiceQuestions is null || _context?.FreeFormQuestions is null)
            return await Task.FromResult<ActionResult<IEnumerable<TestQuestion>>>(NotFound());


        return await _context.TestQuestions
                             .Where(x => x.CreatedBy == GetCurrentUserId() && default == (x.TestId ?? default))
                             .ToListAsync();
    }

    // GET: api/Question/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TestQuestion>> GetTestQuestion(int id) {
        if (_context?.MultipleChoiceQuestions is null)
            return NotFound();
        
        var question = await _context.TestQuestions.FirstOrDefaultAsync(x=>x.TestQuestionId == id && x.CreatedBy == GetCurrentUserId());
        if (question is null) 
            return NotFound();

        return question;
    }
    
    [HttpPut("{id}")]
    [Route("MultipleChoice")]
    public async Task<IActionResult> PutTestQuestion(int id, MultipleChoiceProblem testQuestion) {
        if (_context?.MultipleChoiceQuestions is null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        if (id == default || id != (testQuestion?.TestQuestionId ?? default))
            return BadRequest();
        
        var orig = await _context.Tests.FirstOrDefaultAsync(x=> x.TestId==testQuestion!.TestQuestionId);
        if (orig is null)
            return NotFound();
        
        orig.LastUpdated = DateTime.UtcNow;
        _context.Entry(orig).CurrentValues.SetValues(testQuestion!);

        try { 
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) {
            if (!TestQuestionExists(id))
                return NotFound();
            throw;
        }
        
        return NoContent();
    }
    
    [HttpPut("{id}")]
    [Route("FreeForm")]
    public async Task<IActionResult> PutTestQuestion(int id, FreeFormProblem testQuestion) {
        if (_context?.MultipleChoiceQuestions is null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        if (id == default || id != (testQuestion?.TestQuestionId ?? default))
            return BadRequest();
        
        var orig = await _context.Tests.FirstOrDefaultAsync(x=> x.TestId==testQuestion!.TestQuestionId);
        if (orig is null)
            return NotFound();
        
        orig.LastUpdated = DateTime.UtcNow;
        _context.Entry(orig).CurrentValues.SetValues(testQuestion!);

        try { 
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) {
            if (!TestQuestionExists(id))
                return NotFound();
            throw;
        }
        
        return NoContent();
    }

    // POST: api/Question
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Route("MultipleChoice")]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(MultipleChoiceProblemDto testQuestion) {
        if (_context?.MultipleChoiceQuestions is null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        
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
        if (_context?.MultipleChoiceQuestions is null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        
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
        if (_context?.MultipleChoiceQuestions == null)
            return NotFound();
        
        var testQuestion = await _context.MultipleChoiceQuestions.FindAsync(id);
        if (testQuestion == null)
            return NotFound();

        _context.MultipleChoiceQuestions.Remove(testQuestion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TestQuestionExists(int id) {
        return (_context.TestQuestions?.Any(e => e.TestQuestionId == id)).GetValueOrDefault();
    }
    
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}