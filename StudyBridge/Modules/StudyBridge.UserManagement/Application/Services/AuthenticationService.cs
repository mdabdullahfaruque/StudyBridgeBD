using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Exceptions;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.UserManagement.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IDispatcher _dispatcher;

    public AuthenticationService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<ServiceResult<Register.Response>> RegisterAsync(Register.Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dispatcher.CommandAsync(command, cancellationToken);
            
            return ServiceResult<Register.Response>.Success(result, "Registration successful", 201);
        }
        catch (ConflictException ex)
        {
            return ServiceResult<Register.Response>.Failure(ex.Message, 409, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<Register.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<Register.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<Register.Response>.Failure($"Registration failed: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<Login.Response>> LoginAsync(Login.Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dispatcher.CommandAsync(command, cancellationToken);
            
            return ServiceResult<Login.Response>.Success(result, "Login successful");
        }
        catch (UnauthorizedException ex)
        {
            return ServiceResult<Login.Response>.Failure(ex.Message, 401, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<Login.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<Login.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<Login.Response>.Failure($"Login failed: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<GoogleLogin.Response>> GoogleLoginAsync(GoogleLogin.Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dispatcher.CommandAsync(command, cancellationToken);
            
            return ServiceResult<GoogleLogin.Response>.Success(result, "Google login successful");
        }
        catch (UnauthorizedException ex)
        {
            return ServiceResult<GoogleLogin.Response>.Failure(ex.Message, 401, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<GoogleLogin.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<GoogleLogin.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<GoogleLogin.Response>.Failure($"Google login failed: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<ChangePassword.Response>> ChangePasswordAsync(ChangePassword.Command command, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _dispatcher.CommandAsync(command, cancellationToken);
            
            return ServiceResult<ChangePassword.Response>.Success(result, "Password changed successfully");
        }
        catch (UnauthorizedException ex)
        {
            return ServiceResult<ChangePassword.Response>.Failure(ex.Message, 401, ex.Errors);
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<ChangePassword.Response>.Failure(ex.Message, 404, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<ChangePassword.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<ChangePassword.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<ChangePassword.Response>.Failure($"Password change failed: {ex.Message}", 500);
        }
    }
}
