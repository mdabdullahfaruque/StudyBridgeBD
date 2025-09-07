using StudyBridge.Shared.Common;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.UserManagement.Application.Contracts;

public interface IProfileService
{
    Task<ServiceResult<GetProfile.Response>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<UpdateProfile.Response>> UpdateProfileAsync(string userId, UpdateProfile.Request request, CancellationToken cancellationToken = default);
}
