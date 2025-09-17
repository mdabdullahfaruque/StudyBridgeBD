# StudyBridge Angular - Quick Reference Guide

## 🚀 Project Structure at a Glance

### 📂 Where to Add New Code

| What to Add | Where to Put It | Example |
|-------------|-----------------|---------|
| **Reusable UI Component** | `shared/components/` | `shared/components/custom-button/` |
| **Layout Component** | `shared/layouts/` | `shared/layouts/new-layout/` |
| **Feature Component** | `features/{feature}/components/` | `features/auth/components/login/` |
| **API Service** | `shared/services/` (if shared) or `features/{feature}/services/` | `shared/services/api.service.ts` |
| **Business Logic Service** | `features/{feature}/services/` | `features/auth/services/auth.service.ts` |
| **Route Guard** | `core/guards/` (if global) or `features/{feature}/guards/` | `core/guards/auth.guard.ts` |
| **HTTP Interceptor** | `core/interceptors/` | `core/interceptors/error.interceptor.ts` |
| **Shared Model** | `shared/models/` | `shared/models/api-response.model.ts` |
| **Feature Model** | `features/{feature}/models/` | `features/auth/models/auth.models.ts` |
| **Pipe** | `shared/pipes/` (if shared) or `features/{feature}/pipes/` | `shared/pipes/date-format.pipe.ts` |
| **Directive** | `shared/directives/` (if shared) or `features/{feature}/directives/` | `shared/directives/auto-focus.directive.ts` |
| **Constants** | `shared/constants/` | `shared/constants/api.constants.ts` |
| **Validators** | `shared/validators/` | `shared/validators/custom.validators.ts` |

### 🎯 Component Creation Checklist

When creating a new component, **ALWAYS**:

1. ✅ Create a folder for the component
2. ✅ Create separate `.ts`, `.html`, and `.scss` files
3. ✅ Use proper naming: `component-name.component.{ts,html,scss}`
4. ✅ Add to appropriate module imports
5. ✅ Update routing if needed

**Example Structure:**
```
new-component/
├── new-component.component.ts    # Component logic
├── new-component.component.html  # Template
├── new-component.component.scss  # Styles
└── new-component.component.spec.ts # Tests (optional)
```

### 📋 Module Rules

| Module Type | Import Rule | Purpose |
|-------------|-------------|---------|
| **Core** | Import ONCE in `main.ts` | Singleton services, guards, interceptors |
| **Shared** | Import in any feature module | Reusable components, common services |
| **Feature** | Lazy load in routing | Self-contained business logic |

### 🔄 Before Making Changes

1. **Check current structure**: Run `./validate-structure.sh`
2. **Plan the change**: Where does it belong in the structure?
3. **Update imports**: Fix any broken import paths
4. **Update docs**: Modify `PROJECT_STRUCTURE.md` if structure changes
5. **Test build**: Run `ng build` to ensure no errors

### 🎨 Import Path Examples

```typescript
// ✅ Shared service import
import { ApiService } from '../../../shared/services/api.service';

// ✅ Feature model import  
import { AuthUser } from '../models/auth.models';

// ✅ Core guard import
import { AuthGuard } from '../../../core/guards/auth.guard';

// ✅ Shared component import
import { CustomButtonComponent } from '../../../shared/components/custom-button/custom-button.component';
```

### 🚨 Common Mistakes to Avoid

❌ **DON'T:**
- Create hardcoded services without syncing with backend
- Mix business logic in shared components
- Import feature modules into each other
- Create inline templates/styles for components
- Put feature-specific code in shared module

✅ **DO:**
- Follow the established folder structure
- Create separate files for each component
- Keep features independent
- Sync services with backend API
- Use shared module for common functionality

### 🔧 Development Commands

```bash
# Validate project structure
./validate-structure.sh

# Generate new component (in correct location)
ng generate component features/auth/components/forgot-password

# Generate new service (in correct location)  
ng generate service features/auth/services/password-reset

# Build and check for errors
ng build

# Run development server
ng serve
```

### 📚 Backend Sync Reminders

- **Models**: TypeScript interfaces should match C# DTOs
- **Services**: API calls should match .NET controller endpoints  
- **Auth**: JWT handling should align with backend implementation
- **Errors**: Error handling should match backend response format
- **Validation**: Form validation should mirror backend validation rules

---

**🔄 Keep this guide updated** when adding new patterns or changing structure!