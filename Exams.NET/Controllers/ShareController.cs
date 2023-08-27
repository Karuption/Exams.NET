using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers; 

/// <summary>
/// This controller gets information about tests and how they are shared
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShareController: ControllerBase {
    private readonly TestSharingContext _shareContext;
    private readonly TestAdministrationContext _testContext;
    private readonly ApplicationDbContext _userContext;
    
    /// <summary>
    /// This controller gets information about tests and how they are shared
    /// </summary>
    /// <param name="shareContext">Context for how the tests are shared</param>
    /// <param name="userContext">Context for Application users</param>
    /// <param name="testContext">Context for tests</param>
    public ShareController(TestSharingContext shareContext, ApplicationDbContext userContext, TestAdministrationContext testContext) {
        _shareContext = shareContext;
        _userContext = userContext;
        _testContext = testContext;
    }

    /// <summary>
    /// Gets a list of all the tests that the user has been shared
    /// Method: GET
    /// Route: api/share
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>List of tests that the user has been shared</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Test>>> GetSharedTests(CancellationToken ct = default) {
         return await _shareContext.TestShares.Include(x=>x.SharedTest.Test)
                                          .Where(x => x.AllowedUserId == GetCurrentUserId() && x.SharedTest.Shared)
                                          .Select(x=>x.SharedTest.Test)
                                          .ToListAsync(ct);
    }
    
    /// <summary>
    /// Share a test by test object.
    /// Method: POST
    /// Route: api/share
    /// </summary>
    /// <param name="userTest">User test to share</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with share id, 404 not found and 500 internal server error</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTestShareLink(Test userTest, CancellationToken ct) {
        return await createTestShare(userTest.TestId, GetCurrentUserId(), ct);
    }
    
    /// <summary>
    /// Share a test by id.
    /// Method: POST
    /// Route: api/share/{testId}
    /// </summary>
    /// <param name="testId">Test id to share</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>200 OK with share id, 404 not found and 500 internal server error</returns>
    [HttpPost("{testId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTestShareLink(int testId, CancellationToken ct) {
        return await createTestShare(testId, GetCurrentUserId(), ct);
    }

    private async Task<IActionResult> createTestShare(int userTestId, string userId, CancellationToken ct = default) {
        if (userTestId < 1)
            return NotFound();
        
        //make sure the the user owns the test
        var test = await _testContext.Tests.AsNoTracking().FirstOrDefaultAsync(
            x => x.UserId == userId && x.TestId == userTestId, ct);
        if (test is null)
            return NotFound("Test not found");

        //check if its already shared
        var shareInfo = await _shareContext.TestShareInfo.FirstOrDefaultAsync(x => x.TestId == test.TestId, ct);
        if (shareInfo is not null)
            return Ok(shareInfo.Id);

        // if not, try and add it.
        shareInfo = new() { Shared = true, TestId = test.TestId, Test = test, OwnerId = test.UserId };
        await _shareContext.TestShareInfo.AddAsync(shareInfo, ct);

        try {
            await _shareContext.SaveChangesAsync(ct);
            return Ok(shareInfo.Id);
        }
        catch {
            return Problem();
        }
    }

    /// <summary>
    /// Share to the current user.
    /// Method: GET
    /// Route: /api/share/{ownerId}/{testId}/{shareId}
    /// </summary>
    /// <param name="ownerId">id of the test owner</param>
    /// <param name="testId">id of the test</param>
    /// <param name="shareId">id of the share</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>204 no content, 404 not found, or 500 internal server error</returns>
    [HttpGet("{ownerId}/{testId}/{shareId}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTestShare(string ownerId, int testId, Guid shareId, CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(ownerId) || testId == default)
            return NotFound();

        // make sure the creator exists
        var owner = await _userContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ownerId, ct);
        if (owner is null || owner.LockoutEnabled&&owner.LockoutEnd>DateTimeOffset.UtcNow) 
            return NotFound();

        // make sure that the test exists
        var test = await _testContext.Tests.AsNoTracking().FirstOrDefaultAsync(x=>x.TestId == testId, ct);
        if (test is null || test.UserId != owner.Id)
            return NotFound();

        // make sure that all of the user provided information adds up
        var share = await _shareContext.TestShareInfo.FirstOrDefaultAsync(x=>x.Id == shareId, ct);
        if (share is null || !share.Shared || share.OwnerId!=owner.Id || share.TestId!=test.TestId)
            return NotFound();
        
        // add the test to the shared tests
        TestShare userEntry = new() { SharedTest = share, AllowedUserId = GetCurrentUserId() };
        await _shareContext.TestShares.AddAsync(userEntry, ct);

        try {
            await _shareContext.SaveChangesAsync(ct);
        }
        catch {
            return Problem();
        }

        return NoContent();
    }
    
    private string GetCurrentUserId() {
        return HttpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }

}