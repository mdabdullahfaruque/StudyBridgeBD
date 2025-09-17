# StudyBridge Client - Angular Application Folder Structure

## Overview
This document outlines the folder structure for the StudyBridge Angular client application, following modular architecture principles with feature modules and shared components.

## Root Structure
```
studybridge-web/
├── src/
│   ├── app/
│   │   ├── core/                    # Core singleton services and guards
│   │   ├── shared/                  # Shared components, services, and utilities
│   │   ├── features/                # Feature modules (lazy loaded)
│   │   ├── app.config.ts           # Application configuration
│   │   ├── app.html                # Root component template
│   │   ├── app.scss               # Global application styles
│   │   ├── app.routes.ts          # Main routing configuration
│   │   ├── app.spec.ts            # Root component tests
│   │   └── app.ts                 # Root component
│   ├── assets/                     # Static assets
│   ├── environments/               # Environment configurations
│   └── styles/                     # Global SCSS files
├── public/                         # Public static files
└── angular.json                    # Angular CLI configuration
```

## Feature Modules Structure

### Authentication Module (`features/auth/`)
```
auth/
├── components/                     # Auth-specific components
│   ├── login/
│   │   ├── login.component.ts
│   │   ├── login.component.html
│   │   └── login.component.scss
│   └── register/
│       ├── register.component.ts
│       ├── register.component.html
│       └── register.component.scss
├── models/                         # Auth-specific interfaces and types
│   ├── auth.models.ts
│   └── login.models.ts
├── services/                       # Auth-specific services
│   ├── auth.service.ts
│   └── token.service.ts
├── guards/                         # Auth-specific guards
│   └── auth.guard.ts
├── auth.module.ts                  # Auth module definition
└── auth-routing.module.ts          # Auth routing configuration
```

### Admin Module (`features/admin/`)
```
admin/
├── components/                     # Admin-specific components
│   ├── admin-dashboard/
│   │   ├── admin-dashboard.component.ts
│   │   ├── admin-dashboard.component.html
│   │   └── admin-dashboard.component.scss
│   └── user-management/
│       ├── user-list/
│       ├── user-form/
│       └── role-management/
├── models/                         # Admin-specific interfaces and types
│   ├── admin.models.ts
│   ├── user-management.models.ts
│   └── role.models.ts
├── services/                       # Admin-specific services
│   ├── admin.service.ts
│   ├── user-management.service.ts
│   └── role.service.ts
├── guards/                         # Admin-specific guards
│   └── admin.guard.ts
├── pipes/                          # Admin-specific pipes
│   └── role-display.pipe.ts
├── admin.module.ts                 # Admin module definition
└── admin-routing.module.ts         # Admin routing configuration
```

### Public Module (`features/public/`)
```
public/
├── components/                     # Public user components
│   ├── dashboard/
│   │   ├── dashboard.component.ts
│   │   ├── dashboard.component.html
│   │   └── dashboard.component.scss
│   ├── vocabulary/
│   │   ├── vocabulary-list/
│   │   ├── vocabulary-quiz/
│   │   └── vocabulary-flashcards/
│   ├── learning/
│   │   ├── learning-session/
│   │   ├── spaced-repetition/
│   │   └── progress-tracker/
│   ├── profile/
│   │   ├── profile-view/
│   │   ├── profile-edit/
│   │   └── subscription-status/
│   └── settings/
│       ├── preferences/
│       ├── notifications/
│       └── account-settings/
├── models/                         # Public module specific types
│   ├── vocabulary.models.ts
│   ├── learning.models.ts
│   ├── progress.models.ts
│   └── user-profile.models.ts
├── services/                       # Public module specific services
│   ├── vocabulary.service.ts
│   ├── learning.service.ts
│   ├── progress.service.ts
│   └── user-profile.service.ts
├── pipes/                          # Public module specific pipes
│   ├── difficulty-level.pipe.ts
│   └── progress-percentage.pipe.ts
├── directives/                     # Public module specific directives
│   └── learning-highlight.directive.ts
├── public.module.ts                # Public module definition
└── public-routing.module.ts        # Public routing configuration
```

