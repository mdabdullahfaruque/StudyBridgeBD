// Registration specific models and validation rules
import { FormControl } from '@angular/forms';

export interface RegisterForm {
  firstName: FormControl<string>;
  lastName: FormControl<string>;
  email: FormControl<string>;
  password: FormControl<string>;
  confirmPassword: FormControl<string>;
  acceptTerms: FormControl<boolean>;
}

export interface RegisterFormValue {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

export interface RegisterValidationErrors {
  firstName?: {
    required?: boolean;
    minlength?: { requiredLength: number; actualLength: number };
    maxlength?: { requiredLength: number; actualLength: number };
    pattern?: boolean;
  };
  lastName?: {
    required?: boolean;
    minlength?: { requiredLength: number; actualLength: number };
    maxlength?: { requiredLength: number; actualLength: number };
    pattern?: boolean;
  };
  email?: {
    required?: boolean;
    email?: boolean;
    emailExists?: boolean;
  };
  password?: {
    required?: boolean;
    minlength?: { requiredLength: number; actualLength: number };
    pattern?: boolean;
    weak?: boolean;
  };
  confirmPassword?: {
    required?: boolean;
    mismatch?: boolean;
  };
  acceptTerms?: {
    required?: boolean;
  };
}

export interface PasswordStrength {
  score: number; // 0-4
  feedback: string[];
  hasUppercase: boolean;
  hasLowercase: boolean;
  hasNumbers: boolean;
  hasSpecialChars: boolean;
  isLengthValid: boolean;
}

export interface RegisterComponentState {
  isSubmitting: boolean;
  showPassword: boolean;
  showConfirmPassword: boolean;
  passwordStrength: PasswordStrength;
  isEmailChecking: boolean;
  emailAvailable: boolean | null;
}

export interface EmailAvailabilityResponse {
  isAvailable: boolean;
  suggestions?: string[];
}