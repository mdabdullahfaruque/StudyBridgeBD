# StudyBridge Project Documentation

## 📋 Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Overview](#architecture-overview)
3. [Domain Model](#domain-model)
4. [Application Flow](#application-flow)
5. [Module Structure](#module-structure)
6. [Authentication & Authorization](#authentication--authorization)
7. [Data Layer](#data-layer)
8. [API Structure](#api-structure)
9. [Frontend Architecture](#frontend-architecture)
10. [Testing Strategy](#testing-strategy)
11. [Development Guidelines](#development-guidelines)
12. [Future Roadmap](#future-roadmap)

---

## 🎯 Project Overview

**StudyBridge** is a comprehensive IELTS vocabulary learning platform designed specifically for Bangladeshi students. The platform combines modern web technologies with proven learning methodologies to create an effective vocabulary acquisition system.

### Key Features
- 🔐 **Multi-Authentication Support**: Google OAuth + Traditional login
- 📚 **Vocabulary Management**: 2,100+ categorized IELTS words
- 🧠 **Spaced Repetition System (SRS)**: Scientific learning algorithms
- 📊 **Progress Tracking**: Detailed analytics and learning streaks
- 🎮 **Gamification**: Interactive learning experience
- 📱 **Responsive Design**: Works across all devices

### Technology Stack

```mermaid
graph TB
    subgraph "Frontend"
        A[Angular 20+] --> B[TypeScript]
        A --> C[RxJS]
        A --> D[Angular Router]
        A --> E[HTTP Interceptors]
    end
    
    subgraph "Backend"
        F[.NET 8 Web API] --> G[Entity Framework Core]
        F --> H[PostgreSQL]
        F --> I[JWT Authentication]
        F --> J[Custom CQRS]
    end
    
    subgraph "Architecture"
        K[Clean Architecture]
        L[Modular Monolith]
        M[Domain Driven Design]
    end
    
    A --> F
    G --> H
    K --> F
    L --> F
    M --> F
```

---

## 🏗️ Architecture Overview

StudyBridge follows **Clean Architecture** principles with a **Modular Monolithic** approach, ensuring maintainability, testability, and scalability.

### High-Level Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        API[StudyBridge.Api]
        UI[Angular Frontend]
    end
    
    subgraph "Application Layer"
        APP[StudyBridge.Application]
        MODULES[Modules]
        
        subgraph "Module: User Management"
            UM[StudyBridge.UserManagement]
        end
        
        subgraph "Future Modules"
            VM[StudyBridge.VocabularyManagement]
            LM[StudyBridge.LearningManagement]
            PM[StudyBridge.ProgressManagement]
        end
    end
    
    subgraph "Domain Layer"
        DOMAIN[StudyBridge.Domain]
        SHARED[StudyBridge.Shared]
    end
    
    subgraph "Infrastructure Layer"
        INFRA[StudyBridge.Infrastructure]
        DB[(PostgreSQL)]
        EXT[External Services]
    end
    
    UI --> API
    API --> UM
    API --> VM
    API --> LM
    API --> PM
    UM --> APP
    VM --> APP
    LM --> APP
    PM --> APP
    APP --> DOMAIN
    APP --> SHARED
    INFRA --> DB
    INFRA --> EXT
    API --> INFRA
```

### CQRS Pattern Implementation

```mermaid
graph LR
    subgraph "CQRS Flow"
        CMD[Command] --> HANDLER[Command Handler]
        QUERY[Query] --> QHANDLER[Query Handler]
        
        HANDLER --> DOMAIN[Domain Logic]
        QHANDLER --> REPO[Repository]
        
        DOMAIN --> EVENT[Domain Events]
        REPO --> DB[(Database)]
    end
    
    subgraph "Custom Implementation"
        IDISP[IDispatcher Interface]
        DISP[Dispatcher Implementation]
        ICMD[ICommand Interface]
        IQUERY[IQuery Interface]
    end
    
    CMD --> IDISP
    QUERY --> IDISP
    IDISP --> DISP
    DISP --> HANDLER
    DISP --> QHANDLER
```

---

## 🎯 Domain Model

### Core Entities Relationship

```mermaid
erDiagram
    AppUser {
        guid Id PK
        string GoogleSub
        string Email UK
        string DisplayName
        string AvatarUrl
        string PasswordHash
        string FirstName
        string LastName
        bool EmailConfirmed
        datetime LastLoginAt
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    UserProfile {
        guid Id PK
        guid UserId FK
        string FullName
        date DateOfBirth
        string Phone
        string Country
        string TimeZone
        string Language
        bool NotificationsEnabled
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    Role {
        guid Id PK
        string Name UK
        string Description
        SystemRole SystemRole
        bool IsActive
        datetime CreatedAt
        datetime UpdatedAt
    }
    
    UserRole {
        guid Id PK
        guid UserId FK
        guid RoleId FK
        datetime AssignedAt
        guid AssignedBy FK
        bool IsActive
    }
    
    Permission {
        guid Id PK
        string Name UK
        string Description
        string Resource
        string Action
        bool IsActive
    }
    
    RolePermission {
        guid Id PK
        guid RoleId FK
        guid PermissionId FK
        datetime GrantedAt
        guid GrantedBy FK
    }
    
    UserSubscription {
        guid Id PK
        guid UserId FK
        SubscriptionType Type
        datetime StartDate
        datetime EndDate
        bool IsActive
        decimal Amount
        string Currency
        datetime CreatedAt
    }
    
    AppUser ||--o| UserProfile : "has"
    AppUser ||--o{ UserRole : "has many"
    Role ||--o{ UserRole : "assigned to many"
    Role ||--o{ RolePermission : "has many"
    Permission ||--o{ RolePermission : "granted to many"
    AppUser ||--o{ UserSubscription : "has many"
```

### Domain Aggregates

```mermaid
graph TB
    subgraph "User Aggregate"
        USER[AppUser Root]
        PROFILE[UserProfile]
        ROLES[UserRoles]
        SUBS[UserSubscriptions]
        
        USER --> PROFILE
        USER --> ROLES
        USER --> SUBS
    end
    
    subgraph "Permission Aggregate"
        ROLE[Role Root]
        PERMS[RolePermissions]
        
        ROLE --> PERMS
    end
    
    subgraph "Future: Vocabulary Aggregate"
        WORD[VocabularyWord Root]
        CATEGORY[WordCategory]
        DIFFICULTY[DifficultyLevel]
        
        WORD --> CATEGORY
        WORD --> DIFFICULTY
    end
    
    subgraph "Future: Learning Aggregate"
        SESSION[LearningSession Root]
        PROGRESS[UserProgress]
        SCHEDULE[SRSSchedule]
        
        SESSION --> PROGRESS
        SESSION --> SCHEDULE
    end
```

---

## 🔄 Application Flow

### User Authentication Flow

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant API as API Gateway
    participant AUTH as Auth Service
    participant DB as Database
    participant GOOGLE as Google OAuth
    
    Note over U,GOOGLE: Google OAuth Flow
    U->>FE: Click "Login with Google"
    FE->>GOOGLE: Redirect to Google OAuth
    GOOGLE->>U: Google Login Page
    U->>GOOGLE: Enter credentials
    GOOGLE->>FE: Return OAuth token
    FE->>API: POST /api/auth/google {token}
    API->>AUTH: Validate Google token
    AUTH->>GOOGLE: Verify token
    GOOGLE->>AUTH: User info
    AUTH->>DB: Find/Create user
    DB->>AUTH: User entity
    AUTH->>AUTH: Generate JWT
    AUTH->>API: JWT token
    API->>FE: {token, user info}
    FE->>FE: Store token & redirect
    
    Note over U,DB: Traditional Login Flow
    U->>FE: Enter email/password
    FE->>API: POST /api/auth/login
    API->>AUTH: Validate credentials
    AUTH->>DB: Query user
    DB->>AUTH: User data
    AUTH->>AUTH: Verify password
    AUTH->>AUTH: Generate JWT
    AUTH->>API: JWT token
    API->>FE: {token, user info}
    FE->>FE: Store token & navigate
```

### CQRS Command/Query Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API Controller
    participant D as Dispatcher
    participant H as Handler
    participant S as Service
    participant R as Repository
    participant DB as Database
    
    Note over C,DB: Command Flow (Write)
    C->>API: POST /api/users/profile
    API->>D: Dispatch(UpdateProfileCommand)
    D->>H: Handle(command)
    H->>H: Validate command
    H->>S: Call domain service
    S->>S: Apply business rules
    S->>R: Update entity
    R->>DB: Save changes
    DB->>R: Confirm
    R->>S: Success
    S->>H: Result
    H->>D: Response
    D->>API: Response
    API->>C: HTTP 200 OK
    
    Note over C,DB: Query Flow (Read)
    C->>API: GET /api/users/profile
    API->>D: Dispatch(GetProfileQuery)
    D->>H: Handle(query)
    H->>R: Query data
    R->>DB: SELECT query
    DB->>R: Data
    R->>H: Result
    H->>D: Response
    D->>API: Response
    API->>C: HTTP 200 + Data
```

### Error Handling Flow

```mermaid
graph TB
    subgraph "Exception Hierarchy"
        SE[StudyBridgeException] --> VE[ValidationException]
        SE --> NF[NotFoundException] 
        SE --> UE[UnauthorizedException]
        SE --> FE[ForbiddenException]
        SE --> CE[ConflictException]
        SE --> BLE[BusinessLogicException]
    end
    
    subgraph "Error Flow"
        EX[Exception Thrown] --> MW[Global Exception Middleware]
        MW --> LOG[Log Error]
        MW --> MAP[Map to HTTP Status]
        MAP --> RESP[API Response]
        RESP --> CLIENT[Client]
    end
    
    VE --> MW
    NF --> MW
    UE --> MW
    FE --> MW
    CE --> MW
    BLE --> MW
```

---

## 📦 Module Structure

### UserManagement Module

```mermaid
graph TB
    subgraph "StudyBridge.UserManagement"
        subgraph "Features"
            AUTH[Authentication Features]
            PROFILE[Profile Features]
            
            subgraph "Auth Features"
                LOGIN[Login]
                REGISTER[Register]
                GOOGLE[GoogleLogin]
                CHANGEPASS[ChangePassword]
            end
            
            subgraph "Profile Features"
                GETPROF[GetProfile]
                UPDATEPROF[UpdateProfile]
            end
        end
        
        subgraph "Application Layer"
            AUTHSVC[AuthenticationService]
            PROFSVC[ProfileService]
            CONTRACTS[Contracts/Interfaces]
        end
        
        subgraph "Extensions"
            SERVICES[ServiceCollection Extensions]
        end
    end
    
    AUTH --> LOGIN
    AUTH --> REGISTER
    AUTH --> GOOGLE
    AUTH --> CHANGEPASS
    
    PROFILE --> GETPROF
    PROFILE --> UPDATEPROF
    
    LOGIN --> AUTHSVC
    REGISTER --> AUTHSVC
    GOOGLE --> AUTHSVC
    CHANGEPASS --> AUTHSVC
    
    GETPROF --> PROFSVC
    UPDATEPROF --> PROFSVC
    
    AUTHSVC --> CONTRACTS
    PROFSVC --> CONTRACTS
```

### Feature Structure Pattern

```mermaid
graph TB
    subgraph "Feature Pattern (Vertical Slice)"
        COMMAND[Command/Query]
        VALIDATOR[FluentValidation Validator]
        HANDLER[Command/Query Handler]
        RESPONSE[Response DTO]
        
        COMMAND --> VALIDATOR
        COMMAND --> HANDLER
        HANDLER --> RESPONSE
    end
    
    subgraph "Example: Login Feature"
        LOGINCMD[Login.Command]
        LOGINVAL[Login.Validator]
        LOGINHDL[Login.Handler]
        LOGINRESP[Login.Response]
        
        LOGINCMD --> LOGINVAL
        LOGINCMD --> LOGINHDL
        LOGINHDL --> LOGINRESP
    end
```

---

## 🔐 Authentication & Authorization

### Authentication System

```mermaid
graph TB
    subgraph "Authentication Methods"
        GOOGLE[Google OAuth 2.0]
        LOCAL[Email/Password]
    end
    
    subgraph "JWT Token System"
        JWT[JWT Token Service]
        CLAIMS[Claims Generation]
        VALIDATION[Token Validation]
    end
    
    subgraph "Security Features"
        HASH[Password Hashing]
        REFRESH[Refresh Tokens]
        EXPIRE[Token Expiration]
    end
    
    GOOGLE --> JWT
    LOCAL --> JWT
    LOCAL --> HASH
    JWT --> CLAIMS
    JWT --> VALIDATION
    JWT --> REFRESH
    JWT --> EXPIRE
```

### Authorization Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant MW as Auth Middleware
    participant JWT as JWT Service
    participant PERM as Permission Service
    participant API as API Endpoint
    
    C->>MW: Request with JWT token
    MW->>JWT: Validate token
    JWT->>MW: Token claims
    MW->>PERM: Check permissions
    PERM->>PERM: Evaluate user roles
    PERM->>MW: Permission result
    
    alt Permission Granted
        MW->>API: Forward request
        API->>C: Process & respond
    else Permission Denied
        MW->>C: 403 Forbidden
    end
```

### Role-Based Access Control

```mermaid
graph LR
    subgraph "System Roles"
        ADMIN[Administrator]
        TEACHER[Teacher]
        STUDENT[Student]
        GUEST[Guest]
    end
    
    subgraph "Permissions"
        USER_MGMT[User Management]
        CONTENT_MGMT[Content Management]
        VOCAB_ACCESS[Vocabulary Access]
        PROGRESS_VIEW[Progress Viewing]
        SYSTEM_CONFIG[System Configuration]
    end
    
    ADMIN --> USER_MGMT
    ADMIN --> CONTENT_MGMT
    ADMIN --> SYSTEM_CONFIG
    
    TEACHER --> CONTENT_MGMT
    TEACHER --> PROGRESS_VIEW
    
    STUDENT --> VOCAB_ACCESS
    STUDENT --> PROGRESS_VIEW
    
    GUEST --> VOCAB_ACCESS
```

---

## 💾 Data Layer

### Database Schema

```mermaid
erDiagram
    Users {
        uuid id PK
        string google_sub
        string email UK
        string display_name
        string avatar_url
        string password_hash
        string first_name
        string last_name
        boolean email_confirmed
        timestamp last_login_at
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }
    
    UserProfiles {
        uuid id PK
        uuid user_id FK
        string full_name
        date date_of_birth
        string phone
        string country
        string time_zone
        string language
        boolean notifications_enabled
        timestamp created_at
        timestamp updated_at
    }
    
    Roles {
        uuid id PK
        string name UK
        string description
        int system_role
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }
    
    UserRoles {
        uuid id PK
        uuid user_id FK
        uuid role_id FK
        timestamp assigned_at
        uuid assigned_by FK
        boolean is_active
    }
    
    Permissions {
        uuid id PK
        string name UK
        string description
        string resource
        string action
        boolean is_active
    }
    
    RolePermissions {
        uuid id PK
        uuid role_id FK
        uuid permission_id FK
        timestamp granted_at
        uuid granted_by FK
    }
    
    UserSubscriptions {
        uuid id PK
        uuid user_id FK
        int subscription_type
        timestamp start_date
        timestamp end_date
        boolean is_active
        decimal amount
        string currency
        timestamp created_at
    }
    
    Users ||--o| UserProfiles : "has"
    Users ||--o{ UserRoles : "has many"
    Roles ||--o{ UserRoles : "assigned to"
    Roles ||--o{ RolePermissions : "has"
    Permissions ||--o{ RolePermissions : "granted to"
    Users ||--o{ UserSubscriptions : "has"
```

### Repository Pattern

```mermaid
graph TB
    subgraph "Repository Pattern"
        IAPPDB[IApplicationDbContext]
        APPDB[AppDbContext]
        ENTITIES[Entity Framework Entities]
        
        subgraph "Repositories"
            USERREPO[UserRepository]
            ROLEREPO[RoleRepository]
            PERMREPO[PermissionRepository]
        end
    end
    
    IAPPDB --> APPDB
    APPDB --> ENTITIES
    USERREPO --> IAPPDB
    ROLEREPO --> IAPPDB
    PERMREPO --> IAPPDB
```

---

## 🌐 API Structure

### API Versioning & Organization

```mermaid
graph TB
    subgraph "API Structure"
        ROOT["/api/v1"]
        
        subgraph "Authentication"
            AUTH["/api/v1/auth"]
            LOGIN["/login"]
            REGISTER["/register"]
            GOOGLE["/google"]
            CHANGE["/change-password"]
        end
        
        subgraph "Profile Management"
            PROFILE["/api/v1/profile"]
            GET["/GET"]
            UPDATE["/PUT"]
        end
        
        subgraph "Admin Operations"
            ADMIN["/api/v1/admin"]
            USERS["/users"]
            ROLES["/roles"]
            PERMISSIONS["/permissions"]
        end
        
        subgraph "Future: Content"
            CONTENT["/api/v1/content"]
            VOCAB["/vocabulary"]
            CATEGORIES["/categories"]
        end
    end
    
    ROOT --> AUTH
    ROOT --> PROFILE
    ROOT --> ADMIN
    ROOT --> CONTENT
    
    AUTH --> LOGIN
    AUTH --> REGISTER
    AUTH --> GOOGLE
    AUTH --> CHANGE
    
    PROFILE --> GET
    PROFILE --> UPDATE
    
    ADMIN --> USERS
    ADMIN --> ROLES
    ADMIN --> PERMISSIONS
    
    CONTENT --> VOCAB
    CONTENT --> CATEGORIES
```

### API Response Pattern

```mermaid
graph TB
    subgraph "API Response Structure"
        RESPONSE[ApiResponse&lt;T&gt;]
        SUCCESS[Success Response]
        ERROR[Error Response]
        
        subgraph "Success Fields"
            DATA[Data: T]
            MESSAGE[Message: string]
            STATUS[StatusCode: int]
            TIMESTAMP[Timestamp: DateTime]
        end
        
        subgraph "Error Fields"
            ERRORS[Errors: List&lt;string&gt;]
            ERRORMSG[Message: string]
            ERRORSTATUS[StatusCode: int]
            ERRORTIMESTAMP[Timestamp: DateTime]
        end
    end
    
    RESPONSE --> SUCCESS
    RESPONSE --> ERROR
    
    SUCCESS --> DATA
    SUCCESS --> MESSAGE
    SUCCESS --> STATUS
    SUCCESS --> TIMESTAMP
    
    ERROR --> ERRORS
    ERROR --> ERRORMSG
    ERROR --> ERRORSTATUS
    ERROR --> ERRORTIMESTAMP
```

---

## 🎨 Frontend Architecture (Angular 20 Implementation)

### Current Application Structure (September 2025)

```mermaid
graph TB
    subgraph "Angular 20 Application (Implemented ✅)"
        subgraph "Core Services"
            AUTH_SVC[AuthService<br/>Observable State Management]
            HTTP_INT[HTTP Interceptor<br/>JWT Token Injection]
            GUARDS[Auth Guards<br/>Route Protection]
            MODELS[TypeScript Models<br/>Strong Typing]
            NOTIFICATION[NotificationService<br/>Toast Messages]
        end
        
        subgraph "Implemented Features"
            AUTH_COMP[Authentication Module<br/>Login + Register + Google OAuth]
            DASHBOARD[Dashboard Module<br/>User Profile Display]
            PROFILE[Profile Management<br/>User Info CRUD]
        end
        
        subgraph "Shared Components"
            BUTTON[ButtonComponent<br/>Reusable UI Button]
            LOADING[LoadingComponent<br/>Loading States]
            HEADER[HeaderComponent<br/>Navigation Bar]
        end
        
        subgraph "Routing System"
            PUBLIC[Public Routes<br/>/auth/*]
            PROTECTED[Protected Routes<br/>/dashboard, /profile]
            REDIRECT[Smart Redirects<br/>returnUrl handling]
        end
    end
    
    AUTH_COMP --> AUTH_SVC
    DASHBOARD --> AUTH_SVC
    PROFILE --> AUTH_SVC
    
    HTTP_INT --> AUTH_SVC
    GUARDS --> AUTH_SVC
    
    AUTH_COMP --> MODELS
    DASHBOARD --> MODELS
    PROFILE --> MODELS
    
    PROTECTED --> GUARDS
    PUBLIC --> REDIRECT
```

### Modern Angular 20 Features Implemented

```mermaid
graph LR
    subgraph "Angular 20 Features ✅"
        STANDALONE[Standalone Components<br/>No NgModules]
        SIGNALS[Reactive State<br/>RxJS + Observables]
        MODERN_FORMS[Reactive Forms<br/>Strong Typing]
        MODERN_ROUTING[Modern Router<br/>Guards & Resolvers]
    end
    
    subgraph "TypeScript 5.9 ✅"
        STRICT[Strict Mode<br/>Null Safety]
        DECORATORS[Modern Decorators<br/>Clean Syntax]
        TYPES[Strong Typing<br/>Interface Driven]
    end
    
    subgraph "Styling Solution ✅"
        TAILWIND[Tailwind CSS 3.4<br/>Utility-first]
        RESPONSIVE[Mobile-first<br/>Responsive Design]
        DARK_MODE[Dark Mode Ready<br/>CSS Variables]
    end
```

### Component Architecture (Implemented)

```typescript
// Example: Modern Standalone Component Implementation
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  loginForm: FormGroup;
  isLoading = false;
  
  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }
  
  onSubmit(): void {
    if (this.loginForm.valid) {
      this.authService.login(this.loginForm.value)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => this.handleLoginSuccess(response),
          error: (error) => this.handleLoginError(error)
        });
    }
  }
}
```

### State Management Flow (Implemented)

```mermaid
sequenceDiagram
    participant UI as Component
    participant AUTH as AuthService
    participant HTTP as HttpClient
    participant INT as JWT Interceptor
    participant API as Backend API
    participant STORE as Local Storage
    
    UI->>AUTH: login(credentials)
    AUTH->>HTTP: POST /auth/login
    HTTP->>INT: Outgoing request
    INT->>API: Request with headers
    API->>INT: JWT token response
    INT->>HTTP: Response
    HTTP->>AUTH: AuthResponse
    AUTH->>STORE: Store JWT token
    AUTH->>AUTH: Update currentUser$ observable
    AUTH->>UI: Success response
    UI->>UI: Navigate to dashboard
    
    Note over AUTH: Observable-based state management
    Note over STORE: Persistent auth state
```

### Routing Implementation

```typescript
// Current routing configuration (implemented)
export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        component: LoginComponent
      },
      {
        path: 'register', 
        component: RegisterComponent
      }
    ]
  },
  {
    path: 'dashboard',
    component: DashboardHomeComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [AuthGuard]
  }
];
```

### UI Component System (Tailwind CSS)

```mermaid
graph TB
    subgraph "Design System ✅"
        COLORS[Color Palette<br/>Primary, Secondary, etc.]
        TYPOGRAPHY[Typography Scale<br/>Headings, Body text]
        SPACING[Spacing System<br/>Consistent margins/padding]
        BREAKPOINTS[Responsive Breakpoints<br/>Mobile-first]
    end
    
    subgraph "Component Library ✅"
        FORMS[Form Components<br/>Input, Select, Checkbox]
        BUTTONS[Button Variants<br/>Primary, Secondary, Ghost]
        FEEDBACK[Feedback Components<br/>Alerts, Toasts, Loading]
        LAYOUT[Layout Components<br/>Header, Container, Grid]
    end
    
    subgraph "Responsive Features ✅"
        MOBILE[Mobile Optimized<br/>Touch-friendly]
        TABLET[Tablet Layout<br/>Medium screens]
        DESKTOP[Desktop Layout<br/>Large screens]
    end
```

### Authentication Flow (Complete Implementation)

```mermaid
flowchart TD
    START[User visits protected route] --> CHECK{Authenticated?}
    CHECK -->|No| LOGIN[Redirect to login]
    CHECK -->|Yes| ALLOW[Allow access]
    
    LOGIN --> FORM[Login form displayed]
    FORM --> SUBMIT{Form submitted}
    SUBMIT -->|Email/Password| LOCAL_AUTH[Local authentication]
    SUBMIT -->|Google OAuth| GOOGLE_AUTH[Google OAuth flow]
    
    LOCAL_AUTH --> API_CALL[POST /auth/login]
    GOOGLE_AUTH --> GOOGLE_POPUP[Google OAuth popup]
    GOOGLE_POPUP --> GOOGLE_TOKEN[Receive Google token]
    GOOGLE_TOKEN --> GOOGLE_API[POST /auth/google]
    
    API_CALL --> SUCCESS{Success?}
    GOOGLE_API --> SUCCESS
    
    SUCCESS -->|Yes| STORE_TOKEN[Store JWT token]
    SUCCESS -->|No| ERROR[Show error message]
    
    STORE_TOKEN --> UPDATE_STATE[Update auth state]
    UPDATE_STATE --> REDIRECT[Redirect to original route]
    
    ERROR --> FORM
    REDIRECT --> ALLOW
```

---

## 🧪 Testing Strategy

### Testing Pyramid

```mermaid
graph TB
    subgraph "Testing Pyramid"
        E2E[End-to-End Tests]
        INTEGRATION[Integration Tests]
        UNIT[Unit Tests - 258 Tests]
        
        subgraph "Unit Test Coverage"
            BUSINESS[Business Logic - 92.2%]
            SERVICES[Application Services - 90.8%]
            DOMAIN[Domain Entities - 64.8%]
            SHARED[Shared Components - 55.4%]
            INFRA[Infrastructure - 100%]
        end
    end
    
    E2E --> INTEGRATION
    INTEGRATION --> UNIT
    
    UNIT --> BUSINESS
    UNIT --> SERVICES
    UNIT --> DOMAIN
    UNIT --> SHARED
    UNIT --> INFRA
```

### Test Organization

```mermaid
graph TB
    subgraph "StudyBridge.Tests.Unit"
        subgraph "UserManagement Tests"
            AUTH_TESTS[Authentication Tests]
            PROFILE_TESTS[Profile Tests]
            PERM_TESTS[Permission Tests]
        end
        
        subgraph "Shared Tests"
            EXCEPTION_TESTS[Exception Tests]
            RESPONSE_TESTS[Response Pattern Tests]
            UTIL_TESTS[Utility Tests]
        end
        
        subgraph "Infrastructure Tests"
            JWT_TESTS[JWT Service Tests]
            HASH_TESTS[Password Hashing Tests]
            DB_TESTS[Database Context Tests]
        end
        
        subgraph "Test Utilities"
            BUILDERS[Test Data Builders]
            MOCKS[Mock Helpers]
            FIXTURES[Test Fixtures]
        end
    end
    
    AUTH_TESTS --> BUILDERS
    PROFILE_TESTS --> BUILDERS
    PERM_TESTS --> BUILDERS
    
    EXCEPTION_TESTS --> MOCKS
    RESPONSE_TESTS --> MOCKS
    
    JWT_TESTS --> FIXTURES
    HASH_TESTS --> FIXTURES
```

---

## 🚀 Development Guidelines

### Code Organization Principles

```mermaid
graph TB
    subgraph "Clean Architecture Layers"
        PRESENTATION[Presentation Layer]
        APPLICATION[Application Layer]
        DOMAIN[Domain Layer]
        INFRASTRUCTURE[Infrastructure Layer]
    end
    
    subgraph "Dependencies"
        PRESENTATION --> APPLICATION
        APPLICATION --> DOMAIN
        INFRASTRUCTURE --> DOMAIN
        PRESENTATION --> INFRASTRUCTURE
    end
    
    subgraph "Key Principles"
        SRP[Single Responsibility]
        OCP[Open/Closed]
        LSP[Liskov Substitution]
        ISP[Interface Segregation]
        DIP[Dependency Inversion]
    end
```

### Feature Development Process

```mermaid
flowchart TD
    START[Start Feature] --> DOMAIN[Define Domain Entities]
    DOMAIN --> CONTRACTS[Create Contracts/Interfaces]
    CONTRACTS --> FEATURE[Implement Feature Handler]
    FEATURE --> VALIDATION[Add FluentValidation]
    VALIDATION --> TESTS[Write Unit Tests]
    TESTS --> CONTROLLER[Create API Controller]
    CONTROLLER --> INTEGRATION[Integration Testing]
    INTEGRATION --> DOCUMENTATION[Update Documentation]
    DOCUMENTATION --> REVIEW[Code Review]
    REVIEW --> DEPLOY[Deploy]
```

---

## 🎯 Future Roadmap

### Planned Modules

```mermaid
gantt
    title StudyBridge Development Roadmap
    dateFormat  YYYY-MM-DD
    section Phase 1 - Foundation
    User Management           :done, user-mgmt, 2024-01-01, 2024-02-15
    Authentication System     :done, auth, 2024-01-15, 2024-02-28
    Testing Infrastructure    :done, testing, 2024-02-01, 2024-02-28
    
    section Phase 2 - Core Features
    Vocabulary Module         :vocab, 2024-03-01, 2024-04-15
    Learning Engine          :learning, 2024-03-15, 2024-05-01
    Progress Tracking        :progress, 2024-04-01, 2024-05-15
    
    section Phase 3 - Advanced Features
    SRS Algorithm            :srs, 2024-05-01, 2024-06-15
    Analytics Dashboard      :analytics, 2024-05-15, 2024-07-01
    Mobile App               :mobile, 2024-06-01, 2024-08-15
    
    section Phase 4 - Scale & Optimize
    Performance Optimization :perf, 2024-07-01, 2024-08-15
    Advanced Features        :advanced, 2024-08-01, 2024-09-30
    Production Deployment    :prod, 2024-09-01, 2024-10-15
```

### Upcoming Features

```mermaid
mindmap
  root((StudyBridge Future))
    Vocabulary System
      2100+ IELTS Words
      Categorization
      Difficulty Levels
      Audio Pronunciation
    Learning Engine
      Spaced Repetition
      Adaptive Difficulty
      Progress Tracking
      Learning Analytics
    Gamification
      Achievement System
      Learning Streaks
      Leaderboards
      Badges & Rewards
    Advanced Features
      Mobile Application
      Offline Mode
      Social Learning
      AI-Powered Recommendations
    Infrastructure
      Microservices Migration
      Cloud Deployment
      Performance Optimization
      Real-time Features
```

---

## 📚 References & Resources

### Key Technologies
- **Backend**: [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8), [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/), [PostgreSQL](https://www.postgresql.org/)
- **Frontend**: [Angular](https://angular.io/), [TypeScript](https://www.typescriptlang.org/), [RxJS](https://rxjs.dev/)
- **Testing**: [xUnit](https://xunit.net/), [Moq](https://github.com/moq/moq4), [FluentAssertions](https://fluentassertions.com/)
- **Authentication**: [JWT](https://jwt.io/), [Google OAuth](https://developers.google.com/identity/protocols/oauth2)

### Architecture Patterns
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Domain Driven Design](https://domainlanguage.com/ddd/)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)

---

*This documentation is maintained as a living document and will be updated as the project evolves. For the latest updates, please check the repository's documentation folder.*
