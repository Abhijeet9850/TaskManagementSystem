using TaskManager.API.Models;

namespace TaskManager.API.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(User user, bool rememberMe);
    }
}
