import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { GoogleAuthService } from '../../../services/google-auth.service';
import { ToastService } from '../../../services/toast.service';
import { environment } from '../../../../environments/environment.development';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private authService: AuthService,
    private googleAuthService: GoogleAuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Initialize Google Auth
    this.initializeGoogleAuth();

    this.authService.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(isAuthenticated => {
        if (isAuthenticated) {
          const redirectUrl = this.authService.getRedirectUrlForUser();
          this.router.navigate([redirectUrl]);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      try {
        const { email, password } = this.loginForm.value;
        await this.authService.login({ email, password });
        this.toastService.success('Login Successful', 'Welcome back to StudyBridge!');
        
        // Use role-based redirection with a slight delay to ensure auth state is updated
        setTimeout(() => {
          const redirectUrl = this.authService.getRedirectUrlForUser();
          this.router.navigate([redirectUrl]).then(() => {
            console.log('Navigation completed to:', redirectUrl);
          });
        }, 100);
      } catch (error: any) {
        const message = error.error?.message || 'Login failed. Please check your credentials.';
        this.toastService.error('Login Failed', message);
      } finally {
        this.isLoading = false;
      }
    }
  }

  async signInWithGoogle(): Promise<void> {
    if (!this.isLoading) {
      this.isLoading = true;
      
      try {
        // Get Google JWT token
        const googleToken = await this.googleAuthService.signInWithPopup();
        
        // Parse the ID token to extract user information
        const tokenPayload = this.googleAuthService.parseIdToken(googleToken);
        
        // Prepare the request in the format expected by the backend
        const googleLoginRequest = {
          idToken: googleToken,
          email: tokenPayload.email || '',
          displayName: tokenPayload.name || '',
          firstName: tokenPayload.given_name || '',
          lastName: tokenPayload.family_name || '',
          googleSub: tokenPayload.sub || '',
          avatarUrl: tokenPayload.picture || null
        };
        
        // Send the complete user data to backend for authentication using AuthService
        await this.authService.googleLoginWithUserData(googleLoginRequest);
        
        this.toastService.success('Google Sign-in Successful', 'Welcome to StudyBridge!');
        
        // Navigation will be handled by the ngOnInit subscription to isAuthenticated$
        // The AuthService properly updates the currentUser state, which triggers the redirect
      } catch (error: any) {
        console.error('Google sign-in error:', error);
        const errorMessage = error?.error?.message || error?.message || 'Please try again or use email/password login.';
        this.toastService.error('Google Sign-in Failed', errorMessage);
      } finally {
        this.isLoading = false;
      }
    }
  }

  private async initializeGoogleAuth(): Promise<void> {
    try {
      // Set the client ID from environment
      this.googleAuthService.setClientId(environment.googleClientId);
      
      // Initialize Google Auth
      await this.googleAuthService.initializeGoogleAuth();
    } catch (error) {
      console.error('Failed to initialize Google Auth:', error);
      // Continue without Google Auth - user can still use email/password
    }
  }

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }
}
