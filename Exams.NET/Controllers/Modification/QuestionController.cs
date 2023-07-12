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
        if (_context.TestQuestions == null)
            return NotFound();
        
        return await _context.TestQuestions.ToListAsync();
    }

    // GET: api/Question/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TestQuestion>> GetTestQuestion(int id) {
        if (_context.TestQuestions == null)
            return NotFound();
        
        var testQuestion = await _context.TestQuestions.FindAsync(id);

        if (testQuestion == null)
            return NotFound();

        return testQuestion;
    }

    // PUT: api/Question/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTestQuestion(int id, MultipleChoiceProblem testQuestion) {
        if (id != testQuestion.TestQuestionId)
            return BadRequest();

        _context.Entry(testQuestion).State = EntityState.Modified;

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
    
    // PUT: api/Question/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTestQuestion(int id, FreeFormProblem testQuestion) {
        if (id != testQuestion.TestQuestionId)
            return BadRequest();

        _context.Entry(testQuestion).State = EntityState.Modified;

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
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(FreeFormProblem testQuestion) {
        if (_context.TestQuestions == null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        testQuestion.TestQuestionId = default;
        _context.TestQuestions.Add(testQuestion);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }
    
    // POST: api/Question
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(MultipleChoiceProblem testQuestion) {
        if (_context.TestQuestions == null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        testQuestion.TestQuestionId = default;
        _context.TestQuestions.Add(testQuestion);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }

    // DELETE: api/Question/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTestQuestion(int id) {
        if (_context.TestQuestions == null)
            return NotFound();
        var testQuestion = await _context.TestQuestions.FindAsync(id);
        if (testQuestion == null)
            return NotFound();

        _context.TestQuestions.Remove(testQuestion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TestQuestionExists(int id) {
        return (_context.TestQuestions?.Any(e => e.TestQuestionId == id)).GetValueOrDefault();
    }
}