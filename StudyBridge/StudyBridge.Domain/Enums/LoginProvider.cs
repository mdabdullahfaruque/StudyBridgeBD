namespace StudyBridge.Domain.Enums;

/// <summary>
/// Defines the login provider type for a user account
/// </summary>
public enum LoginProvider
{
    /// <summary>
    /// User registered and logs in with local email/password
    /// </summary>
    Local = 0,
    
    /// <summary>
    /// User logs in with Google OAuth
    /// </summary>
    Google = 1,
    
    /// <summary>
    /// User logs in with Facebook OAuth (future implementation)
    /// </summary>
    Facebook = 2,
    
    /// <summary>
    /// User logs in with Microsoft OAuth (future implementation)
    /// </summary>
    Microsoft = 3,
    
    /// <summary>
    /// User logs in with Apple OAuth (future implementation)
    /// </summary>
    Apple = 4
}
