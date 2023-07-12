using System.Security.Claims;
using Duende.IdentityServer;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers.Modification; 


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
    public async Task<ActionResult<IEnumerable<Test>>> GetTests() {
        if (_testContext?.Tests == null)
            return NotFound();

        var tests = _testContext.Tests;

        if (tests.Any())
            return await tests.ToListAsync();

        return Array.Empty<Test>();
    }

    // GET: api/Tests/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetTest(int id) {
        if (_testContext.Tests == null)
            return NotFound();
        var test = await _testContext.Tests.FindAsync(id);

        if (test == null)
            return NotFound();

        return test;
    }

    // PUT: api/Tests/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTest(int id, Test test) {
        if (id != test.TestId)
            return BadRequest();

        _testContext.Entry(test).State = EntityState.Modified;

        try {
            await _testContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) {
            if (!TestExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }
    
    [HttpPut]
    public async Task<IActionResult> PutTest(Test test) {
        return await PutTest(test.TestId, test);
    }

    // POST: api/Tests
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Test>> PostTest(TestDto test) {
        if (_testContext.Tests == null)
            return Problem("Entity set 'TestAdministrationContext.Tests'  is null.");
        
        test.UserId = HttpContext.User.Claims.FirstOrDefault(x=>x.Type==ClaimTypes.NameIdentifier)!.Value;
        test.LastUpdated = test.Created = DateTime.UtcNow;
        
        _testContext.Tests.Add(_testMapper.DtoToEntity(test));
        await _testContext.SaveChangesAsync();

        return CreatedAtAction("GetTest", new { id = test.TestId }, test);
    }

    // DELETE: api/Tests/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTest(int id) {
        if (_testContext.Tests == null)
            return NotFound();
        var test = await _testContext.Tests.FindAsync(id);
        if (test == null)
            return NotFound();

        _testContext.Tests.Remove(test);
        await _testContext.SaveChangesAsync();

        return NoContent();
    }

    private bool TestExists(int id) {
        return (_testContext.Tests?.Any(e => e.TestId == id)).GetValueOrDefault();
    }
}