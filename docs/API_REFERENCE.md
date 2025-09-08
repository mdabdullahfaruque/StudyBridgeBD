# StudyBridge API Reference (Updated September 2025)

## 📋 Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [Profile Management](#profile-management)
3. [Admin Operations](#admin-operations)
4. [Error Handling](#error-handling)
5. [Request/Response Patterns](#requestresponse-patterns)
6. [Authentication & Authorization](#authentication--authorization)
7. [Configuration & Security](#configuration--security)

---

## 🚀 Current Implementation Status

All endpoints are **fully implemented and tested** with 258 passing unit tests and 92.2% business logic coverage.

**Base URL**: `http://localhost:5000/api/v1` (Development)  
**Authentication**: JWT Bearer Token  
**Content Type**: `application/json`

---

## 🔐 Authentication Endpoints

### POST /api/v1/auth/login
Authenticate user with email and password.

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant AuthService
    participant Database
    
    Client->>API: POST /api/v1/auth/login
    Note over Client,API: { email, password }
    API->>AuthService: Validate credentials
    AuthService->>Database: Query user
    Database->>AuthService: User data
    AuthService->>AuthService: Verify password
    AuthService->>AuthService: Generate JWT
    AuthService->>API: Token + user info
    API->>Client: 200 OK + response
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "userpassword123"
}
```

**Response:**
```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "",
    "expiresAt": "2024-03-15T10:30:00Z",
    "email": "user@example.com",
    "displayName": "John Doe",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "roles": ["Student"]
  },
  "message": "Login successful",
  "statusCode": 200,
  "timestamp": "2024-03-14T10:30:00Z"
}
```

### POST /api/v1/auth/register
Register a new user account.

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "newpassword123",
  "displayName": "Jane Smith",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

### POST /api/v1/auth/google
Authenticate using Google OAuth token.

**Request Body:**
```json
{
  "googleToken": "google_oauth_token_here"
}
```

### POST /api/v1/auth/change-password
Change user password (requires authentication).

**Request Body:**
```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword456"
}
```

---

## 👤 Profile Management

### GET /api/v1/profile
Get current user's profile information.

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant ProfileService
    participant Database
    
    Client->>API: GET /api/v1/profile
    Note over Client,API: Bearer Token Required
    API->>ProfileService: GetProfile(userId)
    ProfileService->>Database: Query profile
    Database->>ProfileService: Profile data
    ProfileService->>API: Profile DTO
    API->>Client: 200 OK + profile
```

**Response:**
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "fullName": "John Doe",
    "dateOfBirth": "1990-01-15",
    "phone": "+8801712345678",
    "country": "Bangladesh",
    "timeZone": "Asia/Dhaka",
    "language": "en-US",
    "notificationsEnabled": true
  },
  "message": "Profile retrieved successfully",
  "statusCode": 200,
  "timestamp": "2024-03-14T10:30:00Z"
}
```

### PUT /api/v1/profile
Update user profile information.

**Request Body:**
```json
{
  "fullName": "John Updated Doe",
  "dateOfBirth": "1990-01-15",
  "phone": "+8801712345678",
  "country": "Bangladesh",
  "timeZone": "Asia/Dhaka",
  "language": "en-US",
  "notificationsEnabled": false
}
```

---

## 🛡️ Admin Operations

### GET /api/v1/admin/users
Get list of all users (Admin only).

```mermaid
graph LR
    subgraph "Authorization Flow"
        TOKEN[JWT Token] --> CLAIMS[Extract Claims]
        CLAIMS --> ROLE[Check Role]
        ROLE --> ADMIN{Is Admin?}
        ADMIN -->|Yes| ACCESS[Grant Access]
        ADMIN -->|No| DENY[403 Forbidden]
    end
```

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `search` (optional): Search term

**Response:**
```json
{
  "data": {
    "users": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "email": "user@example.com",
        "displayName": "John Doe",
        "isActive": true,
        "lastLoginAt": "2024-03-14T09:15:00Z",
        "roles": ["Student"]
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 10,
    "totalPages": 15
  },
  "message": "Users retrieved successfully",
  "statusCode": 200
}
```

### POST /api/v1/admin/users/{userId}/roles
Assign role to user (Admin only).

**Request Body:**
```json
{
  "roleId": "456e7890-e89b-12d3-a456-426614174001"
}
```

---

## ⚠️ Error Handling

### Error Response Structure

```mermaid
graph TB
    subgraph "Exception Types"
        BASE[StudyBridgeException]
        VALIDATION[ValidationException - 400]
        NOTFOUND[NotFoundException - 404]
        UNAUTHORIZED[UnauthorizedException - 401]
        FORBIDDEN[ForbiddenException - 403]
        CONFLICT[ConflictException - 409]
        BUSINESS[BusinessLogicException - 422]
    end
    
    BASE --> VALIDATION
    BASE --> NOTFOUND
    BASE --> UNAUTHORIZED
    BASE --> FORBIDDEN
    BASE --> CONFLICT
    BASE --> BUSINESS
