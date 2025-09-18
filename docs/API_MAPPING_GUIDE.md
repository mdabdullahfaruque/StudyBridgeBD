# StudyBridge API Mapping Guide

## 🎯 Purpose

This guide provides a systematic approach to analyze StudyBridge backend APIs and create accurate frontend mappings. **ALWAYS follow this process before implementing any new frontend features**.

## 🏗️ StudyBridge Backend Architecture

StudyBridge uses **Clean Architecture + CQRS (Command Query Responsibility Segregation)** pattern:

```
StudyBridge Backend Structure:
├── StudyBridge.Api/Controllers/          # API Controllers (HTTP layer)
├── StudyBridge.{Module}.Features/        # CQRS Commands/Queries  
├── StudyBridge.Domain/Entities/          # Domain Models
├── StudyBridge.Shared/Common/            # Shared DTOs & Utilities
└── StudyBridge.Infrastructure/           # Data Access Layer
```

## 📊 API Analysis Workflow

### Step 1: Locate the API Controller

```bash
# Find relevant controllers
find . -name "*Controller.cs" -path "*/Api/Controllers/*"

# Example results:
./StudyBridge.Api/Controllers/AdminController.cs
./StudyBridge.Api/Controllers/AuthController.cs  
./StudyBridge.Api/Controllers/UserController.cs
```

### Step 2: Analyze Controller Endpoints

```csharp
// Example: StudyBridge.Api/Controllers/AdminController.cs
[ApiController]
[Route("api/v1/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]                    // 🔗 Endpoint: GET /api/v1/admin/users
    [RequirePermission("users.view")]     // 🔐 Authorization required
    public async Task<IActionResult> GetUsers([FromQuery] GetUsers.Query query)
    {
        var response = await _getUsersHandler.HandleAsync(query);
        return Ok(ApiResponse<GetUsers.Response>.SuccessResult(response, "Users retrieved successfully"));
    }
}
```

**Key Information to Extract**:
- 🔗 **Full Endpoint URL**: `GET /api/v1/admin/users`
- 🔐 **Authorization Requirements**: `[RequirePermission("users.view")]`
- 📥 **Request Type**: `GetUsers.Query` (from CQRS)
- 📤 **Response Type**: `ApiResponse<GetUsers.Response>`

### Step 3: Analyze CQRS Query/Command Structure

```bash
# Find the CQRS feature implementation
find . -name "GetUsers.cs" -path "*/Features/*"
# Result: ./StudyBridge.UserManagement/Features/Admin/GetUsers.cs
```

```csharp
// StudyBridge.UserManagement/Features/Admin/GetUsers.cs
public static class GetUsers
{
    // 📥 REQUEST STRUCTURE
    public class Query : IQuery<Response>
    {
        public int PageNumber { get; set; } = 1;      // Frontend: pageNumber
        public int PageSize { get; set; } = 10;       // Frontend: pageSize  
        public string? SearchTerm { get; set; }       // Frontend: searchTerm
        public string? Role { get; set; }             // Frontend: role
        public bool? IsActive { get; set; }           // Frontend: isActive
        public string? SortBy { get; set; }           // Frontend: sortBy
        public string? SortDirection { get; set; }    // Frontend: sortDirection
    }

    // 📤 RESPONSE STRUCTURE
    public class Response
    {
        public PaginatedResult<UserDto> Users { get; set; } = new();
        public string Message { get; set; } = "Users retrieved successfully";
    }

    // 📋 INDIVIDUAL ITEM STRUCTURE  
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }      // ⚠️ NOT isEmailVerified
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<UserSubscriptionDto> Subscriptions { get; set; } = new();
    }
}
```

### Step 4: Analyze Shared Response Structures

```csharp
// StudyBridge.Shared/Common/ApiResponse.cs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }                      // 📦 Contains the actual data
    public List<string> Errors { get; set; } = new();
}

// StudyBridge.Shared/Common/PaginatedResult.cs  
public class PaginatedResult<T>
{
    public IList<T> Items { get; set; } = new List<T>();  // 📋 The actual items
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
}
```

## 🎯 Frontend Mapping Process

### Step 1: Create Exact Interface Mappings

