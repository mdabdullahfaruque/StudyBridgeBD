namespace StudyBridge.Domain.Enums;

/// <summary>
/// Defines the type of permission for role-based access control
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// Permission to view/read data
    /// </summary>
    View = 0,
    
    /// <summary>
    /// Permission to create new data
    /// </summary>
    Create = 1,
    
    /// <summary>
    /// Permission to edit/update existing data
    /// </summary>
    Edit = 2,
    
    /// <summary>
    /// Permission to delete data
    /// </summary>
    Delete = 3,
    
    /// <summary>
    /// Permission to execute specific operations
    /// </summary>
    Execute = 4,
    
    /// <summary>
    /// Permission to manage (all CRUD operations)
    /// </summary>
    Manage = 5
}