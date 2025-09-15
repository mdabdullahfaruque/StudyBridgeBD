namespace StudyBridge.Domain.Entities;

public enum SystemPermission
{
    // User Management
    ViewUsers = 1,
    CreateUsers = 2,
    EditUsers = 3,
    DeleteUsers = 4,
    ManageUserRoles = 5,
    
    // Content Management
    ViewContent = 10,
    CreateContent = 11,
    EditContent = 12,
    DeleteContent = 13,
    PublishContent = 14,
    
    // Financial Management
    ViewFinancials = 20,
    ManagePayments = 21,
    ViewReports = 22,
    ManageSubscriptions = 23,
    ManageRefunds = 24,
    
    // System Administration
    ViewSystemLogs = 30,
    ManageSystemSettings = 31,
    ViewAnalytics = 32,
    ManageBackups = 33,
    
    // Module Management
    ManageVocabularyModule = 40,
    ManageIeltsModule = 41,
    ManagePteModule = 42,
    ManageGreModule = 43,
    ManageHigherStudiesModule = 44,
    
    // Super Admin Only
    ManageAdministrators = 50,
    SystemMaintenance = 51
}