```typescript
// 1. Map ApiResponse structure
export interface ApiResponse<T> {
  success: boolean;        // matches backend Success
  data?: T;               // matches backend Data  
  message?: string;       // matches backend Message
  errors?: string[];      // matches backend Errors
}

// 2. Map PaginatedResult structure
export interface PaginatedResult<T> {
  items: T[];             // matches backend Items
  totalCount: number;     // matches backend TotalCount
  pageNumber: number;     // matches backend PageNumber
  pageSize: number;       // matches backend PageSize
  totalPages: number;     // matches backend TotalPages
  hasNextPage: boolean;   // matches backend HasNextPage
  hasPreviousPage: boolean; // matches backend HasPreviousPage
}

// 3. Map Query parameters EXACTLY
export interface GetUsersRequest {
  pageNumber?: number;    // matches Query.PageNumber
  pageSize?: number;      // matches Query.PageSize
  searchTerm?: string;    // matches Query.SearchTerm
  role?: string;          // matches Query.Role
  isActive?: boolean;     // matches Query.IsActive
  sortBy?: string;        // matches Query.SortBy
  sortDirection?: 'asc' | 'desc'; // matches Query.SortDirection
}

// 4. Map DTO properties EXACTLY (watch for naming differences)
export interface AdminUser {
  id: string;             // matches UserDto.Id
  email: string;          // matches UserDto.Email
  displayName: string;    // matches UserDto.DisplayName
  firstName?: string;     // matches UserDto.FirstName
  lastName?: string;      // matches UserDto.LastName
  isActive: boolean;      // matches UserDto.IsActive
  emailConfirmed: boolean; // ⚠️ matches UserDto.EmailConfirmed (NOT isEmailVerified)
  roles: string[];        // matches UserDto.Roles
  permissions: string[];  // matches UserDto.Permissions
  subscriptions: UserSubscription[]; // matches UserDto.Subscriptions
  createdAt: string;      // matches UserDto.CreatedAt (as ISO string)
  lastLoginAt?: string;   // matches UserDto.LastLoginAt
}

// 5. Map Response structure EXACTLY
export interface GetUsersResponse {
  users: PaginatedResult<AdminUser>; // matches Response.Users
  message: string;                   // matches Response.Message
}
```

### Step 2: Create HTTP Service Method

```typescript
@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getUsers(request: GetUsersRequest = {}): Observable<ApiResponse<GetUsersResponse>> {
    let params = new HttpParams();
    
    // Map frontend request to backend Query exactly
    if (request.pageNumber) params = params.set('pageNumber', request.pageNumber.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request.searchTerm) params = params.set('searchTerm', request.searchTerm);
    if (request.role) params = params.set('role', request.role);
    if (request.isActive !== undefined) params = params.set('isActive', request.isActive.toString());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<ApiResponse<GetUsersResponse>>(`${this.apiUrl}/users`, { params });
  }
}
```

### Step 3: Handle Response in Component

```typescript
loadUsers(): void {
  const request: GetUsersRequest = {
    pageNumber: this.currentPage,
    pageSize: this.pageSize,
    searchTerm: this.searchTerm,
    sortBy: 'createdAt',
    sortDirection: 'desc'
  };

  this.adminService.getUsers(request)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (response: ApiResponse<GetUsersResponse>) => {
        // Handle successful response
        if (response.success && response.data?.users?.items) {
          this.users = response.data.users.items;
          this.processUsersForDisplay();
          this.toastService.success('Success', `Loaded ${this.users.length} users`);
        } else {
          this.handleError('Failed to load users', response.message);
        }
      },
      error: (error) => {
        // Handle HTTP errors
        this.handleError('API Error', error.message);
      }
    });
}
```

## 🔧 Testing & Validation

### Step 1: Manual API Testing

```bash
# Start the backend
cd StudyBridge/StudyBridge.Api
dotnet run

# Test the endpoint
curl -X GET \
  "http://localhost:5000/api/v1/admin/users?pageNumber=1&pageSize=5" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json"
```

### Step 2: Verify Response Structure

```javascript
// In browser console, log the actual response
console.log('Full API Response:', response);
console.log('Response Data:', response.data);
console.log('Users Collection:', response.data?.users);
console.log('User Items:', response.data?.users?.items);
console.log('First User:', response.data?.users?.items?.[0]);
```

### Step 3: Validate Property Mappings

```typescript
// Add temporary logging to verify mappings
private processUsersForDisplay(): void {
  this.processedUsers = this.users.map(user => {
    console.log('Processing user:', user);
    console.log('isActive:', user.isActive, 'emailConfirmed:', user.emailConfirmed);
    
    return {
      ...user,
      statusDisplay: this.getStatusDisplay(user.isActive),
      emailVerifiedDisplay: this.getEmailVerifiedDisplay(user.emailConfirmed) // Use correct property
    };
  });
}
```

## ❌ Common Mapping Mistakes

### 1. Property Name Mismatches
```typescript
// ❌ Wrong: Assuming property names
isEmailVerified: user.isEmailVerified

// ✅ Correct: Using actual backend property
emailConfirmed: user.emailConfirmed
```

### 2. Response Structure Assumptions  
```typescript
// ❌ Wrong: Assuming flat structure
this.users = response.data.items;

// ✅ Correct: Using actual nested structure  
this.users = response.data.users.items;
```

### 3. Missing Error Handling
```typescript
// ❌ Wrong: No validation
this.users = response.data.users.items;

// ✅ Correct: Proper validation
if (response.success && response.data?.users?.items) {
  this.users = response.data.users.items;
} else {
  this.handleError('Invalid response structure');
}
```

## 🎯 Best Practices

1. **Always Analyze First**: Never assume API structure
2. **Match Exactly**: Copy backend property names precisely  
3. **Test Manually**: Verify endpoints work before frontend integration
4. **Log Everything**: Use console.log during development to verify mappings
5. **Handle Errors**: Always add proper error handling and validation
6. **Update Interfaces**: Keep frontend interfaces synchronized with backend changes

## 📚 Reference Implementations

See these completed implementations:
- **User Management**: `AdminService.getUsers()` 
- **Role Management**: `AdminService.getRoles()`
- **Permission Management**: `AdminService.getPermissions()`

Each follows this exact mapping pattern for consistency and reliability.