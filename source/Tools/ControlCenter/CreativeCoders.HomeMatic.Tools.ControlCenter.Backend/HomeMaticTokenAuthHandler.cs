using CreativeCoders.AspNetCore.TokenAuth;

namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend;

public class HomeMaticTokenAuthHandler : IUserAuthProvider
{
    public bool CheckUser(string userName, string password, string? domain)
    {
        return true;
    }
}