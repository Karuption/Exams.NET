using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;

namespace Exams.NET.Controllers; 

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserAnswerController : Controller {
    private readonly TestAdministrationContext _context;
    public UserAnswerController(TestAdministrationContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserTestQuestionAnswer>>> GetAnswers() {
        return await _context.UserAnswers.Where(x => x.UserId == GetCurrentUserId())
                       .ToListAsync();
    }
    
    [HttpGet]
    [Route("{id:guid}")]
    public async Task<ActionResult<UserTestQuestionAnswer>> GetAnswer(Guid id) {
        return await _context.UserAnswers.FirstAsync(x => x.UserId == GetCurrentUserId()
                            && x.Id == id);
    }
    
    [HttpGet]
    [Route("{TestId:int}")]
    public async Task<ActionResult<IEnumerable<UserTestQuestionAnswer>>> GetAnswers(int TestId) {
        var test = await _context.Tests.AsNoTracking()
                                 .Include(test => test.Problems)
                                 .FirstOrDefaultAsync(x => x.TestId == TestId);
        if (test is null) 
            return BadRequest();

        var testQuestionAnswers = new List<UserTestQuestionAnswer>(test.Problems?.Count ?? 0);
        foreach (var question in test.Problems??Array.Empty<TestQuestion>()) {
            var questionUserAnswer = await _context.UserAnswers
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(x => x.QuestionId == question.TestQuestionId)
                                     ?? new UserTestQuestionAnswer(){QuestionId = question.TestQuestionId,UserId = GetCurrentUserId(), Answer = ""};
            
            testQuestionAnswers.Add(questionUserAnswer);
        }

        return testQuestionAnswers;
    }
    
    [HttpPut]
    public async Task<ActionResult> PutAnswer(Guid id, UserTestQuestionAnswer userTestQuestionAnswer) {
        if (id == default || id != userTestQuestionAnswer.Id)
            return BadRequest();

        var orig = _context.UserAnswers.FirstAsync(x => x.Id == id);
        
        _context.Entry(orig).CurrentValues.SetValues(userTestQuestionAnswer);

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException err) {
            if (!_context.UserAnswers.Any(x => x.Id == id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<UserTestQuestionAnswer>> PostUserAnswer(
        UserTestQuestionAnswerCreationDto userTestQuestionAnswerCreationDto) {
        if ((userTestQuestionAnswerCreationDto.Id ?? default) != default || 
                userTestQuestionAnswerCreationDto.QuestionId == default)
            return BadRequest();

        userTestQuestionAnswerCreationDto.UserId = GetCurrentUserId();
        var userTestQuestionAnswer = userTestQuestionAnswerCreationDto.ToEntity();

        await _context.AddAsync(userTestQuestionAnswer);

        try {
            await _context.SaveChangesAsync();
        }
        catch {
            return BadRequest();
        }

        return CreatedAtAction("GetAnswer", new { id = userTestQuestionAnswer.Id }, userTestQuestionAnswer);
    }
    
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }

}