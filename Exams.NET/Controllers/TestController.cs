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
    public async Task<ActionResult<IEnumerable<Test>>> GetTests() {
        if (_context.Tests == null)
            return NotFound();
        return await _context.Tests.ToListAsync();
    }

    // GET: api/Test/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Test>> GetTest(int id) {
        if (_context.Tests == null)
            return NotFound();
        var test = await _context.Tests.FindAsync(id);

        if (test == null)
            return NotFound();

        return test;
    }
}