# StudyBridge

A comprehensive IELTS vocabulary learning platform for Bangladeshi students built with .NET 8 and Angular 20.

> **📚 For GitHub Copilot Context**: This project includes comprehensive documentation in the `/docs` folder. Always reference these files for architectural patterns, coding standards, and development guidelines:
> - [`/docs/INDEX.md`](./docs/INDEX.md) - Documentation hub
> - [`/docs/IMPLEMENTATION_STATUS.md`](./docs/IMPLEMENTATION_STATUS.md) - **Current progress (September 2025)**
> - [`/docs/PROJECT_DOCUMENTATION.md`](./docs/PROJECT_DOCUMENTATION.md) - Complete overview
> - [`/docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md) - Technical deep dive  
> - [`/docs/API_REFERENCE.md`](./docs/API_REFERENCE.md) - API documentation
> - [`/docs/DEVELOPMENT_GUIDE.md`](./docs/DEVELOPMENT_GUIDE.md) - Developer guide
> - [`/CONFIGURATION_SECURITY.md`](./CONFIGURATION_SECURITY.md) - Secure configuration guide
> - [`.github/copilot-instructions.md`](./.github/copilot-instructions.md) - Copilot context

## ⚡ Quick Status (September 2025)

**🎯 Production Ready**: Complete authentication system with modern Angular frontend
- **258 tests passing** with 92.2% business logic coverage
- **Angular 20** with Tailwind CSS and responsive design
- **Secure configuration** management with GitHub Push Protection compliance
- **Google OAuth 2.0** + JWT authentication fully implemented

## Architecture

- **Backend**: .NET 8 Web API with Clean Architecture and modular monolithic design
- **Frontend**: Angular 20 with standalone components and Tailwind CSS
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: Google OAuth 2.0 with JWT tokens
- **Testing**: Comprehensive test suite with xUnit, Moq, and FluentAssertions

## Project Structure

```
StudyBridge/
├── src/
│   ├── Shared/
│   │   └── StudyBridge.Shared/          # Common types, CQRS, utilities
│   ├── Modules/
│   │   └── StudyBridge.UserManagement/  # User auth and management
│   └── StudyBridge.Api/                 # Main API project
└── StudyBridge.sln

Client/
└── Web/
    └── studybridge-web/                 # Angular frontend
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js (latest LTS)
- PostgreSQL
- Angular CLI

### Backend Setup

1. **Database Setup**:
   ```bash
   # Update connection string in appsettings.json
   # Create database (EF will create tables automatically)
   ```

2. **API Configuration**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=studybridge_dev;Username=postgres;Password=your_password"
     },
     "JwtSettings": {
       "SecretKey": "your-super-secret-jwt-key-here-min-32-chars",
       "ExpiryInDays": 7
     },
     "GoogleAuth": {
       "ClientId": "your-google-client-id.apps.googleusercontent.com"
     }
   }
   ```

3. **Run API**:
   ```bash
   cd StudyBridge/src/StudyBridge.Api
   dotnet run
   ```

### Frontend Setup

1. **Install Dependencies**:
   ```bash
   cd Client/Web/studybridge-web
   npm install
   ```

2. **Configure Google OAuth**:
   - Update Google Client ID in `login.component.ts`
   - Add Google OAuth script to `index.html`

3. **Run Angular App**:
   ```bash
   ng serve
   ```

## Features Implemented (September 2025)

### Backend (API) ✅
- ✅ Modular monolithic architecture with Clean Architecture
- ✅ Custom CQRS implementation (no paid dependencies)
- ✅ Complete User Management module
- ✅ Google OAuth 2.0 authentication
- ✅ JWT token generation and validation
- ✅ Secure configuration management
- ✅ Comprehensive test suite (258 tests, 92.2% coverage)
- ✅ ASP.NET Core Identity integration
- ✅ FluentValidation for input validation
- ✅ Structured logging with Serilog

### Frontend (Angular 20) ✅
- ✅ Modern standalone components architecture
- ✅ Login and registration with Google OAuth
- ✅ Responsive dashboard with user profile display
- ✅ Reactive forms with comprehensive validation
- ✅ Tailwind CSS styling with mobile-first design
- ✅ TypeScript 5.9 with strict mode
- ✅ RxJS state management
- ✅ Route guards and protected navigation
- ✅ Toast notifications and loading states
- ✅ JWT token management and HTTP interceptors

### Infrastructure ✅
- ✅ PostgreSQL database with Entity Framework Core
- ✅ Generic API response wrapper
- ✅ CORS configuration for Angular integration
- ✅ Secure configuration with environment-based secrets
- ✅ GitHub Push Protection compliance

## 🔄 Next Phase
- 🚧 Vocabulary Management module (2,100+ IELTS words)
- 🚧 Learning Engine with Spaced Repetition System
- 🚧 Progress tracking and analytics
- 🚧 Mobile application (React Native)

## 📊 Testing Coverage
- **Total Tests**: 258 (all passing ✅)
- **Business Logic**: 92.2% coverage
- **Testing Tools**: xUnit, Moq, FluentAssertions, MockQueryable.EF

## 🚀 Quick Start

**⚠️ Important**: This project uses secure configuration management. See [`CONFIGURATION_SECURITY.md`](./CONFIGURATION_SECURITY.md) for setup details.

1. **Clone and setup backend**:
   ```bash
   git clone https://github.com/mdabdullahfaruque/StudyBridgeBD.git
   cd StudyBridgeBD/StudyBridge
   cp StudyBridge.Api/appsettings.Example.json StudyBridge.Api/appsettings.Development.json
   # Edit appsettings.Development.json with your credentials
   dotnet run --project StudyBridge.Api
   ```

2. **Setup frontend**:
   ```bash
   cd Client/Web/studybridge-web
   npm install
   ng serve
   ```

3. **Access the application**:
   - Frontend: http://localhost:4200
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger

## 📚 Documentation

For comprehensive documentation, start with [`/docs/INDEX.md`](./docs/INDEX.md) which provides:
- Complete project overview and architecture
- API reference with examples
- Development guidelines and standards
- Implementation status and progress tracking

## 🤝 Contributing

1. Follow Clean Architecture principles
2. Use the established CQRS pattern  
3. Add comprehensive tests for new features
4. Update documentation
5. Follow secure configuration practices

## 📄 License

This project is licensed under the MIT License.

---

*Last updated: September 8, 2025*
