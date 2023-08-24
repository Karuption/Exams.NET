using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers; 

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TestController : ControllerBase {
    private readonly TestAdministrationContext _context;

    public TestController(TestAdministrationContext context) {
        _context = context;
    }

    // GET: api/Test
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserTest>>> GetTests() {
        return await _context.Tests
                            .Select(x=>x.ToUser())
                            .ToListAsync();
    }

    // GET: api/Test/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserTest>> GetTest(int id) {
        var test = await _context.Tests
                                 .Include(x=>x.Problems)
                                 .FirstOrDefaultAsync(x=>x.TestId == id);

        if (test is null)
            return NotFound();

        return test.ToUser();
    }
}