```

### Error Response Format

```json
{
  "data": null,
  "message": "Validation failed",
  "statusCode": 400,
  "errors": [
    "Email is required",
    "Password must be at least 8 characters"
  ],
  "timestamp": "2024-03-14T10:30:00Z"
}
```

### Common HTTP Status Codes

| Status Code | Description | Example |
|-------------|-------------|---------|
| 200 | Success | Successful login, profile update |
| 400 | Bad Request | Validation errors, malformed JSON |
| 401 | Unauthorized | Invalid token, expired token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | User not found, resource not found |
| 409 | Conflict | Email already exists |
| 422 | Unprocessable Entity | Business logic validation failed |
| 500 | Internal Server Error | Unexpected server error |

---

## 📤 Request/Response Patterns

### Standard API Response Wrapper

```mermaid
graph TB
    subgraph "ApiResponse&lt;T&gt; Structure"
        DATA[Data: T | null]
        MESSAGE[Message: string]
        STATUS[StatusCode: int]
        ERRORS[Errors: List&lt;string&gt; | null]
        TIMESTAMP[Timestamp: DateTime]
    end
    
    subgraph "Success Response"
        SUCCESS_DATA[Actual response data]
        SUCCESS_MSG["Success message"]
        SUCCESS_STATUS[200, 201, etc.]
        SUCCESS_ERRORS[null]
    end
    
    subgraph "Error Response"
        ERROR_DATA[null]
        ERROR_MSG["Error description"]
        ERROR_STATUS[400, 401, 403, etc.]
        ERROR_ERRORS["List of error messages"]
    end
    
    DATA --> SUCCESS_DATA
    DATA --> ERROR_DATA
    MESSAGE --> SUCCESS_MSG
    MESSAGE --> ERROR_MSG
    STATUS --> SUCCESS_STATUS
    STATUS --> ERROR_STATUS
    ERRORS --> SUCCESS_ERRORS
    ERRORS --> ERROR_ERRORS
```

### Pagination Pattern

For endpoints returning lists of data:

```json
{
  "data": {
    "items": [...],
    "totalCount": 150,
    "page": 1,
    "pageSize": 10,
    "totalPages": 15,
    "hasNext": true,
    "hasPrevious": false
  },
  "message": "Data retrieved successfully",
  "statusCode": 200
}
```

---

## 🔒 Authentication & Authorization

### JWT Token Structure

```mermaid
graph LR
    subgraph "JWT Token Claims"
        HEADER[Header]
        PAYLOAD[Payload]
        SIGNATURE[Signature]
    end
    
    subgraph "Payload Claims"
        SUB[sub: User ID]
        EMAIL[email: User Email]
        ROLES[roles: User Roles]
        IAT[iat: Issued At]
        EXP[exp: Expires At]
        ISS[iss: Issuer]
        AUD[aud: Audience]
    end
    
    PAYLOAD --> SUB
    PAYLOAD --> EMAIL
    PAYLOAD --> ROLES
    PAYLOAD --> IAT
    PAYLOAD --> EXP
    PAYLOAD --> ISS
    PAYLOAD --> AUD
```

### Authorization Header Format

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

### Role-Based Access Control

| Role | Permissions | Endpoints |
|------|-------------|-----------|
| **Administrator** | Full system access | All endpoints |
| **Teacher** | Content management, progress viewing | Content CRUD, student progress |
| **Student** | Learning activities, personal progress | Profile, vocabulary, progress |
| **Guest** | Limited vocabulary access | Public vocabulary endpoints |

### Permission Structure

```mermaid
graph TB
    subgraph "Permission Model"
        PERM[Permission]
        RESOURCE[Resource: string]
        ACTION[Action: string]
        DESCRIPTION[Description: string]
    end
    
    subgraph "Example Permissions"
        USER_READ["users:read"]
        USER_WRITE["users:write"]
        VOCAB_READ["vocabulary:read"]
        VOCAB_WRITE["vocabulary:write"]
        PROGRESS_READ["progress:read"]
    end
    
    PERM --> RESOURCE
    PERM --> ACTION
    PERM --> DESCRIPTION
    
    RESOURCE --> USER_READ
    RESOURCE --> USER_WRITE
    RESOURCE --> VOCAB_READ
    RESOURCE --> VOCAB_WRITE
    RESOURCE --> PROGRESS_READ
