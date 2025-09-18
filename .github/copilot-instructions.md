# GitHub Copilot Instructions for StudyBridge Project

## 📋 Project Context

StudyBridge is an IELTS vocabulary learning platform built with .NET 8 (backend) and Angular 20+ (frontend), following Clean Architecture and CQRS patterns.

## 🏗️ Architecture Overview

- **Backend**: .NET 8 Web API with modular monolithic architecture
- **Frontend**: Angular with zoneless change detection
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT + Google OAuth
- **Pattern**: Clean Architecture + CQRS + Domain-Driven Design

## 📚 Documentation References

When working on this project, always reference these comprehensive documentation files:

### ⚠️ MANDATORY READING BEFORE ANY WORK
- **`/docs/UI_COMPONENTS_GUIDE.md`**: **CRITICAL - MUST READ FIRST** - UI component architecture and prohibited practices

### Core Documentation
- **`/docs/IMPLEMENTATION_STATUS.md`**: Current project status and completed features (September 2025)
- **`/docs/PROJECT_DOCUMENTATION.md`**: Complete project overview with Mermaid diagrams
- **`/docs/ARCHITECTURE.md`**: Technical architecture deep dive
- **`/docs/API_REFERENCE.md`**: Complete API endpoint documentation  
- **`/docs/DEVELOPMENT_GUIDE.md`**: Development workflows and standards
- **`/docs/INDEX.md`**: Documentation navigation hub
- **`/CONFIGURATION_SECURITY.md`**: Secure configuration management guide

## 🎯 Key Patterns to Follow

### CQRS Feature Pattern
```csharp
// Follow this pattern for all features
public static class FeatureName
{
    public class Command : ICommand<Response> { }
    public class Validator : AbstractValidator<Command> { }
    public class Response { }
    public class Handler : ICommandHandler<Command, Response> { }
}
```

### Module Structure
- **StudyBridge.UserManagement**: Complete (authentication, profiles)
- **StudyBridge.VocabularyManagement**: Planned (2100+ IELTS words)
- **StudyBridge.LearningManagement**: Planned (SRS algorithm)
- **StudyBridge.ProgressManagement**: Planned (analytics, streaks)

### Domain Entities
- **AppUser**: User accounts with OAuth/local auth
- **UserProfile**: Extended user information
- **Role/Permission**: RBAC system
- **UserSubscription**: Subscription management

### Technology Stack
- **.NET 8**: Latest LTS framework
- **Entity Framework Core**: ORM with PostgreSQL
- **FluentValidation**: Input validation
- **Serilog**: Structured logging
- **xUnit + Moq**: Testing (258 tests, 92.2% business logic coverage)
- **Angular 20**: Modern frontend with standalone components
- **TypeScript 5.9**: Strict mode with comprehensive type safety
- **Tailwind CSS 3.4**: Utility-first styling with responsive design
- **RxJS 7.8**: Reactive programming for state management
- **Custom CQRS**: No MediatR dependency

## 🔧 Development Guidelines

### Naming Conventions
- **Classes/Interfaces**: PascalCase (UserService, IUserService)
- **Methods/Properties**: PascalCase (GetUserById)
- **Private fields**: camelCase with underscore (_userRepository)
- **Variables/Parameters**: camelCase (userId, emailAddress)

### Code Standards
- Always use dependency injection
- Follow Clean Architecture layer dependencies
- Implement comprehensive unit tests
- Use FluentValidation for input validation
- Apply async/await patterns consistently
- Use structured logging with Serilog

### API Patterns
- Use ApiResponse<T> wrapper for all endpoints
- Implement proper HTTP status codes
- Follow RESTful conventions
- Use JWT Bearer authentication
- Apply role-based authorization

### Testing Standards
- Follow AAA pattern (Arrange, Act, Assert)
- Use test data builders for complex objects
- Mock external dependencies
- Aim for 90%+ coverage on business logic
- Write descriptive test names: `Method_Scenario_ExpectedResult`

## 🚀 Current Status (September 2025)

### Completed ✅
- User Management module (92.2% test coverage)
- Authentication system (Google OAuth + JWT)
- Clean Architecture foundation
- CQRS infrastructure with custom implementation
- Comprehensive testing suite (258 tests passing)
- Database schema and migrations
- **Angular 20 frontend with Tailwind CSS**
- **Responsive dashboard and authentication UI**
- **Secure configuration management**
- **Modern component architecture with standalone components**

### In Progress 🚧
- Enhanced documentation and developer guides
- CI/CD pipeline setup

### Planned 📋
- Vocabulary Management module (2,100+ IELTS words)
- Learning Engine with SRS algorithm
- Progress tracking and analytics
- Mobile application (React Native)

## 🎯 When Generating Code

### ⚠️ CRITICAL REQUIREMENTS (MUST BE FOLLOWED)
1. **READ `/docs/UI_COMPONENTS_GUIDE.md` FIRST** - Contains mandatory UI component requirements
2. **NEVER create new Table, Form, or Tree components** - Use existing PrimeNG wrappers:
   - Tables: `app-table-wrapper` (comprehensive PrimeNG Table)
   - Forms: `app-dynamic-form` (full PrimeNG Form capabilities)  
   - Trees: `app-tree-wrapper` (complete PrimeNG Tree implementation)
3. **ALWAYS use PrimeNG for all UI components** - No other UI libraries allowed

### Standard Development Rules
4. **Always follow established patterns** from existing modules
5. **Reference documentation** for architectural decisions
6. **Maintain consistency** with naming conventions
7. **Include comprehensive tests** for new features
8. **Follow Clean Architecture** layer dependencies
9. **Use proper error handling** with custom exceptions
10. **Implement validation** using FluentValidation
11. **Add logging** for important operations
12. **Follow CQRS pattern** for commands and queries
13. **Update documentation** when adding new features

## 📁 Project Structure Reference

```
StudyBridge/
├── StudyBridge.Api/              # Presentation layer
├── StudyBridge.Domain/           # Domain entities
├── StudyBridge.Application/      # Application services
├── StudyBridge.Infrastructure/   # Infrastructure layer
├── StudyBridge.Shared/          # Shared components
├── Modules/                     # Feature modules
│   └── StudyBridge.UserManagement/
└── StudyBridge.Tests.Unit/      # Comprehensive tests
```

This project follows production-ready patterns and maintains high code quality standards. Always ensure new code aligns with these established conventions and architectural principles.
