# StudyBridge - IELTS Vocabulary Learning Platform

A comprehensive vocabulary learning platform for Bangladeshi students preparing for IELTS, built with .NET 8 Web API and Angular.

## Project Structure

```
studybridgebd/
├── StudyBridge/                    # .NET 8 Web API (Backend)
│   ├── src/
│   │   ├── StudyBridge.Api/        # Main API project
│   │   ├── Shared/
│   │   │   └── StudyBridge.Shared/ # Common utilities, CQRS
│   │   └── Modules/
│   │       └── StudyBridge.UserManagement/  # User auth module
│   └── StudyBridge.sln
└── Client/
    └── Web/
        └── studybridge-web/        # Angular Client
```

## Tech Stack

### Backend (.NET 8)
- **Architecture**: Modular Monolithic with Clean Architecture
- **CQRS**: Custom implementation (no MediatR dependency)
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT + Google OAuth
- **Logging**: Serilog

### Frontend (Angular 18+)
- **Architecture**: Feature-based modules
- **Styling**: SCSS with custom component library
- **State Management**: RxJS Observables
- **UI Components**: Standalone reusable components

## Features Implemented

### ✅ User Management
- Google OAuth authentication
- JWT token management
- User profile management
- Session handling

### ✅ Shared Components
- Reusable button component with variants
- Loading spinner component
- Header with navigation
- API service with error handling

### ✅ Architecture
- Clean separation of concerns
- Modular structure for easy feature addition
- Custom CQRS implementation
- Proper routing and lazy loading

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- PostgreSQL
- Google OAuth credentials

### Backend Setup

1. **Navigate to backend directory**
   ```bash
   cd StudyBridge
   ```

2. **Update connection string in appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=studybridge_dev;Username=your_user;Password=your_password"
     },
     "GoogleAuth": {
       "ClientId": "your-google-client-id.apps.googleusercontent.com"
     }
   }
   ```

3. **Run the API**
   ```bash
   dotnet run --project src/StudyBridge.Api
   ```

   API will be available at: `https://localhost:7001`

### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd Client/Web/studybridge-web
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Update environment configuration**
   Edit `src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7001/api/v1'
   };
   ```

4. **Run the development server**
   ```bash
   npm start
   ```

   App will be available at: `http://localhost:4200`

## Development Workflow

### Adding New Features

1. **Backend**: Create new module in `src/Modules/`
2. **Frontend**: Create new feature module in `src/app/features/`
3. **Shared**: Add reusable components to `src/app/shared/`

## Upcoming Features

- [ ] Vocabulary learning module
- [ ] Spaced repetition system
- [ ] Progress tracking and analytics
- [ ] IELTS-specific word packs
- [ ] Speaking practice integration
- [ ] Admin panel for content management

## License

This project is licensed under the MIT License.