```

---

## � Configuration & Security (September 2025)

### Secure Configuration Management

The API now implements a comprehensive secure configuration pattern to protect sensitive data:

```mermaid
graph TB
    subgraph "Configuration Security"
        TEMPLATE[appsettings.json<br/>📄 Safe placeholders]
        DEV[appsettings.Development.json<br/>🔒 Real secrets - Git ignored]
        EXAMPLE[appsettings.Example.json<br/>📋 Documentation template]
        PROD[Environment Variables<br/>🔧 Production secrets]
    end
    
    subgraph "Security Features"
        GITHUB[GitHub Push Protection<br/>🛡️ Prevents secret commits]
        GITIGNORE[.gitignore rules<br/>🚫 Excludes dev configs]
        OAUTH[Google OAuth 2.0<br/>🔑 Secure authentication]
        JWT[JWT Token Security<br/>🎫 Secure sessions]
    end
    
    TEMPLATE --> DEV
    DEV --> GITIGNORE
    EXAMPLE --> PROD
    OAUTH --> JWT
```

### OAuth 2.0 Configuration

**Google OAuth Setup**:
```json
{
  "GoogleAuth": {
    "ClientId": "your-google-oauth-client-id",
    "ClientSecret": "your-google-oauth-client-secret"
  }
}
```

**Security Implementation**:
- ✅ GitHub Push Protection compliance
- ✅ Environment-based secret management  
- ✅ Development/Production configuration separation
- ✅ Comprehensive setup documentation

### JWT Security Features

```mermaid
sequenceDiagram
    participant Client
    participant API
    participant JWT as JWT Service
    participant Google
    participant Database
    
    Note over Client,Database: Enhanced Security Flow
    
    Client->>API: Login request
    API->>JWT: Generate secure token
    JWT->>JWT: Sign with secret key
    JWT->>API: Return JWT + metadata
    API->>Database: Update last login
    API->>Client: Secure response
    
    Note over JWT: Token includes: userId, email, roles, expiry
    Note over API: Automatic token validation on protected routes
```

---

## �🔄 API Versioning

### Version Strategy

```mermaid
graph TB
    subgraph "API Versioning"
        V1["/api/v1/*"]
        V2["/api/v2/* (Future)"]
        
        subgraph "v1 Endpoints (Implemented ✅)"
            AUTH["/api/v1/auth/*"]
            PROFILE["/api/v1/profile"]
            ADMIN["/api/v1/admin/*"]
        end
        
        subgraph "Future v2 Endpoints"
            AUTH_V2["/api/v2/auth/*"]
            VOCAB_V2["/api/v2/vocabulary/*"]
            LEARNING_V2["/api/v2/learning/*"]
        end
    end
    
    V1 --> AUTH
    V1 --> PROFILE
    V1 --> ADMIN
    
    V2 --> AUTH_V2
    V2 --> VOCAB_V2
    V2 --> LEARNING_V2
```

### Implementation Status (September 2025)

| Endpoint | Status | Tests | Security | Notes |
|----------|--------|-------|----------|-------|
| `POST /auth/login` | ✅ Complete | ✅ 14 tests | ✅ JWT + validation | Production ready |
| `POST /auth/register` | ✅ Complete | ✅ 12 tests | ✅ Password hashing | Production ready |
| `POST /auth/google` | ✅ Complete | ✅ 8 tests | ✅ OAuth 2.0 flow | Production ready |
| `POST /auth/change-password` | ✅ Complete | ✅ 6 tests | ✅ Current password validation | Production ready |
| `GET /profile` | ✅ Complete | ✅ 4 tests | ✅ JWT required | Production ready |
| `PUT /profile` | ✅ Complete | ✅ 6 tests | ✅ Input validation | Production ready |
| `GET /admin/users` | ✅ Complete | ✅ 3 tests | ✅ Admin role required | Production ready |
| `DELETE /admin/users/{id}` | ✅ Complete | ✅ 3 tests | ✅ Admin role required | Production ready |

**Total: 258 tests passing with 92.2% business logic coverage**

---

*API Reference last updated: September 8, 2025*

### Breaking Changes Policy

- Major version changes (v1 → v2) for breaking changes
- Minor version increments for new features
- Patch version increments for bug fixes
- Minimum 6-month deprecation notice for v1 endpoints

---

*This API reference is automatically generated from the StudyBridge codebase and updated with each release.*
