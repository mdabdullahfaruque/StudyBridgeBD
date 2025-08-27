# StudyBridge

A comprehensive IELTS vocabulary learning platform for Bangladeshi students.

## Architecture

- **Backend**: .NET 8 Web API with modular monolithic architecture
- **Frontend**: Angular (latest) web application
- **Database**: PostgreSQL with EF Core
- **Authentication**: Google OAuth with JWT tokens

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

## Features Implemented

### Backend (API)
- ✅ Modular monolithic architecture
- ✅ Clean Architecture with CQRS
- ✅ Custom MediaR replacement (no paid dependencies)
- ✅ User Management module
- ✅ Google OAuth authentication
- ✅ JWT token generation and validation
- ✅ PostgreSQL with EF Core
- ✅ Generic API response wrapper
- ✅ CORS configuration for Angular

### Frontend (Angular)
- ✅ Latest Angular with zoneless change detection
- ✅ Routing setup
- ✅ Authentication service
- ✅ HTTP interceptor for JWT tokens
- ✅ Login component with Google OAuth
- ✅ Dashboard component
- ✅ Shared models and services
- ✅ Responsive design

## API Endpoints

### Authentication
- `POST /api/v1/auth/google` - Google OAuth login

## Next Steps

1. **Vocabulary Module**: Add vocabulary entities, services, and endpoints
2. **Database Seeding**: Import 2,100 vocabulary words with categories
3. **SRS Algorithm**: Implement spaced repetition scheduling
4. **Angular Components**: Add vocabulary learning interface
5. **Progress Tracking**: User learning analytics and streaks
6. **Testing**: Unit and integration tests

## Development Guidelines

- Keep modules loosely coupled
- Use CQRS pattern for business operations
- Maintain clean architecture boundaries
- Follow Angular best practices
- Write comprehensive tests
- Document API changes

## Contributing

1. Follow clean architecture principles
2. Use the established CQRS pattern
3. Add tests for new features
4. Update documentation
