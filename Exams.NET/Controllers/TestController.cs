using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers; 

/// <summary>
/// This controller is designed to retrieve data when a student is taking a test. This is to not leak answers into the client that may be inspected by the test taker.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase {
    private readonly TestAdministrationContext _context;

    /// <summary>
    /// This controller needs the test context to create test taker appropriate data.
    /// </summary>
    /// <param name="context">Db context for test and test question retrieval</param>
    public TestController(TestAdministrationContext context) {
        _context = context;
    }

    
    /// <summary>
    /// Gets all the questions that a user can answer without leaking said answers.
    /// Method: GET
    /// Route: api/test
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 JSON encoded list of user available tests</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserTest>>> GetTests(CancellationToken ct = default) {
        return await _context.Tests
                            .Select(x=>x.ToUser())
                            .ToListAsync(cancellationToken: ct);
    }
    
    /// <summary>
    /// Gets a specific test designed for a user to take.
    /// Method: GET
    /// Route: api/test/{id}
    /// </summary>
    /// <param name="id">id of the test to retrieve</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK JSON encoded user test or 404 not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserTest>> GetTest(int id, CancellationToken ct = default) {
        var test = await _context.Tests
                                 .Include(x=>x.Problems)
                                 .FirstOrDefaultAsync(x=>x.TestId == id, cancellationToken: ct);

        if (test is null)
            return NotFound();

        return test.ToUser();
    }
}