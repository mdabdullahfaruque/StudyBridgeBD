# StudyBridge Architecture Deep Dive

## 📋 Table of Contents

1. [Clean Architecture Implementation](#clean-architecture-implementation)
2. [Modular Monolith Structure](#modular-monolith-structure)
3. [CQRS Pattern](#cqrs-pattern)
4. [Domain-Driven Design](#domain-driven-design)
5. [Dynamic API-Driven Frontend](#dynamic-api-driven-frontend)
6. [Role-Based Access Control (RBAC)](#role-based-access-control-rbac)
7. [Dependency Injection](#dependency-injection)
8. [Data Access Patterns](#data-access-patterns)
9. [Cross-Cutting Concerns](#cross-cutting-concerns)
10. [Scalability Considerations](#scalability-considerations)

---

## 🏗️ Clean Architecture Implementation

StudyBridge implements Clean Architecture with clear separation of concerns across four main layers:

### Architecture Layers

```mermaid
graph TB
    subgraph "Clean Architecture Layers"
        subgraph "Presentation Layer"
            API[StudyBridge.Api]
            CONTROLLERS[Controllers]
            MIDDLEWARE[Middleware]
        end
        
        subgraph "Application Layer"
            APP[StudyBridge.Application]
            FEATURES[Feature Handlers]
            SERVICES[Application Services]
            CONTRACTS[Contracts/Interfaces]
        end
        
        subgraph "Domain Layer"
            DOMAIN[StudyBridge.Domain]
            ENTITIES[Domain Entities]
            VALUEOBJECTS[Value Objects]
            DOMAINSVC[Domain Services]
        end
        
        subgraph "Infrastructure Layer"
            INFRA[StudyBridge.Infrastructure]
            REPOS[Repositories]
            EXTERNALSVC[External Services]
            PERSISTENCE[Data Persistence]
        end
        
        subgraph "Shared Layer"
            SHARED[StudyBridge.Shared]
            COMMON[Common Types]
            CQRS[CQRS Infrastructure]
            EXCEPTIONS[Exception Types]
        end
    end
    
    API --> APP
    APP --> DOMAIN
    INFRA --> DOMAIN
    API --> INFRA
    APP --> SHARED
    INFRA --> SHARED
```

### Dependency Rules

```mermaid
flowchart LR
    subgraph "Dependency Direction"
        OUTER[Outer Layers] --> INNER[Inner Layers]
    end
    
    subgraph "Allowed Dependencies"
        PRESENTATION --> APPLICATION
        APPLICATION --> DOMAIN
        INFRASTRUCTURE --> DOMAIN
        INFRASTRUCTURE --> APPLICATION
    end
    
    subgraph "Forbidden Dependencies"
        DOMAIN -.->|❌| APPLICATION
        DOMAIN -.->|❌| INFRASTRUCTURE
        DOMAIN -.->|❌| PRESENTATION
    end
```

### Benefits of This Architecture

```mermaid
mindmap
  root((Clean Architecture Benefits))
    Testability
      Unit Testing
      Mocking Dependencies
      Isolated Testing
    Maintainability
      Clear Separation
      Single Responsibility
      Loose Coupling
    Flexibility
      Framework Independence
      Database Independence
      UI Independence
    Scalability
      Easy Module Addition
      Team Scalability
      Code Organization
```

---

## 🧩 Modular Monolith Structure

StudyBridge is designed as a modular monolith, providing the benefits of modular architecture while maintaining deployment simplicity.

### Module Structure

```mermaid
graph TB
    subgraph "StudyBridge Modular Monolith"
        subgraph "Core Modules"
            SHARED[StudyBridge.Shared]
            DOMAIN[StudyBridge.Domain]
            APP[StudyBridge.Application]
            INFRA[StudyBridge.Infrastructure]
        end
        
        subgraph "Feature Modules"
            USER_MGMT[StudyBridge.UserManagement]
            VOCAB_MGMT[StudyBridge.VocabularyManagement]
            LEARNING_MGMT[StudyBridge.LearningManagement]
            PROGRESS_MGMT[StudyBridge.ProgressManagement]
        end
        
        subgraph "Hosting"
            API[StudyBridge.Api]
        end
    end
    
    USER_MGMT --> SHARED
    USER_MGMT --> DOMAIN
    VOCAB_MGMT --> SHARED
    VOCAB_MGMT --> DOMAIN
    LEARNING_MGMT --> SHARED
    LEARNING_MGMT --> DOMAIN
    PROGRESS_MGMT --> SHARED
    PROGRESS_MGMT --> DOMAIN
    
    API --> USER_MGMT
    API --> VOCAB_MGMT
    API --> LEARNING_MGMT
    API --> PROGRESS_MGMT
    API --> INFRA
```

### Module Communication

```mermaid
sequenceDiagram
    participant API as API Layer
    participant UM as UserManagement
    participant VM as VocabularyManagement
    participant LM as LearningManagement
    participant SHARED as Shared Services
    
    API->>UM: User Authentication
    UM->>SHARED: Publish UserLoggedIn Event
    SHARED->>VM: Route Event
    VM->>VM: Initialize User Vocabulary
    SHARED->>LM: Route Event
    LM->>LM: Setup Learning Session
    LM->>API: Return Session Info
```

### Module Boundaries

```mermaid
graph LR
    subgraph "Module Isolation"
        subgraph "UserManagement"
            UM_FEATURES[Features]
            UM_SERVICES[Services]
            UM_CONTRACTS[Contracts]
        end
        
        subgraph "VocabularyManagement"
            VM_FEATURES[Features]
            VM_SERVICES[Services]
            VM_CONTRACTS[Contracts]
        end
        
        subgraph "Shared Contracts"
            EVENTS[Domain Events]
            INTERFACES[Shared Interfaces]
            DTOS[Data Transfer Objects]
        end
    end
    
    UM_FEATURES -.->|Events| EVENTS
    VM_FEATURES -.->|Events| EVENTS
    UM_SERVICES --> INTERFACES
    VM_SERVICES --> INTERFACES
```

---

## ⚡ CQRS Pattern

StudyBridge implements a custom CQRS (Command Query Responsibility Segregation) pattern without external dependencies.

### CQRS Architecture

```mermaid
graph TB
    subgraph "CQRS Implementation"
        subgraph "Command Side (Write)"
            CMD[Commands]
            CMD_HANDLER[Command Handlers]
            CMD_VALIDATOR[Validators]
            DOMAIN_SVC[Domain Services]
        end
        
        subgraph "Query Side (Read)"
            QUERY[Queries]
            QUERY_HANDLER[Query Handlers]
            READ_MODEL[Read Models]
            PROJECTIONS[Projections]
        end
        
        subgraph "Infrastructure"
            DISPATCHER[Dispatcher]
            DB[(Database)]
            EVENTS[Domain Events]
        end
    end
    
    CMD --> CMD_VALIDATOR
    CMD_VALIDATOR --> CMD_HANDLER
    CMD_HANDLER --> DOMAIN_SVC
    DOMAIN_SVC --> DB
    DOMAIN_SVC --> EVENTS
    
    QUERY --> QUERY_HANDLER
    QUERY_HANDLER --> READ_MODEL
    READ_MODEL --> DB
    
    CMD --> DISPATCHER
    QUERY --> DISPATCHER
    DISPATCHER --> CMD_HANDLER
    DISPATCHER --> QUERY_HANDLER
```

### CQRS Interfaces

```csharp
// Command Pattern
public interface ICommand<TResponse> { }

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

// Query Pattern
public interface IQuery<TResponse> { }

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

// Dispatcher
public interface IDispatcher
{
    Task<TResponse> DispatchAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
    Task<TResponse> DispatchAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
```

### Feature Implementation Pattern

```mermaid
graph TB
    subgraph "Feature: Login"
        subgraph "Command Structure"
            LOGIN_CMD[Login.Command]
            LOGIN_VALIDATOR[Login.Validator]
            LOGIN_HANDLER[Login.Handler]
            LOGIN_RESPONSE[Login.Response]
        end
        
        subgraph "Flow"
            REQUEST[HTTP Request] --> LOGIN_CMD
            LOGIN_CMD --> LOGIN_VALIDATOR
            LOGIN_VALIDATOR --> LOGIN_HANDLER
            LOGIN_HANDLER --> BUSINESS_LOGIC[Business Logic]
            BUSINESS_LOGIC --> LOGIN_RESPONSE
            LOGIN_RESPONSE --> HTTP_RESPONSE[HTTP Response]
        end
    end
```

---

## 🎯 Domain-Driven Design

StudyBridge applies DDD principles to model the complex vocabulary learning domain.

### Domain Model

```mermaid
graph TB
    subgraph "User Aggregate"
        USER[AppUser - Aggregate Root]
        PROFILE[UserProfile - Entity]
        ROLES[UserRoles - Entity]
        SUBSCRIPTION[UserSubscription - Entity]
        
        USER --> PROFILE
        USER --> ROLES
        USER --> SUBSCRIPTION
    end
    
    subgraph "Vocabulary Aggregate"
        WORD[VocabularyWord - Aggregate Root]
        DEFINITION[Definition - Value Object]
        PRONUNCIATION[Pronunciation - Value Object]
        EXAMPLES[Examples - Value Object Collection]
        
        WORD --> DEFINITION
        WORD --> PRONUNCIATION
        WORD --> EXAMPLES
    end
    
    subgraph "Learning Aggregate"
        SESSION[LearningSession - Aggregate Root]
        PROGRESS[Progress - Entity]
        SRS[SRSSchedule - Entity]
        STATS[Statistics - Value Object]
        
        SESSION --> PROGRESS
        SESSION --> SRS
        SESSION --> STATS
    end
```

### Aggregate Design Principles

```mermaid
flowchart TD
    subgraph "Aggregate Rules"
        CONSISTENCY[Enforce Consistency]
        BOUNDARIES[Clear Boundaries]
        IDENTITY[Single Identity]
        TRANSACTIONS[Transaction Boundaries]
    end
    
    subgraph "Design Guidelines"
        SMALL[Keep Aggregates Small]
        REFERENCES[Reference by Identity]
        EVENTUAL[Eventual Consistency Between Aggregates]
        INVARIANTS[Protect Business Invariants]
    end
    
    CONSISTENCY --> SMALL
    BOUNDARIES --> REFERENCES
    IDENTITY --> EVENTUAL
    TRANSACTIONS --> INVARIANTS
```

### Domain Events

```mermaid
sequenceDiagram
    participant AGGREGATE as Aggregate Root
    participant HANDLER as Domain Event Handler
    participant SERVICE as Application Service
    participant EXTERNAL as External System
    
    Note over AGGREGATE: Business Operation
    AGGREGATE->>AGGREGATE: Execute Business Logic
    AGGREGATE->>AGGREGATE: Raise Domain Event
    AGGREGATE->>HANDLER: Publish Event
    HANDLER->>SERVICE: Handle Event
    SERVICE->>EXTERNAL: Update External System
    SERVICE->>HANDLER: Confirm
    HANDLER->>AGGREGATE: Event Handled
```

---

## 🌐 Dynamic API-Driven Frontend

StudyBridge frontend is designed to be completely dynamic and API-driven, eliminating hardcoded configurations and making the system adaptable to backend changes.

### Frontend Architecture

```mermaid
graph TB
    subgraph "Angular 20 Frontend Architecture"
        subgraph "Presentation Layer"
            LAYOUTS[Layout Components]
            COMPONENTS[Feature Components]
            GUARDS[Route Guards]
        end
        
        subgraph "State Management"
            SIGNALS[Angular Signals]
            SERVICES[Injectable Services]
            STORES[Local State Stores]
        end
        
        subgraph "API Integration"
            HTTP_CLIENT[HTTP Client]
            API_SERVICES[API Services]
            INTERCEPTORS[HTTP Interceptors]
        end
        
        subgraph "Dynamic Configuration"
            MENU_SERVICE[Menu Service]
            AUTH_SERVICE[Auth Service]
            PERMISSION_SERVICE[Permission Service]
        end
    end
    
    LAYOUTS --> SERVICES
    COMPONENTS --> SERVICES
    GUARDS --> AUTH_SERVICE
    SERVICES --> API_SERVICES
    API_SERVICES --> HTTP_CLIENT
    HTTP_CLIENT --> INTERCEPTORS
    MENU_SERVICE --> API_SERVICES
    PERMISSION_SERVICE --> API_SERVICES
```

### Dynamic Menu System

```mermaid
sequenceDiagram
    participant USER as User
    participant FRONTEND as Angular Frontend
    participant MENU_SVC as Menu Service
    participant BACKEND as Backend API
    participant DB as Database
    
    USER->>FRONTEND: Login/Access Application
    FRONTEND->>MENU_SVC: Request Menus for User
    MENU_SVC->>BACKEND: GET /api/admin/menus/user-menus
    BACKEND->>DB: Query User Permissions & Menus
    DB->>BACKEND: Return Filtered Menus
    BACKEND->>MENU_SVC: Return Menu Structure
    MENU_SVC->>FRONTEND: Provide Dynamic Menus
    FRONTEND->>USER: Render User-Specific Navigation
    
    Note over MENU_SVC: Fallback to default menus if API fails
    Note over FRONTEND: No hardcoded menu items
```

### Role-Based Layout Selection

```mermaid
flowchart TD
    subgraph "Dynamic Layout Architecture"
        USER_LOGIN[User Login]
        AUTH_CHECK{Check User Role}
        
        ADMIN_LAYOUT[Admin Layout<br/>- Collapsible Sidebar<br/>- Admin Navigation<br/>- Full Management UI]
        PUBLIC_LAYOUT[Public Layout<br/>- Top Navigation Bar<br/>- User-Friendly UI<br/>- Learning Interface]
        
        ADMIN_MENUS[Dynamic Admin Menus<br/>- User Management<br/>- Content Management<br/>- System Configuration]
        PUBLIC_MENUS[Dynamic Public Menus<br/>- Dashboard<br/>- Vocabulary<br/>- Learning Modules]
    end
    
    USER_LOGIN --> AUTH_CHECK
    AUTH_CHECK -->|Admin Roles| ADMIN_LAYOUT
    AUTH_CHECK -->|User Role| PUBLIC_LAYOUT
    ADMIN_LAYOUT --> ADMIN_MENUS
    PUBLIC_LAYOUT --> PUBLIC_MENUS
    
    style ADMIN_LAYOUT fill:#e1f5fe
    style PUBLIC_LAYOUT fill:#f3e5f5
```

### API-Driven Configuration

```mermaid
graph LR
    subgraph "Configuration Sources"
        MENUS_API[Menus API<br/>/api/admin/menus]
        PERMISSIONS_API[Permissions API<br/>/api/admin/permissions]
        USER_API[User API<br/>/api/profile]
        ROLES_API[Roles API<br/>/api/admin/roles]
    end
    
    subgraph "Frontend Services"
        MENU_SERVICE[MenuService<br/>- Dynamic menu loading<br/>- Fallback handling<br/>- Hierarchical structure]
        AUTH_SERVICE[AuthService<br/>- Role detection<br/>- Permission checking<br/>- Route redirection]
        PERMISSION_SERVICE[PermissionService<br/>- Dynamic permissions<br/>- Access control<br/>- UI element visibility]
    end
    
    MENUS_API --> MENU_SERVICE
    PERMISSIONS_API --> PERMISSION_SERVICE
    USER_API --> AUTH_SERVICE
    ROLES_API --> AUTH_SERVICE
    
    MENU_SERVICE --> |No Hardcoded Menus| LAYOUTS
    AUTH_SERVICE --> |Dynamic Role Check| GUARDS
    PERMISSION_SERVICE --> |Runtime Permissions| COMPONENTS
```

---

## 🛡️ Role-Based Access Control (RBAC)

StudyBridge implements a simplified menu-based RBAC system that dynamically controls access to features and data based on user roles and their associated menus.

### Simplified RBAC Architecture

```mermaid
graph TB
    subgraph "Simplified RBAC System"
        subgraph "User Context"
            USER[User Account]
            USER_ROLES[User Roles]
            USER_MENUS[Accessible Menus]
        end
        
        subgraph "Menu System"
            ROLES[System Roles]
            MENUS[System Menus]
            ROLE_MENUS[Role-Menu Mapping]
        end
        
        subgraph "Access Control"
            MENU_ACCESS[Menu Access Control]
            ROUTE_GUARDS[Route Guards]
            COMPONENT_ACCESS[Component-Level Access]
            API_ACCESS[API Endpoint Access]
        end
        
        subgraph "Dynamic Enforcement"
            FRONTEND_AUTH[Frontend Authorization]
            BACKEND_AUTH[Backend Authorization]
            REAL_TIME_CHECKS[Real-time Menu Checks]
        end
    end
    
    USER --> USER_ROLES
    USER_ROLES --> ROLE_MENUS
    ROLE_MENUS --> MENUS
    MENUS --> USER_MENUS
    
    USER_MENUS --> MENU_ACCESS
    USER_MENUS --> ROUTE_GUARDS
    USER_MENUS --> COMPONENT_ACCESS
    USER_MENUS --> API_ACCESS
    
    MENU_ACCESS --> FRONTEND_AUTH
    ROUTE_GUARDS --> FRONTEND_AUTH
    COMPONENT_ACCESS --> FRONTEND_AUTH
    API_ACCESS --> BACKEND_AUTH
    
    FRONTEND_AUTH --> REAL_TIME_CHECKS
    BACKEND_AUTH --> REAL_TIME_CHECKS
```

### Dynamic Menu Access Evaluation

```mermaid
sequenceDiagram
    participant COMP as Component
    participant AUTH_SVC as AuthService
    participant MENU_SVC as MenuService
    participant API as Backend API
    
    COMP->>AUTH_SVC: Check User Access
    AUTH_SVC->>MENU_SVC: Get User Menus
    MENU_SVC->>API: Fetch Accessible Menus
    API->>MENU_SVC: Return Menu List
    MENU_SVC->>COMP: Provide Authorized Content
    
    Note over COMP: UI updates dynamically based on menu access
    Note over API: All menu access verified server-side
```

### Role-Based Menu Filtering

```mermaid
flowchart TD
    subgraph "Simplified Menu Access Control Flow"
        USER_LOGIN[User Authentication]
        LOAD_ROLES[Load User Roles]
        FETCH_ROLE_MENUS[Fetch Role Menus]
        GET_MENUS[Get Available Menus]
        FILTER_MENUS[Filter by Role Menus]
        RENDER_UI[Render Authorized UI]
        
        MENU_ACCESS_CHECK{Has Menu Access?}
        MENU_ITEM[Menu Item]
        INCLUDE_MENU[Include in Navigation]
        EXCLUDE_MENU[Hide from Navigation]
    end
    
    USER_LOGIN --> LOAD_ROLES
    LOAD_ROLES --> FETCH_ROLE_MENUS
    FETCH_ROLE_MENUS --> GET_MENUS
    GET_MENUS --> FILTER_MENUS
    FILTER_MENUS --> MENU_ACCESS_CHECK
    MENU_ACCESS_CHECK -->|Yes| INCLUDE_MENU
    MENU_ACCESS_CHECK -->|No| EXCLUDE_MENU
    INCLUDE_MENU --> RENDER_UI
    
    style INCLUDE_MENU fill:#c8e6c9
    style EXCLUDE_MENU fill:#ffcdd2
```

### System Roles and Menu Access

```mermaid
graph TB
    subgraph "StudyBridge Simplified Role System"
        subgraph "Administrative Roles"
            SUPER_ADMIN[SuperAdmin<br/>- Full system access<br/>- User management<br/>- System configuration]
            ADMIN[Admin<br/>- User management<br/>- Content management<br/>- Analytics access]
            CONTENT_MANAGER[ContentManager<br/>- Vocabulary management<br/>- Learning content<br/>- Progress tracking]
        end
        
        subgraph "User Roles"
            USER[User<br/>- Learning dashboard<br/>- Vocabulary practice<br/>- Progress viewing<br/>- Profile management]
        end
        
        subgraph "Menu Categories"
            SYSTEM_MENUS[System Menus<br/>- system.manage<br/>- system.view<br/>- system.configure]
            USER_MENUS[User Menus<br/>- users.view<br/>- users.create<br/>- users.edit<br/>- users.delete]
            CONTENT_MENUS[Content Menus<br/>- vocabulary.manage<br/>- ielts.manage<br/>- content.view]
            PUBLIC_MENUS[Public Menus<br/>- dashboard.view<br/>- vocabulary.practice<br/>- learning.access]
        end
    end
    
    SUPER_ADMIN --> SYSTEM_MENUS
    SUPER_ADMIN --> USER_MENUS
    SUPER_ADMIN --> CONTENT_MENUS
    
    ADMIN --> USER_MENUS
    ADMIN --> CONTENT_MENUS
    
    CONTENT_MANAGER --> CONTENT_MENUS
    
    USER --> PUBLIC_MENUS
```

---

## 💉 Dependency Injection

StudyBridge uses .NET's built-in DI container with custom service registration patterns.

### Service Registration Strategy

```mermaid
graph TB
    subgraph "Service Registration Layers"
        subgraph "API Layer"
            API_SERVICES[API Services]
            MIDDLEWARE_SERVICES[Middleware]
            AUTH_SERVICES[Authentication]
        end
        
        subgraph "Module Services"
            MODULE_EXT[Module Extensions]
            FEATURE_HANDLERS[Feature Handlers]
            VALIDATORS[Validators]
        end
        
        subgraph "Infrastructure Services"
            DB_CONTEXT[DbContext]
            REPOSITORIES[Repositories]
            EXTERNAL_SERVICES[External Services]
        end
        
        subgraph "Shared Services"
            CQRS_SERVICES[CQRS Services]
            COMMON_SERVICES[Common Services]
            UTILITIES[Utilities]
        end
    end
    
    API_SERVICES --> MODULE_EXT
    MODULE_EXT --> FEATURE_HANDLERS
    MODULE_EXT --> VALIDATORS
    API_SERVICES --> DB_CONTEXT
    DB_CONTEXT --> REPOSITORIES
    REPOSITORIES --> EXTERNAL_SERVICES
    MODULE_EXT --> CQRS_SERVICES
    CQRS_SERVICES --> COMMON_SERVICES
    COMMON_SERVICES --> UTILITIES
```

### Service Lifetime Management

```mermaid
graph LR
    subgraph "Service Lifetimes"
        SINGLETON[Singleton]
        SCOPED[Scoped]
        TRANSIENT[Transient]
    end
    
    subgraph "Singleton Services"
        CONFIG[Configuration]
        LOGGING[Logging]
        CACHE[Caching]
    end
    
    subgraph "Scoped Services"
        DBCONTEXT[DbContext]
        HANDLERS[Handlers]
        SERVICES[Application Services]
    end
    
    subgraph "Transient Services"
        VALIDATORS[Validators]
        HELPERS[Helper Classes]
        FACTORIES[Factories]
    end
    
    SINGLETON --> CONFIG
    SINGLETON --> LOGGING
    SINGLETON --> CACHE
    
    SCOPED --> DBCONTEXT
    SCOPED --> HANDLERS
    SCOPED --> SERVICES
    
    TRANSIENT --> VALIDATORS
    TRANSIENT --> HELPERS
    TRANSIENT --> FACTORIES
```

---

## 📊 Data Access Patterns

### Repository Pattern Implementation

```mermaid
graph TB
    subgraph "Data Access Layer"
        subgraph "Abstractions"
            IAPPDB[IApplicationDbContext]
            IREPO[IRepository&lt;T&gt;]
            IUNITOFWORK[IUnitOfWork]
        end
        
        subgraph "Implementations"
            APPDB[AppDbContext]
            GENERIC_REPO[GenericRepository&lt;T&gt;]
            SPECIFIC_REPOS[Specific Repositories]
        end
        
        subgraph "Entity Framework"
            DBSETS[DbSets]
            MIGRATIONS[Migrations]
            CONFIGURATIONS[Entity Configurations]
        end
    end
    
    IAPPDB --> APPDB
    IREPO --> GENERIC_REPO
    IREPO --> SPECIFIC_REPOS
    APPDB --> DBSETS
    APPDB --> CONFIGURATIONS
    DBSETS --> MIGRATIONS
```

### Entity Configuration Pattern

```mermaid
graph TB
    subgraph "Entity Framework Configuration"
        subgraph "Base Configuration"
            BASE_ENTITY[BaseEntity Config]
            AUDIT_ENTITY[BaseAuditableEntity Config]
        end
        
        subgraph "Specific Configurations"
            USER_CONFIG[AppUser Configuration]
            PROFILE_CONFIG[UserProfile Configuration]
            ROLE_CONFIG[Role Configuration]
        end
        
        subgraph "Configuration Features"
            RELATIONSHIPS[Relationships]
            CONSTRAINTS[Constraints]
            INDEXES[Indexes]
            CONVERSIONS[Value Conversions]
        end
    end
    
    BASE_ENTITY --> USER_CONFIG
    BASE_ENTITY --> PROFILE_CONFIG
    AUDIT_ENTITY --> ROLE_CONFIG
    
    USER_CONFIG --> RELATIONSHIPS
    USER_CONFIG --> CONSTRAINTS
    PROFILE_CONFIG --> INDEXES
    ROLE_CONFIG --> CONVERSIONS
```

### Query Optimization Patterns

```mermaid
graph LR
    subgraph "Query Strategies"
        PROJECTION[Select Projections]
        FILTERING[Where Filtering]
        INCLUDES[Include Related Data]
        SPLITTING[Split Queries]
    end
    
    subgraph "Performance Techniques"
        ASYNC[Async Operations]
        TRACKING[No Tracking Queries]
        COMPILED[Compiled Queries]
        BULK[Bulk Operations]
    end
    
    PROJECTION --> ASYNC
    FILTERING --> TRACKING
    INCLUDES --> COMPILED
    SPLITTING --> BULK
```

---

## 🔧 Cross-Cutting Concerns

### Logging Strategy

```mermaid
graph TB
    subgraph "Logging Architecture"
        subgraph "Logging Levels"
            TRACE[Trace - Detailed Debug]
            DEBUG[Debug - Development Info]
            INFO[Information - General Flow]
            WARN[Warning - Unexpected Events]
            ERROR[Error - Failures]
            CRITICAL[Critical - App Crashes]
        end
        
        subgraph "Logging Targets"
            CONSOLE[Console Output]
            FILES[Rolling Files]
            STRUCTURED[Structured Logging]
            EXTERNAL[External Services]
        end
        
        subgraph "Serilog Configuration"
            ENRICHERS[Context Enrichers]
            FILTERS[Log Filters]
            SINKS[Output Sinks]
        end
    end
    
    TRACE --> CONSOLE
    DEBUG --> CONSOLE
    INFO --> FILES
    WARN --> FILES
    ERROR --> STRUCTURED
    CRITICAL --> EXTERNAL
    
    ENRICHERS --> SINKS
    FILTERS --> SINKS
```

### Exception Handling Strategy

```mermaid
graph TB
    subgraph "Exception Handling Flow"
        EXCEPTION[Exception Thrown]
        GLOBAL_HANDLER[Global Exception Handler]
        LOGGER[Exception Logger]
        MAPPER[Status Code Mapper]
        RESPONSE[Error Response]
        CLIENT[Client Response]
    end
    
    subgraph "Exception Types"
        VALIDATION[ValidationException → 400]
        NOTFOUND[NotFoundException → 404]
        UNAUTHORIZED[UnauthorizedException → 401]
        FORBIDDEN[ForbiddenException → 403]
        CONFLICT[ConflictException → 409]
        BUSINESS[BusinessLogicException → 422]
        SYSTEM[SystemException → 500]
    end
    
    EXCEPTION --> GLOBAL_HANDLER
    GLOBAL_HANDLER --> LOGGER
    GLOBAL_HANDLER --> MAPPER
    MAPPER --> RESPONSE
    RESPONSE --> CLIENT
    
    VALIDATION --> MAPPER
    NOTFOUND --> MAPPER
    UNAUTHORIZED --> MAPPER
    FORBIDDEN --> MAPPER
    CONFLICT --> MAPPER
    BUSINESS --> MAPPER
    SYSTEM --> MAPPER
```

### Security Implementation

```mermaid
graph TB
    subgraph "Security Layers"
        subgraph "Authentication"
            JWT[JWT Tokens]
            GOOGLE[Google OAuth]
            LOCAL[Local Authentication]
        end
        
        subgraph "Authorization"
            ROLES[Role-Based Access]
            PERMISSIONS[Permission-Based Access]
            POLICIES[Custom Policies]
        end
        
        subgraph "Data Protection"
            HASHING[Password Hashing]
            ENCRYPTION[Data Encryption]
            SANITIZATION[Input Sanitization]
        end
        
        subgraph "Security Headers"
            CORS[CORS Configuration]
            HTTPS[HTTPS Enforcement]
            HEADERS[Security Headers]
        end
    end
    
    JWT --> ROLES
    GOOGLE --> ROLES
    LOCAL --> HASHING
    ROLES --> PERMISSIONS
    PERMISSIONS --> POLICIES
    HASHING --> ENCRYPTION
    ENCRYPTION --> SANITIZATION
    POLICIES --> CORS
    CORS --> HTTPS
    HTTPS --> HEADERS
```

---

## 📈 Scalability Considerations

### Horizontal Scaling Preparation

```mermaid
graph TB
    subgraph "Scalability Strategies"
        subgraph "Current Architecture"
            MONOLITH[Modular Monolith]
            SHARED_DB[Shared Database]
            STATELESS[Stateless Services]
        end
        
        subgraph "Future Microservices"
            USER_SVC[User Service]
            VOCAB_SVC[Vocabulary Service]
            LEARNING_SVC[Learning Service]
            PROGRESS_SVC[Progress Service]
        end
        
        subgraph "Data Strategy"
            DB_PER_SERVICE[Database per Service]
            EVENT_SOURCING[Event Sourcing]
            CQRS_SEPARATION[CQRS Read/Write Split]
        end
    end
    
    MONOLITH --> USER_SVC
    MONOLITH --> VOCAB_SVC
    MONOLITH --> LEARNING_SVC
    MONOLITH --> PROGRESS_SVC
    
    USER_SVC --> DB_PER_SERVICE
    VOCAB_SVC --> EVENT_SOURCING
    LEARNING_SVC --> CQRS_SEPARATION
```

### Performance Optimization Patterns

```mermaid
graph LR
    subgraph "Performance Strategies"
        subgraph "Caching"
            MEMORY[Memory Cache]
            DISTRIBUTED[Distributed Cache]
            CDN[CDN Caching]
        end
        
        subgraph "Database"
            INDEXING[Database Indexing]
            QUERY_OPT[Query Optimization]
            CONNECTION_POOL[Connection Pooling]
        end
        
        subgraph "Application"
            ASYNC[Async Programming]
            BULK_OPS[Bulk Operations]
            LAZY_LOADING[Lazy Loading]
        end
    end
    
    MEMORY --> INDEXING
    DISTRIBUTED --> QUERY_OPT
    CDN --> CONNECTION_POOL
    INDEXING --> ASYNC
    QUERY_OPT --> BULK_OPS
    CONNECTION_POOL --> LAZY_LOADING
```

### Monitoring & Observability

```mermaid
graph TB
    subgraph "Observability Stack"
        subgraph "Metrics"
            APP_METRICS[Application Metrics]
            PERF_COUNTERS[Performance Counters]
            CUSTOM_METRICS[Custom Metrics]
        end
        
        subgraph "Logging"
            STRUCTURED_LOGS[Structured Logging]
            CORRELATION[Correlation IDs]
            LOG_AGGREGATION[Log Aggregation]
        end
        
        subgraph "Tracing"
            DISTRIBUTED_TRACING[Distributed Tracing]
            REQUEST_TRACKING[Request Tracking]
            DEPENDENCY_TRACKING[Dependency Tracking]
        end
        
        subgraph "Health Checks"
            APP_HEALTH[Application Health]
            DB_HEALTH[Database Health]
            EXTERNAL_HEALTH[External Service Health]
        end
    end
    
    APP_METRICS --> STRUCTURED_LOGS
    PERF_COUNTERS --> CORRELATION
    CUSTOM_METRICS --> LOG_AGGREGATION
    
    STRUCTURED_LOGS --> DISTRIBUTED_TRACING
    CORRELATION --> REQUEST_TRACKING
    LOG_AGGREGATION --> DEPENDENCY_TRACKING
    
    DISTRIBUTED_TRACING --> APP_HEALTH
    REQUEST_TRACKING --> DB_HEALTH
    DEPENDENCY_TRACKING --> EXTERNAL_HEALTH
```

---

---

## 📝 Implementation Notes

### Recent Updates (September 2025)

**Dynamic API Integration Completed:**
- ✅ Removed all hardcoded admin roles from `AuthService`
- ✅ Replaced hardcoded permissions mapping with API-driven approach
- ✅ Cleaned up hardcoded menu items in layout components
- ✅ Implemented fallback mechanisms for graceful API failure handling
- ✅ Added proper role-based layout selection (Admin vs User)
- ✅ Enhanced MenuService with dynamic loading capabilities

**Key Changes Made:**
1. **AuthService Improvements:**
   - `hasAdminRole()`: Now dynamically determines admin roles instead of hardcoded list
   - `isAdminUser()`: Uses API-provided role data for authorization decisions
   - Removed hardcoded role arrays: `['Admin', 'SuperAdmin', 'Administrator', ...]`

2. **Layout Components:**
   - **AdminLayoutComponent**: Uses `MenuService.loadAdminMenus()` for dynamic navigation
   - **PublicLayoutComponent**: Integrated with `MenuService.loadPublicMenus()` 
   - Fallback menus provide minimal navigation when API is unavailable

3. **Menu System:**
   - **MenuService**: Enhanced with hierarchical menu building and permission filtering
   - Backend integration via `/api/admin/menus/user-menus` and `/api/admin/public-menus`
   - Automatic fallback to default menus if API calls fail

4. **Security & Permissions:**
   - Dynamic permission checking based on API responses
   - Real-time role validation without hardcoded assumptions
   - Graceful handling of missing permissions data

**Migration Path for Microservices:**
The current implementation is designed to easily transition from the modular monolith to microservices:
- Service abstractions allow easy extraction to separate APIs
- Event-driven communication patterns already implemented  
- Database per service preparation through module boundaries
- CQRS foundation supports distributed query/command handling

---

*This architecture documentation provides deep insights into StudyBridge's technical implementation and design decisions. It serves as a guide for developers working on the system and for future architectural evolution.*
