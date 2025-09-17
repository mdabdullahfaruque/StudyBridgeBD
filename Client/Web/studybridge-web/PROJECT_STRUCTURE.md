# StudyBridge Angular Project Structure

**Version:** 1.0  
**Last Updated:** September 17, 2025  
**Branch:** feature/rbac-dynamic-permissions  

## 🏗️ Current Project Structure

```
studybridge-web/
├── src/
│   ├── app/
│   │   ├── core/                           # Singleton services, guards, interceptors
│   │   │   ├── guards/                     # Application-wide guards
│   │   │   │   ├── auth.guard.ts          # [MOVED HERE]
│   │   │   │   └── role.guard.ts          # [MOVED HERE]
│   │   │   ├── interceptors/              # HTTP interceptors
│   │   │   │   ├── auth.interceptor.ts    # [MOVED HERE]
│   │   │   │   ├── error.interceptor.ts   # [MOVED HERE]
│   │   │   │   └── loading.interceptor.ts # [MOVED HERE]
│   │   │   ├── services/                  # Core singleton services
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   ├── models/                    # Core application models
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   └── core.module.ts             # [TO BE CREATED]
│   │   │
│   │   ├── shared/                        # Shared components & utilities
│   │   │   ├── components/                # Reusable UI components
│   │   │   │   ├── dynamic-form/          # [EXISTING]
│   │   │   │   ├── table-wrapper/         # [EXISTING]
│   │   │   │   ├── toast-container/       # [EXISTING]
│   │   │   │   └── tree-wrapper/          # [EXISTING]
│   │   │   ├── layouts/                   # Layout components
│   │   │   │   ├── admin-layout/          # [MOVED HERE]
│   │   │   │   ├── auth-layout/           # [MOVED HERE]
│   │   │   │   ├── base-layout/           # [MOVED HERE]
│   │   │   │   ├── public-layout/         # [MOVED HERE]
│   │   │   │   ├── footer/               # [MOVED HERE]
│   │   │   │   ├── header/               # [MOVED HERE]
│   │   │   │   └── sidebar/              # [MOVED HERE]
│   │   │   ├── models/                    # Shared interfaces & types
│   │   │   │   ├── api-response.model.ts  # [MOVED HERE]
│   │   │   │   ├── common.model.ts        # [MOVED HERE]
│   │   │   │   └── navigation.model.ts    # [MOVED HERE]
│   │   │   ├── services/                  # Shared services
│   │   │   │   ├── api.service.ts         # [MOVED HERE]
│   │   │   │   ├── loading.service.ts     # [MOVED HERE]
│   │   │   │   ├── notification.service.ts # [MOVED HERE]
│   │   │   │   └── storage.service.ts     # [MOVED HERE]
│   │   │   ├── pipes/                     # Shared pipes
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   ├── directives/                # Shared directives
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   ├── validators/                # Custom form validators
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   ├── constants/                 # Application constants
│   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   ├── interfaces/                # Legacy interfaces
│   │   │   │   └── [EXISTING - TO BE ORGANIZED]
│   │   │   └── shared.module.ts           # [EXISTING]
│   │   │
│   │   ├── features/                      # Feature modules (lazy loaded)
│   │   │   │
│   │   │   ├── auth/                      # Authentication module
│   │   │   │   ├── components/            # Auth components
│   │   │   │   │   ├── login/             # [MOVED HERE]
│   │   │   │   │   │   ├── login.component.ts
│   │   │   │   │   │   ├── login.component.html
│   │   │   │   │   │   └── login.component.scss
│   │   │   │   │   └── register/          # [MOVED HERE]
│   │   │   │   │       ├── register.component.ts
│   │   │   │   │       ├── register.component.html
│   │   │   │   │       └── register.component.scss
│   │   │   │   ├── models/                # Auth-specific models
│   │   │   │   │   ├── auth.models.ts     # [CREATED]
│   │   │   │   │   ├── login.models.ts    # [CREATED]
│   │   │   │   │   ├── register.models.ts # [CREATED]
│   │   │   │   │   └── index.ts           # [CREATED]
│   │   │   │   ├── services/              # Auth services
│   │   │   │   │   └── [READY FOR BACKEND SYNC]
│   │   │   │   ├── guards/                # Auth guards
│   │   │   │   │   └── [READY FOR BACKEND SYNC]
│   │   │   │   ├── auth.module.ts         # [EXISTING]
│   │   │   │   └── auth-routing.module.ts # [EXISTING]
│   │   │   │
│   │   │   ├── admin/                     # Admin module
│   │   │   │   ├── components/            # Admin components
│   │   │   │   │   ├── admin-dashboard/   # [MOVED HERE]
│   │   │   │   │   │   ├── admin-dashboard.component.ts
│   │   │   │   │   │   ├── admin-dashboard.component.html
│   │   │   │   │   │   └── admin-dashboard.component.scss
│   │   │   │   │   ├── user-management/   # [MOVED HERE]
│   │   │   │   │   │   └── [Multiple components]
│   │   │   │   │   └── components/        # [LEGACY - TO BE ORGANIZED]
│   │   │   │   ├── models/                # Admin-specific models
│   │   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   │   ├── services/              # Admin services
│   │   │   │   │   └── services/          # [LEGACY - TO BE ORGANIZED]
│   │   │   │   ├── guards/                # Admin guards
│   │   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   │   ├── pipes/                 # Admin pipes
│   │   │   │   │   └── [READY FOR IMPLEMENTATION]
│   │   │   │   ├── admin.module.ts        # [EXISTING]
│   │   │   │   ├── admin-routing.module.ts # [EXISTING]
│   │   │   │   └── admin.routes.ts        # [LEGACY]
│   │   │   │
│   │   │   └── public/                    # Public user module
│   │   │       ├── components/            # Public components
│   │   │       │   ├── dashboard/         # [MOVED HERE]
│   │   │       │   │   ├── dashboard.component.ts
│   │   │       │   │   ├── dashboard.component.html
│   │   │       │   │   └── dashboard.component.scss
│   │   │       │   ├── learning/          # [MOVED HERE]
│   │   │       │   ├── profile/           # [MOVED HERE]
│   │   │       │   ├── settings/          # [MOVED HERE]
│   │   │       │   └── vocabulary/        # [MOVED HERE]
│   │   │       ├── models/                # Public module models
│   │   │       │   └── [READY FOR IMPLEMENTATION]
│   │   │       ├── services/              # Public services
│   │   │       │   └── [READY FOR IMPLEMENTATION]
│   │   │       ├── pipes/                 # Public pipes
│   │   │       │   └── [READY FOR IMPLEMENTATION]
│   │   │       ├── directives/            # Public directives
│   │   │       │   └── [READY FOR IMPLEMENTATION]
│   │   │       ├── public.module.ts       # [EXISTING]
│   │   │       └── public-routing.module.ts # [EXISTING]
│   │   │
│   │   ├── app.config.ts                  # App configuration
│   │   ├── app.html                       # Root template
│   │   ├── app.scss                       # Global styles
│   │   ├── app.routes.ts                  # Main routing
│   │   ├── app.spec.ts                    # App tests
│   │   └── app.ts                         # Root component
│   │
│   ├── assets/                            # Static assets
│   ├── environments/                      # Environment configs
│   └── styles/                            # Global SCSS
├── public/                                # Public files
├── angular.json                           # Angular CLI config
├── package.json                           # Dependencies
├── tailwind.config.js                     # Tailwind config
├── tsconfig.json                          # TypeScript config
└── CLIENT_FOLDER_STRUCTURE.md             # This document
```

