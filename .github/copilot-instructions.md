# StudyBridge - AI Coding Agent Instructions

## 🎯 Essential Knowledge for Immediate Productivity

StudyBridge is a production-ready IELTS vocabulary platform with **258 tests (92.2% coverage)** and modern architecture.

## 🏗️ Architecture Overview

**Backend**: .NET 8 with custom CQRS + Clean Architecture  
**Frontend**: Angular 20 standalone components + PrimeNG  
**Database**: PostgreSQL with Entity Framework Core  
**Authentication**: Google OAuth + JWT tokens  

## ⚡ CQRS Pattern (Critical)

Every backend feature follows this exact structure in `StudyBridge/Modules/{Module}/Features/{Feature}.cs`:

```csharp
public static class Login // Example from actual codebase
{
    public class Command : ICommand<Response>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator() 
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(1);
        }
    }

    public class Response
    {
        public string Token { get; init; } = string.Empty;
        public string UserId { get; init; } = string.Empty;
        // ... other properties
    }

    public class Handler : ICommandHandler<Command, Response>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(IApplicationDbContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Response> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            // Implementation with logging
            _logger.LogInformation("Processing {Feature} for {User}", nameof(Login), command.Email);
            // Business logic here
            return new Response { /* ... */ };
        }
    }
}
```

**Controllers** dispatch to handlers via custom `IDispatcher`:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login.Command command)
    {
        var result = await _authenticationService.LoginAsync(command);
        return HandleServiceResult(result); // Returns ApiResponse<T>
    }
}
```

## 🎨 Angular Frontend Patterns

**Standalone Components** with signals (from actual codebase):
```typescript
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html'
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  // Modern Angular signals
  isLoading = signal(false);
  user = signal<UserDto | null>(null);
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => this.user.set(user));
  }
}
```

**API Services** pattern matching backend exactly:
```typescript
@Injectable({ providedIn: 'root' })
export class AuthApiService {
  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>('/api/v1/auth/login', request);
  }
}
```

## � Critical Development Rules

### Backend
- **CQRS Only**: Every feature = Command/Query + Validator + Handler + Response
- **Custom CQRS**: Uses `IDispatcher` (no MediatR dependency)  
- **API Wrapper**: All endpoints return `ApiResponse<T>`
- **Testing**: AAA pattern with MockQueryable.EF for EF mocking

### Frontend  
- **Standalone Components**: No NgModules for components
- **PrimeNG Only**: All UI components must use PrimeNG
- **Signal State**: Use Angular signals for reactive state
- **API Sync**: TypeScript interfaces MUST match backend DTOs exactly

## 🧪 Testing Standards

**Backend Tests** (258 existing tests as reference):
```csharp
public class LoginTests
{
    [Fact]
    public async Task HandleAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var command = new Login.Command { Email = "test@test.com", Password = "password" };
        
        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);
        
        // Assert
        Assert.NotEmpty(result.Token);
        _mockContext.Verify(c => c.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
```

## 🔧 Development Workflow

```bash
# Backend
cd StudyBridge/StudyBridge.Api
dotnet run  # Runs on http://localhost:5000

# Frontend  
cd Client/Web/studybridge-web
ng serve   # Runs on http://localhost:4200

# Tests
dotnet test # 258 tests should pass
ng test     # Angular tests
```

## 📂 Key File Locations

**Backend CQRS Features**: `StudyBridge/Modules/StudyBridge.UserManagement/Features/`  
**Angular Services**: `Client/Web/studybridge-web/src/app/shared/services/`  
**API Controllers**: `StudyBridge/StudyBridge.Api/Controllers/`  
**Test Suite**: `StudyBridge/StudyBridge.Tests.Unit/`

## 🎯 Critical Integration Points

- **Authentication**: JWT tokens via `AuthService` and `AuthInterceptor`
- **Menu System**: Dynamic navigation via `MenuService` from backend API
- **Error Handling**: Global error interceptor + `NotificationService` toasts  
- **Database**: EF Core with explicit entity configurations
- **API**: Custom `BaseController` with `HandleServiceResult` pattern

**Read `/docs/` folder for deep architectural details** - this is the essential knowledge for immediate productivity.
