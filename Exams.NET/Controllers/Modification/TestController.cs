using System.Security.Claims;
using Exams.NET.Data;
using Exams.NET.Models;
using Exams.NET.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Exams.NET.Controllers.Modification;

/// <summary>
/// This controller is meant for the modification of test objects and requires the user to have authorized in order to use it. There is no restriction on who can create tests at this time 
/// The route is api/admin/test.
/// </summary>
[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class TestController : ControllerBase {
    private readonly TestAdministrationContext _testContext;
    private readonly IUserIdProvider _idProvider;
    
    /// <summary>
    /// The test controller requires the test administration db context.
    /// </summary>
    /// <param name="testContext">Db context to administer tests</param>
    public TestController(TestAdministrationContext testContext, IUserIdProvider idProvider) {
        _testContext = testContext;
        _idProvider = idProvider;
    }

    /// <summary>
    /// Gets all of the tests that the user can change.
    /// Method: GET
    /// Route: /api/test
    /// </summary>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>An IEnumerable of Tests that can be modified</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    
public async Task<ActionResult<IEnumerable<Test>>> GetTests(CancellationToken cancellationToken = default) {
        var tests = _testContext.Tests.Where(x=> x.UserId == GetCurrentUserId())
                                .Include(x=>x.Problems);

        if (tests.Any())
            return await tests.ToListAsync(cancellationToken);

        return Array.Empty<Test>();
    }
    /// <summary>
    /// Gets a specific test based on the id of the test in the route.
    /// Method: GET
    /// Route:api/admin/test/{id}
    /// </summary>
    /// <param name="id">The id of the list to find</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>200 with the test that has the associated id or 404</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Test>> GetTest(int id, CancellationToken cancellationToken = default) {
        var test = await _testContext.Tests.Include(x=>x.Problems)
                                     .FirstOrDefaultAsync(x=>x.UserId == GetCurrentUserId() && x.TestId == id, cancellationToken: cancellationToken);

        if (test == null)
            return NotFound();

        return test;
    }

    
    /// <summary>
    /// Updates Tests and any attached problems.
    /// Method: PUT
    /// Route: /api/admin/test/{id}
    /// </summary>
    /// <remarks>
    /// Any test without problems attached will be removed from the test. Make sure to set up navigation properties.
    /// </remarks>
    /// <param name="id">Id of the test to update</param>
    /// <param name="test">Updated Test</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Bad Request, 204 No Content or, 404</returns>
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
    
    /// <summary>
    /// Updates Tests and any attached problems.
    /// Method: PUT
    /// Route: /api/admin/test
    /// </summary>
    /// <remarks>
    /// Any test without problems attached will be removed from the test. Make sure to set up navigation properties.
    /// </remarks>
    /// <param name="test">Updated Test</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>Bad Request, 204 No Content or, 404</returns>
    [HttpPut]
    public async Task<IActionResult> PutTest(Test test, CancellationToken cancellationToken = default) {
        return await PutTest(test.TestId, test, cancellationToken);
    }

    /// <summary>
    /// Create a test.
    /// </summary>
    /// <remarks>This does not also create or link problems. You must make a PUT request to link those</remarks>
    /// <param name="testCreation">Test to create</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>201 created with the route and created object or internal server error if the object cannot be created </returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Test>> PostTest(TestCreationDto testCreation, CancellationToken cancellationToken = default) {
        testCreation.UserId = GetCurrentUserId();
        testCreation.LastUpdated = testCreation.Created = DateTime.UtcNow;
        
        await _testContext.Tests.AddAsync(testCreation.ToEntity(), cancellationToken);
        try {
            await _testContext.SaveChangesAsync(cancellationToken);
        }
        catch {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return CreatedAtAction("GetTest", new { id = testCreation.TestId }, testCreation);
    }
    
    /// <summary>
    /// Deletes test with the associated id.
    /// Method: DELETE
    /// Route: /admin/test/{id}
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>204 No Content or 404 not found</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTest(int id, CancellationToken cancellationToken = default) {
        if (id == default)
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
        return _testContext.Tests.Any(e => e.TestId == id);
    }
    private string GetCurrentUserId() {
        return _idProvider.GetCurrentUserId(this.HttpContext);
    }
}