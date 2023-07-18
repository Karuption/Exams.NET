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
    private readonly TestMapper _mapper;

    public QuestionController(TestAdministrationContext context, TestMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Question
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestQuestion>>> GetTestQuestions() {
        if (_context?.TestQuestions == null)
            return NotFound();
        
        return await _context.TestQuestions.Where(x => x.CreatedBy == GetCurrentUserId()).ToListAsync();
    }

    // GET: api/Question/5
    [HttpGet("{id}")]
    public async Task<ActionResult<TestQuestion>> GetTestQuestion(int id) {
        if (_context?.TestQuestions == null)
            return NotFound();
        
        var testQuestion = await _context.TestQuestions.FirstOrDefaultAsync(x=>x.TestQuestionId == id && x.CreatedBy == GetCurrentUserId());
        if (testQuestion is null)
            return NotFound();

        return testQuestion;
    }

    // PUT: api/Question/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTestQuestion(int id, TestQuestion testQuestion) {
        if (_context?.TestQuestions is null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        if (id == default || testQuestion == null || id != (testQuestion?.TestQuestionId ?? default))
            return BadRequest();
        
        var orig = await _context.Tests.FirstOrDefaultAsync(x=> x.TestId==testQuestion!.TestQuestionId);
        if (orig == null)
            return NotFound();

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
    public async Task<ActionResult<TestQuestion>> PostTestQuestion(MultipleChoiceProblemDto testQuestion) {
        if (_context?.TestQuestions == null)
            return Problem("Entity set 'TestAdministrationContext.TestQuestions'  is null.");
        
        testQuestion.TestQuestionId = default;
        testQuestion.CreatedBy = GetCurrentUserId();

        var mapped = _mapper.QuestionDtoToEntity(testQuestion);

        foreach (var choice in mapped.Choices) {
            choice.TestQuestion = mapped;
        }
        
        _context.TestQuestions.Add(mapped);
        
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTestQuestion", new { id = testQuestion.TestQuestionId }, testQuestion);
    }
    
    

    // DELETE: api/Question/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTestQuestion(int id) {
        if (_context?.TestQuestions == null)
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
    
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}