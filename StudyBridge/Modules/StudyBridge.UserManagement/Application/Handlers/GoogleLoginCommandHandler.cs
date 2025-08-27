using StudyBridge.Shared.CQRS;
using StudyBridge.UserManagement.Application.Commands;
using StudyBridge.UserManagement.Application.DTOs;
using StudyBridge.UserManagement.Domain.Entities;
using StudyBridge.UserManagement.Domain.Repositories;
using StudyBridge.UserManagement.Application.Services;

namespace StudyBridge.UserManagement.Application.Handlers;

public class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;

    public GoogleLoginCommandHandler(
        IUserRepository userRepository,
        IGoogleAuthService googleAuthService,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> HandleAsync(GoogleLoginCommand command, CancellationToken cancellationToken = default)
    {
        var googleUser = await _googleAuthService.ValidateTokenAsync(command.IdToken);
        
        var user = await _userRepository.GetByGoogleSubAsync(googleUser.Sub);
        
        if (user == null)
        {
            user = new AppUser
            {
                GoogleSub = googleUser.Sub,
                Email = googleUser.Email,
                DisplayName = googleUser.Name,
                AvatarUrl = googleUser.Picture,
                LastLoginAt = DateTime.UtcNow
            };
            
            user = await _userRepository.CreateAsync(user, cancellationToken);
        }
        else
        {
            user = user with { LastLoginAt = DateTime.UtcNow };
            user = await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(7);

        return new LoginResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            },
            ExpiresAt = expiresAt
        };
    }
}
