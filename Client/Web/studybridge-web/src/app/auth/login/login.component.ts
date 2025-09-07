import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

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
    private authService: AuthService,
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

  async onSubmit(): Promise<void> {
    if (this.loginForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      try {
        const { email, password } = this.loginForm.value;
        await this.authService.login({ email, password });
        this.toastService.success('Login Successful', 'Welcome back to StudyBridge!');
        this.router.navigate(['/dashboard']);
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
        this.toastService.warning('Google Sign-in', 'Google OAuth integration coming soon!');
      } catch (error: any) {
        this.toastService.error('Sign-in Failed', 'Google sign-in failed');
      } finally {
        this.isLoading = false;
      }
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
