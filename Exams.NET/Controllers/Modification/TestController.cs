using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers.Modification;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class TestController : ControllerBase {
    private readonly TestAdministrationContext _testContext;

    public TestController(TestAdministrationContext testContext) {
        _testContext = testContext;
    }

    // GET: api/Tests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Test>>> GetTests(CancellationToken cancellationToken = default) {
        var tests = _testContext.Tests.Where(x=> x.UserId == GetCurrentUserId())
                                .Include(x=>x.Problems);

        if (tests.Any())
            return await tests.ToListAsync(cancellationToken);

        return Array.Empty<Test>();
    }

    // GET: api/Tests/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetTest(int id, CancellationToken cancellationToken = default) {
        var test = await _testContext.Tests.Include(x=>x.Problems)
                                     .FirstOrDefaultAsync(x=>x.UserId == GetCurrentUserId() && x.TestId == id, cancellationToken: cancellationToken);

        if (test == null)
            return NotFound();

        return test;
    }

    // PUT: api/Tests/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTest(int id, Test test, CancellationToken cancellationToken = default) {
        if (id != test.TestId)
            return BadRequest();

        var orig = await _testContext.Tests.Include(x=>x.Problems).FirstOrDefaultAsync(x=> x.TestId==test.TestId && 
                                                                   x.UserId==GetCurrentUserId(), cancellationToken: cancellationToken);
        if (orig == null)
            return NotFound();

        test.LastUpdated = DateTime.UtcNow;
        _testContext.Entry(orig).CurrentValues.SetValues(test);
        
        var problemsToRemove = orig.Problems?.Where(problem => test.Problems!.Any(updatedProblem => updatedProblem.TestQuestionId == problem.TestQuestionId)).ToList();
    
        foreach (var problemToRemove in problemsToRemove!)
        {
            var trackedProblem = await _testContext.TestQuestions.FirstOrDefaultAsync(x => x.TestQuestionId == problemToRemove.TestQuestionId, cancellationToken: cancellationToken);
            if(trackedProblem is null)
                continue;
            trackedProblem.Test = null;
            trackedProblem.TestId = null;
            _testContext.Entry(trackedProblem).State = EntityState.Modified;
            orig.Problems?.Remove(trackedProblem);
        }

        foreach (var updatedProblem in test.Problems!) {
            var existingProblem = orig.Problems?.FirstOrDefault(problem => problem.TestQuestionId == updatedProblem.TestQuestionId);
            if (existingProblem is null) {
                var trackedProblem = await _testContext.TestQuestions.FirstOrDefaultAsync(x => x.TestQuestionId == updatedProblem.TestQuestionId, cancellationToken: cancellationToken) ?? updatedProblem;
                trackedProblem.TestId = orig.TestId;
                trackedProblem.Test = orig;
                _testContext.Entry(trackedProblem).State = EntityState.Modified;
                orig.Problems?.Add(trackedProblem);
            } else
                _testContext.Entry(existingProblem).CurrentValues.SetValues(updatedProblem);
        }

        
        try {
            await _testContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) {
            if (!TestExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }
    
    [HttpPut]
    public async Task<IActionResult> PutTest(Test test, CancellationToken cancellationToken = default) {
        return await PutTest(test.TestId, test, cancellationToken);
    }

    // POST: api/Tests
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Test>> PostTest(TestCreationDto testCreation, CancellationToken cancellationToken = default) {
        testCreation.UserId = GetCurrentUserId();
        testCreation.LastUpdated = testCreation.Created = DateTime.UtcNow;
        
        await _testContext.Tests.AddAsync(testCreation.ToEntity(), cancellationToken);
        await _testContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction("GetTest", new { id = testCreation.TestId }, testCreation);
    }

    // DELETE: api/Tests/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTest(int id, CancellationToken cancellationToken = default) {
        var test = await _testContext.Tests.FirstOrDefaultAsync(x=> x.TestId==id 
                                                                    && x.UserId == GetCurrentUserId(), cancellationToken: cancellationToken);
        if (test == null)
            return NotFound();

        _testContext.Tests.Remove(test);
        await _testContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private bool TestExists(int id) {
        return _testContext.Tests.Any(e => e.TestId == id);
    }
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}