## Shared Module Structure (`shared/`)
```
shared/
├── components/                     # Reusable UI components
│   ├── buttons/
│   │   ├── primary-button/
│   │   ├── secondary-button/
│   │   └── icon-button/
│   ├── forms/
│   │   ├── form-field/
│   │   ├── validation-message/
│   │   └── form-wrapper/
│   ├── navigation/
│   │   ├── breadcrumb/
│   │   ├── pagination/
│   │   └── tab-menu/
│   ├── data-display/
│   │   ├── data-table/
│   │   ├── card/
│   │   └── progress-bar/
│   ├── feedback/
│   │   ├── loading-spinner/
│   │   ├── error-message/
│   │   └── success-message/
│   └── modals/
│       ├── confirmation-dialog/
│       ├── info-dialog/
│       └── form-dialog/
├── layouts/                        # Layout components
│   ├── base-layout/
│   │   ├── base-layout.component.ts
│   │   ├── base-layout.component.html
│   │   └── base-layout.component.scss
│   ├── admin-layout/
│   │   ├── admin-layout.component.ts
│   │   ├── admin-layout.component.html
│   │   └── admin-layout.component.scss
│   ├── public-layout/
│   │   ├── public-layout.component.ts
│   │   ├── public-layout.component.html
│   │   └── public-layout.component.scss
│   ├── auth-layout/
│   │   ├── auth-layout.component.ts
│   │   ├── auth-layout.component.html
│   │   └── auth-layout.component.scss
│   ├── header/
│   │   ├── header.component.ts
│   │   ├── header.component.html
│   │   └── header.component.scss
│   ├── sidebar/
│   │   ├── sidebar.component.ts
│   │   ├── sidebar.component.html
│   │   └── sidebar.component.scss
│   └── footer/
│       ├── footer.component.ts
│       ├── footer.component.html
│       └── footer.component.scss
├── models/                         # Shared interfaces and types
│   ├── api.models.ts
│   ├── common.models.ts
│   ├── navigation.models.ts
│   └── layout.models.ts
├── services/                       # Shared services
│   ├── api.service.ts
│   ├── notification.service.ts
│   ├── loading.service.ts
│   ├── theme.service.ts
│   └── storage.service.ts
├── pipes/                          # Shared pipes
│   ├── date-format.pipe.ts
│   ├── truncate.pipe.ts
│   └── safe-html.pipe.ts
├── directives/                     # Shared directives
│   ├── click-outside.directive.ts
│   ├── auto-focus.directive.ts
│   └── permission.directive.ts
├── validators/                     # Custom form validators
│   ├── custom-validators.ts
│   └── async-validators.ts
├── utils/                          # Utility functions
│   ├── date.utils.ts
│   ├── string.utils.ts
│   ├── array.utils.ts
│   └── storage.utils.ts
├── constants/                      # Application constants
│   ├── api.constants.ts
│   ├── route.constants.ts
│   └── app.constants.ts
└── shared.module.ts               # Shared module definition
```

## Core Module Structure (`core/`)
```
core/
├── guards/                         # Application guards
│   ├── auth.guard.ts
│   ├── role.guard.ts
│   └── unsaved-changes.guard.ts
├── interceptors/                   # HTTP interceptors
│   ├── auth.interceptor.ts
│   ├── error.interceptor.ts
│   ├── loading.interceptor.ts
│   └── retry.interceptor.ts
├── services/                       # Singleton core services
│   ├── error-handler.service.ts
│   ├── logger.service.ts
│   └── config.service.ts
├── models/                         # Core application models
│   ├── error.models.ts
│   └── config.models.ts
└── core.module.ts                 # Core module definition (imported only once)
```

## Component Structure Convention

Each component follows this structure:
```
component-name/
├── component-name.component.ts     # Component logic
├── component-name.component.html   # Component template
├── component-name.component.scss   # Component styles
└── component-name.component.spec.ts # Component tests (optional)
```

## Module Organization Principles

### Feature Modules
- **Self-contained**: Each feature module contains all its dependencies
- **Lazy loaded**: Loaded only when needed to improve performance
- **Domain-specific**: Organized around business features
- **Independent**: Minimal dependencies on other feature modules

### Shared Module
- **Reusable components**: Used across multiple feature modules
- **Common services**: Shared utilities and services
- **UI components**: Generic, configurable components
- **Imported by features**: Feature modules import SharedModule

### Core Module
- **Singleton services**: Services that should have only one instance
- **Application-wide guards**: Security and navigation guards
- **HTTP interceptors**: Request/response handling
- **Imported once**: Only imported in AppModule/main.ts

## File Naming Conventions

- **Components**: `component-name.component.ts`
- **Services**: `service-name.service.ts`
- **Models**: `model-name.models.ts`
- **Guards**: `guard-name.guard.ts`
- **Pipes**: `pipe-name.pipe.ts`
- **Directives**: `directive-name.directive.ts`
- **Modules**: `module-name.module.ts`
- **Routing**: `module-name-routing.module.ts`

## Best Practices

1. **Separation of Concerns**: Each folder has a specific purpose
2. **Barrel Exports**: Use index.ts files for clean imports
3. **Consistent Naming**: Follow Angular style guide conventions
4. **Feature Isolation**: Keep feature-specific code within feature modules
5. **Shared Reusability**: Common functionality in shared module
6. **Lazy Loading**: All feature modules should be lazy loaded
7. **Type Safety**: Strong typing with TypeScript interfaces
8. **Component Architecture**: Separate .ts, .html, and .scss files

## Environment Structure
```
src/
├── environments/
│   ├── environment.ts              # Development environment
│   ├── environment.prod.ts         # Production environment
│   └── environment.staging.ts      # Staging environment (optional)
```

## Assets Structure
```
src/
├── assets/
│   ├── images/                     # Application images
│   ├── icons/                      # SVG icons and favicons
│   ├── fonts/                      # Custom fonts
│   ├── i18n/                       # Internationalization files
│   └── data/                       # Static JSON data files
```

This structure ensures maintainability, scalability, and follows Angular best practices for enterprise applications.