namespace StudyBridge.Application.Contracts.Services;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
