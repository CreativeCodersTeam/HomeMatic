using System.Security.Claims;
using CreativeCoders.AspNetCore.TokenAuthApi.Abstractions;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend;

public class HomeMaticApiUserProvider : IUserProvider
{
    public Task<bool> AuthenticateAsync(string userName, string password, string? domain)
    {
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Claim>> GetUserClaimsAsync(string userName, string? domain)
    {
        return Task.FromResult<IEnumerable<Claim>>([]);
    }
}
