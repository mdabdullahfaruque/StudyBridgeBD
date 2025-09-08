import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerForm: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      displayName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      agreeToTerms: [false, [Validators.requiredTrue]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.authService.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuthenticated => {
        if (isAuthenticated) {
          this.router.navigate(['/dashboard']);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
    } else if (confirmPassword?.errors?.['passwordMismatch']) {
      delete confirmPassword.errors['passwordMismatch'];
      if (Object.keys(confirmPassword.errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }
    
    return null;
  }

  async onSubmit(): Promise<void> {
    if (this.registerForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      try {
        const { firstName, lastName, email, password } = this.registerForm.value;
        const displayName = `${firstName} ${lastName}`.trim();
        const userData = { 
          email, 
          password, 
          displayName,
          firstName, 
          lastName 
        };
        
        await this.authService.register(userData);
        this.toastService.success('Registration Successful', 'Welcome to StudyBridge!');
        
        // Use Angular router for navigation with a slight delay to ensure auth state is updated
        setTimeout(() => {
          this.router.navigate(['/dashboard']).then(() => {
            console.log('Navigation to dashboard completed');
          });
        }, 100);
      } catch (error: any) {
        const message = error.error?.message || 'Registration failed. Please try again.';
        this.toastService.error('Registration Failed', message);
      } finally {
        this.isLoading = false;
      }
    }
  }

  async signUpWithGoogle(): Promise<void> {
    if (!this.isLoading) {
      this.isLoading = true;
      
      try {
        this.toastService.warning('Google Sign-up', 'Google OAuth integration coming soon!');
      } catch (error: any) {
        this.toastService.error('Sign-up Failed', 'Google sign-up failed');
      } finally {
        this.isLoading = false;
      }
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  get passwordMismatch(): boolean {
    const password = this.registerForm.get('password')?.value;
    const confirmPassword = this.registerForm.get('confirmPassword')?.value;
    return !!(password && confirmPassword && password !== confirmPassword && this.registerForm.get('confirmPassword')?.touched);
  }

  getPasswordStrength(): number {
    const password = this.registerForm.get('password')?.value || '';
    let strength = 0;
    
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z\d]/.test(password)) strength++;
    
    return Math.min(strength, 4);
  }

  getPasswordStrengthClass(index: number): string {
    const strength = this.getPasswordStrength();
    if (index < strength) {
      if (strength <= 1) return 'strength-weak';
      if (strength <= 2) return 'strength-fair';
      if (strength <= 3) return 'strength-good';
      return 'strength-strong';
    }
    return 'strength-empty';
  }

  getPasswordStrengthText(): string {
    const strength = this.getPasswordStrength();
    if (strength <= 1) return 'Weak password';
    if (strength <= 2) return 'Fair password';
    if (strength <= 3) return 'Good password';
    return 'Strong password';
  }

  getPasswordStrengthTextClass(): string {
    const strength = this.getPasswordStrength();
    if (strength <= 1) return 'text-weak';
    if (strength <= 2) return 'text-fair';
    if (strength <= 3) return 'text-good';
    return 'text-strong';
  }
}