## 📋 Module Organization Rules

### 🔵 Core Module (Singleton Pattern)
- **Purpose:** Application-wide singleton services, guards, and interceptors
- **Import:** Only once in `main.ts` or `app.config.ts`
- **Contains:** 
  - Guards (auth.guard.ts, role.guard.ts)
  - Interceptors (auth, error, loading)
  - Core services (error handling, logging, config)
  - Core models (error types, config interfaces)

### 🟢 Shared Module (Reusable Components)
- **Purpose:** Components, services, and utilities used across multiple features
- **Import:** In any feature module that needs shared functionality
- **Contains:**
  - UI Components (buttons, forms, modals, tables)
  - Layout Components (headers, footers, sidebars, layouts)
  - Shared Services (API, notification, storage)
  - Common Models (API response types, navigation)
  - Pipes, Directives, Validators, Constants

### 🟠 Feature Modules (Business Logic)
- **Purpose:** Self-contained feature implementations
- **Load:** Lazy loaded for performance
- **Structure:** Each feature has its own:
  - `components/` - Feature-specific UI components
  - `models/` - Feature-specific TypeScript interfaces
  - `services/` - Feature business logic services
  - `guards/` - Feature-specific route guards (if needed)
  - `pipes/` - Feature-specific display pipes (if needed)
  - `directives/` - Feature-specific DOM manipulation (if needed)

