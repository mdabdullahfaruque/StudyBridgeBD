import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, timer } from 'rxjs';
import { map, catchError, tap, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  GoogleLoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  UpdateProfileRequest,
  AuthResponse,
  User,
  ApiResponse,
  RefreshTokenRequest,
  LogoutRequest
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user_data';
  
  private currentUserSubject = new BehaviorSubject<User | null>(this.getUserFromStorage());
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasValidToken());
  private tokenRefreshTimer: any;

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeTokenRefresh();
  }

  // Authentication Methods
  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, loginData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.setAuthData(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Login failed');
        }),
        catchError(this.handleError)
      );
  }

  googleLogin(idToken: string): Observable<AuthResponse> {
    const googleLoginData: GoogleLoginRequest = { idToken };
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/google-login`, googleLoginData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.setAuthData(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Google login failed');
        }),
        catchError(this.handleError)
      );
  }

  register(registerData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/register`, registerData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.setAuthData(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Registration failed');
        }),
        catchError(this.handleError)
      );
  }

  forgotPassword(forgotPasswordData: ForgotPasswordRequest): Observable<void> {
    return this.http.post<ApiResponse>(`${environment.apiUrl}/auth/forgot-password`, forgotPasswordData)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || 'Failed to send reset email');
          }
        }),
        catchError(this.handleError)
      );
  }

  resetPassword(resetPasswordData: ResetPasswordRequest): Observable<void> {
    return this.http.post<ApiResponse>(`${environment.apiUrl}/auth/reset-password`, resetPasswordData)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || 'Password reset failed');
          }
        }),
        catchError(this.handleError)
      );
  }

  changePassword(changePasswordData: ChangePasswordRequest): Observable<void> {
    return this.http.post<ApiResponse>(`${environment.apiUrl}/auth/change-password`, changePasswordData)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || 'Password change failed');
          }
        }),
        catchError(this.handleError)
      );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    const refreshData: RefreshTokenRequest = { refreshToken };
    return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/refresh-token`, refreshData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.setAuthData(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Token refresh failed');
        }),
        catchError((error) => {
          this.logout();
          return this.handleError(error);
        })
      );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    if (refreshToken) {
      const logoutData: LogoutRequest = { refreshToken };
      this.http.post<ApiResponse>(`${environment.apiUrl}/auth/logout`, logoutData)
        .subscribe(); // Don't wait for response
    }

    this.clearAuthData();
    this.router.navigate(['/auth/login']);
  }

  // Profile Methods
  getCurrentUser(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${environment.apiUrl}/auth/profile`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.currentUserSubject.next(response.data);
            this.updateUserInStorage(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Failed to get user profile');
        }),
        catchError(this.handleError)
      );
  }

  updateProfile(updateData: UpdateProfileRequest): Observable<User> {
    return this.http.put<ApiResponse<User>>(`${environment.apiUrl}/auth/profile`, updateData)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            this.currentUserSubject.next(response.data);
            this.updateUserInStorage(response.data);
            return response.data;
          }
          throw new Error(response.message || 'Profile update failed');
        }),
        catchError(this.handleError)
      );
  }

  // Utility Methods
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getCurrentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  hasValidToken(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Math.floor(Date.now() / 1000);
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  private setAuthData(authResponse: AuthResponse): void {
    localStorage.setItem(this.TOKEN_KEY, authResponse.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
    
    this.currentUserSubject.next(authResponse.user);
    this.isAuthenticatedSubject.next(true);
    
    this.scheduleTokenRefresh(authResponse.expiresAt);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
    
    if (this.tokenRefreshTimer) {
      clearTimeout(this.tokenRefreshTimer);
    }
  }

  private getUserFromStorage(): User | null {
    const userData = localStorage.getItem(this.USER_KEY);
    return userData ? JSON.parse(userData) : null;
  }

  private updateUserInStorage(user: User): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  private initializeTokenRefresh(): void {
    if (this.hasValidToken()) {
      const token = this.getToken();
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          const expiresAt = new Date(payload.exp * 1000).toISOString();
          this.scheduleTokenRefresh(expiresAt);
        } catch (error) {
          console.error('Error parsing token:', error);
        }
      }
    }
  }

  private scheduleTokenRefresh(expiresAt: string): void {
    const expirationTime = new Date(expiresAt).getTime();
    const currentTime = Date.now();
    const refreshTime = expirationTime - currentTime - (5 * 60 * 1000); // Refresh 5 minutes before expiry

    if (refreshTime > 0) {
      this.tokenRefreshTimer = setTimeout(() => {
        this.refreshToken().subscribe({
          error: (error) => {
            console.error('Auto token refresh failed:', error);
            this.logout();
          }
        });
      }, refreshTime);
    }
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.error?.errors && Array.isArray(error.error.errors)) {
        errorMessage = error.error.errors.join(', ');
      } else {
        switch (error.status) {
          case 400:
            errorMessage = 'Invalid request. Please check your input.';
            break;
          case 401:
            errorMessage = 'Invalid credentials or session expired.';
            break;
          case 403:
            errorMessage = 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = 'The requested resource was not found.';
            break;
          case 500:
            errorMessage = 'Internal server error. Please try again later.';
            break;
          default:
            errorMessage = `Error: ${error.status} - ${error.statusText}`;
        }
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}