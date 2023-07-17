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
    private readonly TestMapper _testMapper;

    public TestController(TestAdministrationContext testContext, TestMapper testMapper) {
        _testContext = testContext;
        _testMapper = testMapper;
    }

    // GET: api/Tests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Test>>> GetTests(CancellationToken cancellationToken = default) {
        if (_testContext?.Tests == null)
            return NotFound();

        var tests = _testContext.Tests.Where(x=> x.UserId == GetCurrentUserId())
                                .Include(x=>x.Problems);

        if (tests.Any())
            return await tests.ToListAsync(cancellationToken);

        return Array.Empty<Test>();
    }

    // GET: api/Tests/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetTest(int id, CancellationToken cancellationToken = default) {
        if (_testContext?.Tests == null)
            return NotFound();
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

        var orig = await _testContext.Tests.FirstOrDefaultAsync(x=> x.TestId==test.TestId && 
                                                                   x.UserId==GetCurrentUserId(), cancellationToken: cancellationToken);
        if (orig == null)
            return NotFound();

        test.LastUpdated = DateTime.UtcNow;
        _testContext.Entry(orig).CurrentValues.SetValues(test);
        
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
        if (_testContext?.Tests == null)
            return Problem("Entity set 'TestAdministrationContext.Tests'  is null.");

        testCreation.UserId = GetCurrentUserId();
        testCreation.LastUpdated = testCreation.Created = DateTime.UtcNow;
        
        await _testContext.Tests.AddAsync(_testMapper.DtoToEntity(testCreation), cancellationToken);
        await _testContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction("GetTest", new { id = testCreation.TestId }, testCreation);
    }

    // DELETE: api/Tests/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTest(int id, CancellationToken cancellationToken = default) {
        if (_testContext?.Tests == null)
            return NotFound();
        var test = await _testContext.Tests.FirstOrDefaultAsync(x=> x.TestId==id 
                                                                    && x.UserId == GetCurrentUserId(), cancellationToken: cancellationToken);
        if (test == null)
            return NotFound();

        _testContext.Tests.Remove(test);
        await _testContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private bool TestExists(int id) {
        return (_testContext.Tests?.Any(e => e.TestId == id)).GetValueOrDefault();
    }
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}