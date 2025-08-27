import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../../../core/services/auth';
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
  imports: [CommonModule, LoadingComponent, ButtonComponent],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  isLoading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check if already authenticated
    this.authService.isAuthenticated$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isAuth => {
      if (isAuth) {
        this.router.navigate(['/dashboard']);
      }
    });

    // Load Google Sign-In script
    this.loadGoogleSignIn();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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
        width: 300
      }
    );
  }

  private handleGoogleCallback(response: any): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.googleLogin(response.credential).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (loginResponse) => {
        this.isLoading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Login failed. Please try again.';
      }
    });
  }
}
