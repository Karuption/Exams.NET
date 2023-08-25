using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers; 

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ShareController: ControllerBase {
    private readonly TestSharingContext _shareContext;
    private readonly TestAdministrationContext _testContext;
    private readonly IdentityUserContext<ApplicationUser> _userContext;
    
    public ShareController(TestSharingContext shareContext, IdentityUserContext<ApplicationUser> userContext, TestAdministrationContext testContext) {
        _shareContext = shareContext;
        _userContext = userContext;
        _testContext = testContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTestShareLink(Test userTest, CancellationToken ct) {
        return await createTestShare(userTest.TestId, GetCurrentUserId(), ct);
    }
    
    [HttpPost("{testId}")]
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
        var shareInfo = await _shareContext.TestShareInfo.FirstOrDefaultAsync(x => x.testId == test.TestId, ct);
        if (shareInfo is not null)
            return NoContent();

        // if not, try and add it.
        shareInfo = new() { shared = true, testId = test.TestId, OwnerId = test.UserId };
        _shareContext.TestShareInfo.Add(shareInfo);

        try {
            await _shareContext.SaveChangesAsync(ct);
            return NoContent();
        }
        catch {
            return Problem();
        }
    }

    [HttpGet("{ownerId}/{testId}/{shareId}")]
    public async Task<IActionResult> GetTestShare(string ownerId, int testId, Guid shareId, CancellationToken ct = default) {
        if (string.IsNullOrWhiteSpace(ownerId) || testId == default)
            return NotFound();

        // make sure the creator exists
        var owner = await _userContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == ownerId, ct);
        if (owner is null || owner.LockoutEnabled&&owner.LockoutEnd>DateTimeOffset.UtcNow) 
            return NotFound();

        // make sure that the test exists
        var test = await _testContext.Tests.AsNoTracking().FirstOrDefaultAsync(x=>x.TestId == testId, ct);
        if (test is null || test.UserId == owner.Id)
            return NotFound();

        // make sure that all of the user provided information adds up
        var share = await _shareContext.TestShareInfo.FirstOrDefaultAsync(x=>x.Id == shareId, ct);
        if (share is null || !share.shared || share.OwnerId!=owner.Id || share.testId!=test.TestId)
            return NotFound();
        
        // add the test to the shared tests
        TestShare userEntry = new() { SharedTest = share, AllowedUserId = GetCurrentUserId() };
        _shareContext.TestShares.Add(userEntry);

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