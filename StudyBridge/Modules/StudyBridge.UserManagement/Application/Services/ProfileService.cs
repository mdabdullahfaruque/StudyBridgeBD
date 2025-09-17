using StudyBridge.Shared.Common;
using StudyBridge.Shared.CQRS;
using StudyBridge.Shared.Exceptions;
using StudyBridge.UserManagement.Application.Contracts;
using StudyBridge.UserManagement.Features.UserProfile;

namespace StudyBridge.UserManagement.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IDispatcher _dispatcher;

    public ProfileService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<ServiceResult<GetProfile.Response>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return ServiceResult<GetProfile.Response>.Failure("Invalid user ID format", 400);
            }

            var query = new GetProfile.Query { UserId = userGuid };
            var result = await _dispatcher.QueryAsync(query, cancellationToken);
            
            return ServiceResult<GetProfile.Response>.Success(result, "Profile retrieved successfully");
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<GetProfile.Response>.Failure(ex.Message, 404, ex.Errors);
        }
        catch (UnauthorizedException ex)
        {
            return ServiceResult<GetProfile.Response>.Failure(ex.Message, 401, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<GetProfile.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<GetProfile.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<GetProfile.Response>.Failure($"Failed to get profile: {ex.Message}", 500);
        }
    }

    public async Task<ServiceResult<UpdateProfile.Response>> UpdateProfileAsync(string userId, UpdateProfile.Request request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Guid.TryParse(userId, out var userGuid))
            {
                return ServiceResult<UpdateProfile.Response>.Failure("Invalid user ID format", 400);
            }

            var command = new UpdateProfile.Command 
            {
                UserId = userGuid,
                DisplayName = request.DisplayName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                AvatarUrl = request.AvatarUrl
            };
            var result = await _dispatcher.CommandAsync(command, cancellationToken);
            
            return ServiceResult<UpdateProfile.Response>.Success(result, "Profile updated successfully");
        }
        catch (NotFoundException ex)
        {
            return ServiceResult<UpdateProfile.Response>.Failure(ex.Message, 404, ex.Errors);
        }
        catch (UnauthorizedException ex)
        {
            return ServiceResult<UpdateProfile.Response>.Failure(ex.Message, 401, ex.Errors);
        }
        catch (ValidationException ex)
        {
            return ServiceResult<UpdateProfile.Response>.Failure(ex.Message, 400, ex.Errors);
        }
        catch (StudyBridgeException ex)
        {
            return ServiceResult<UpdateProfile.Response>.Failure(ex.Message, ex.StatusCode, ex.Errors);
        }
        catch (Exception ex)
        {
            return ServiceResult<UpdateProfile.Response>.Failure($"Failed to update profile: {ex.Message}", 500);
        }
    }
}
