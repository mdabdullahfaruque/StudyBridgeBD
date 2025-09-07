using StudyBridge.Shared.Common;
using StudyBridge.UserManagement.Features.Authentication;

namespace StudyBridge.UserManagement.Application.Contracts;

public interface IAuthenticationService
{
    Task<ServiceResult<Register.Response>> RegisterAsync(Register.Command command, CancellationToken cancellationToken = default);
    Task<ServiceResult<Login.Response>> LoginAsync(Login.Command command, CancellationToken cancellationToken = default);
    Task<ServiceResult<GoogleLogin.Response>> GoogleLoginAsync(GoogleLogin.Command command, CancellationToken cancellationToken = default);
    Task<ServiceResult<ChangePassword.Response>> ChangePasswordAsync(ChangePassword.Command command, CancellationToken cancellationToken = default);
}
