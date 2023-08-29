using System.Security.Claims;

namespace Exams.NET.Providers;

internal class UserIdProvider : IUserIdProvider {
    public string GetCurrentUserId(HttpContext httpContext) {
        return httpContext.User.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
    }
}

public interface IUserIdProvider {
    public string GetCurrentUserId(HttpContext httpContext);
}