## 🎯 Component Structure Convention

**MANDATORY:** Each component must have its own folder with separate files:

```
component-name/
├── component-name.component.ts     # Component logic
├── component-name.component.html   # Component template  
├── component-name.component.scss   # Component styles
└── component-name.component.spec.ts # Component tests (optional)
```

## 🔄 Update Protocol

### When Adding New Features:
1. **Create feature module** under `features/`
2. **Add folder structure** following the established pattern
3. **Update this document** with new structure
4. **Update version number** and last modified date

### When Adding Components:
1. **Create component folder** with separate .ts, .html, .scss files
2. **Place in appropriate module** (core/shared/feature)
3. **Update module imports** if needed
4. **Document in appropriate section**

### When Moving Files:
1. **Update all import paths** in affected files
2. **Update routing configurations** if applicable
3. **Update this document** to reflect new locations
4. **Test build** to ensure no broken imports

## 📝 Status Legend

- `[EXISTING]` - Already implemented and working
- `[MOVED HERE]` - Relocated from another location
- `[CREATED]` - Newly created in this structure
- `[READY FOR IMPLEMENTATION]` - Folder exists, awaiting implementation
- `[LEGACY]` - Old structure, needs refactoring
- `[TO BE ORGANIZED]` - Exists but needs proper organization

## ⚠️ Backend Synchronization Notes

- **No hardcoded services:** All service implementations must align with .NET backend API
- **Model alignment:** TypeScript interfaces should match C# DTOs from backend
- **API endpoints:** Use backend's actual endpoint structure and naming
- **Authentication:** Sync with backend JWT and OAuth implementation
- **Error handling:** Match backend error response format

## 🔮 Future Structure Additions

When implementing new features, consider these potential additions:

### Vocabulary Management Module (Planned)
```
features/vocabulary-management/
├── components/          # Word lists, quizzes, flashcards
├── models/             # Vocabulary models synced with backend
├── services/           # Vocabulary CRUD operations
└── pipes/              # Word difficulty, progress pipes
```

### Learning Engine Module (Planned)
```
features/learning-engine/
├── components/          # SRS sessions, progress tracking
├── models/             # Learning algorithm models
├── services/           # Spaced repetition service
└── directives/         # Learning interaction directives
```

### Progress Analytics Module (Planned)
```
features/progress-analytics/
├── components/          # Charts, statistics, streaks
├── models/             # Analytics data models
├── services/           # Progress calculation service
└── pipes/              # Statistics formatting pipes
```

---

**🔄 IMPORTANT:** This document MUST be updated every time we modify the project structure. Always increment the version number and update the "Last Updated" date when making structural changes.