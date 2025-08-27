import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../../shared/services/auth.service';
import { NotificationService } from '../../../../shared/services/notification.service';
import { LoadingComponent } from '../../../../shared/components/loading/loading';
import { ButtonComponent } from '../../../../shared/components/button/button';
import { CommonModule } from '@angular/common';

declare global {
  interface Window {
    google: any;
  }
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LoadingComponent, ButtonComponent],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  loginForm: FormGroup;
  isLoading = false;
  isGoogleLoading = false;
  showPassword = false;
  returnUrl = '/dashboard';

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute,
    private formBuilder: FormBuilder
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Get return URL from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    // Check if already authenticated
    this.authService.isAuthenticated$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isAuth => {
      if (isAuth) {
        this.router.navigate([this.returnUrl]);
      }
    });

    // Load Google Sign-In script
    this.loadGoogleSignIn();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.loginForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const { email, password } = this.loginForm.value;
      
      this.authService.login({ email, password }).pipe(
        takeUntil(this.destroy$)
      ).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.notificationService.success(
            'Welcome back!', 
            `Hello ${response.user.firstName}, you have successfully logged in.`
          );
          this.router.navigate([this.returnUrl]);
        },
        error: (error) => {
          this.isLoading = false;
          this.notificationService.error(
            'Login Failed', 
            error.message || 'Please check your credentials and try again.'
          );
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  onGoogleLogin(): void {
    if (window.google && window.google.accounts) {
      window.google.accounts.id.prompt();
    } else {
      this.notificationService.error(
        'Google Sign-In Error',
        'Google Sign-In is not available. Please try again later.'
      );
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  navigateToRegister(): void {
    this.router.navigate(['/auth/register'], {
      queryParams: { returnUrl: this.returnUrl }
    });
  }

  navigateToForgotPassword(): void {
    this.router.navigate(['/auth/forgot-password']);
  }

  // Form helper methods
  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (field && field.errors && (field.dirty || field.touched)) {
      if (field.errors['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      if (field.errors['email']) {
        return 'Please enter a valid email address';
      }
      if (field.errors['minlength']) {
        return `Password must be at least ${field.errors['minlength'].requiredLength} characters`;
      }
    }
    return '';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      email: 'Email',
      password: 'Password'
    };
    return displayNames[fieldName] || fieldName;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  private loadGoogleSignIn(): void {
    if (typeof window.google !== 'undefined') {
      this.initializeGoogleSignIn();
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => this.initializeGoogleSignIn();
    script.onerror = () => {
      console.error('Failed to load Google Sign-In script');
    };
    document.head.appendChild(script);
  }

  private initializeGoogleSignIn(): void {
    window.google.accounts.id.initialize({
      client_id: 'your-google-client-id.apps.googleusercontent.com', // Replace with your actual client ID
      callback: (response: any) => this.handleGoogleCallback(response)
    });

    window.google.accounts.id.renderButton(
      document.getElementById('google-signin-button'),
      {
        theme: 'outline',
        size: 'large',
        width: 300,
        text: 'signin_with'
      }
    );
  }

  private handleGoogleCallback(response: any): void {
    this.isGoogleLoading = true;

    this.authService.googleLogin(response.credential).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (authResponse) => {
        this.isGoogleLoading = false;
        this.notificationService.success(
          'Welcome!', 
          `Hello ${authResponse.user.firstName}, you have successfully logged in with Google.`
        );
        this.router.navigate([this.returnUrl]);
      },
      error: (error) => {
        this.isGoogleLoading = false;
        this.notificationService.error(
          'Google Login Failed', 
          error.message || 'Please try again.'
        );
      }
    });
  }
}
