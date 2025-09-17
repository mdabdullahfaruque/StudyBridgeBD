// Login specific models and validation rules
import { FormControl } from '@angular/forms';

export interface LoginForm {
  email: FormControl<string>;
  password: FormControl<string>;
  rememberMe: FormControl<boolean>;
}

export interface LoginFormValue {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface LoginValidationErrors {
  email?: {
    required?: boolean;
    email?: boolean;
  };
  password?: {
    required?: boolean;
    minlength?: { requiredLength: number; actualLength: number };
  };
}

export interface LoginComponentState {
  isSubmitting: boolean;
  showPassword: boolean;
  loginAttempts: number;
  lastAttemptTime: Date | null;
  isBlocked: boolean;
}

// Google OAuth specific
export interface GoogleLoginConfig {
  clientId: string;
  scopes: string[];
  redirectUri?: string;
}

// Social login providers
export type SocialProvider = 'google' | 'facebook' | 'microsoft';

export interface SocialLoginResult {
  provider: SocialProvider;
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  profilePicture?: string;
}