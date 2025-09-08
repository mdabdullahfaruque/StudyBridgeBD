import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { 
  User, 
  LoginRequest, 
  RegisterRequest, 
  GoogleLoginRequest, 
  ChangePasswordRequest, 
  LoginResponse,
  ApiResponse 
} from '../models/user.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isLoadingSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();
  public isAuthenticated$ = this.currentUser$.pipe(map(user => !!user));

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = this.getStoredToken();
    const user = this.getStoredUser();
    
    if (token && user && !this.isTokenExpired(token)) {
      this.currentUserSubject.next(user);
    } else {
      this.clearAuthData();
    }
  }

  // Authentication Methods
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    this.isLoadingSubject.next(true);
    
    try {
      const response = await this.http.post<ApiResponse<LoginResponse>>(
        `${this.apiUrl}/auth/login`,
        credentials
      ).toPromise();

      if (response?.data) {
        this.handleAuthSuccess(response.data);
        return response.data;
      }
      
      throw new Error('Invalid response format');
    } catch (error) {
      this.handleError('Login failed', error);
      throw error;
    } finally {
      this.isLoadingSubject.next(false);
    }
  }

  async register(userData: RegisterRequest): Promise<LoginResponse> {
    this.isLoadingSubject.next(true);
    
    try {
      const response = await this.http.post<ApiResponse<LoginResponse>>(
        `${this.apiUrl}/auth/register`,
        userData
      ).toPromise();

      if (response?.data) {
        this.handleAuthSuccess(response.data);
        return response.data;
      }
      
      throw new Error('Invalid response format');
    } catch (error) {
      this.handleError('Registration failed', error);
      throw error;
    } finally {
      this.isLoadingSubject.next(false);
    }
  }

  async googleLogin(googleToken: string): Promise<LoginResponse> {
    this.isLoadingSubject.next(true);
    
    try {
      const request: GoogleLoginRequest = { googleToken };
      const response = await this.http.post<ApiResponse<LoginResponse>>(
        `${this.apiUrl}/auth/google`,
        request
      ).toPromise();

      if (response?.data) {
        this.handleAuthSuccess(response.data);
        return response.data;
      }
      
      throw new Error('Invalid response format');
    } catch (error) {
      this.handleError('Google login failed', error);
      throw error;
    } finally {
      this.isLoadingSubject.next(false);
    }
  }

  async changePassword(passwordData: ChangePasswordRequest): Promise<void> {
    this.isLoadingSubject.next(true);
    
    try {
      await this.http.post<ApiResponse<void>>(
        `${this.apiUrl}/auth/change-password`,
        passwordData
      ).toPromise();
    } catch (error) {
      this.handleError('Password change failed', error);
      throw error;
    } finally {
      this.isLoadingSubject.next(false);
    }
  }

  logout(): void {
    this.clearAuthData();
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  // Token Management
  getToken(): string | null {
    return this.getStoredToken();
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  // Private Helper Methods
  private handleAuthSuccess(loginResponse: LoginResponse): void {
    const user: User = {
      id: loginResponse.userId,
      email: loginResponse.email,
      displayName: loginResponse.displayName,
      roles: loginResponse.roles,
      isActive: true,
      lastLoginAt: new Date()
    };

    // Store auth data
    localStorage.setItem(environment.tokenKey, loginResponse.token);
    localStorage.setItem(environment.userKey, JSON.stringify(user));
    
    // Update current user - use next() to immediately update the observable
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem(environment.tokenKey);
    localStorage.removeItem(environment.userKey);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(environment.tokenKey);
  }

  private getStoredUser(): User | null {
    const userJson = localStorage.getItem(environment.userKey);
    return userJson ? JSON.parse(userJson) : null;
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expirationTime = payload.exp * 1000; // Convert to milliseconds
      return Date.now() >= expirationTime;
    } catch {
      return true; // If we can't parse the token, consider it expired
    }
  }

  private handleError(message: string, error: any): void {
    if (environment.enableLogging) {
      console.error(message, error);
    }

    if (error instanceof HttpErrorResponse) {
      if (error.status === 401) {
        this.logout();
      }
    }
  }
}
