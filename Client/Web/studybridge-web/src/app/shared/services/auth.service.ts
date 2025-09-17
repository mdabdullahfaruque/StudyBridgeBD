import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment.development';
import { 
  UserDto, 
  LoginRequest, 
  RegisterRequest, 
  GoogleLoginRequest, 
  ChangePasswordRequest, 
  LoginResponse,
  ApiResponse 
} from '../models/api.models';
import { AuthApiService } from './auth-api.service';
import { NotificationService } from './notification.service';
import { API_CONFIG } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  private isLoadingSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();
  public isAuthenticated$ = this.currentUser$.pipe(map(user => !!user));

  constructor(
    private http: HttpClient,
    private router: Router,
    private authApiService: AuthApiService,
    private notificationService: NotificationService
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
      // FOR DEMO PURPOSES: Create a mock user to test PrimeNG styling
      this.createMockUser();
    }
  }

  private createMockUser(): void {
    const mockUser: UserDto = {
      id: '1',
      email: 'demo@studybridge.com',
      firstName: 'Demo',
      lastName: 'User',
      displayName: 'Demo User',
      isActive: true,
      emailConfirmed: true,
      roles: [{ 
        id: '1', 
        name: 'User', 
        description: 'Standard user role', 
        isActive: true,
        systemRole: 1,
        permissions: [{ 
          id: '1', 
          permissionKey: 'dashboard.view', 
          displayName: 'View Dashboard',
          description: 'View dashboard', 
          permissionType: 1, 
          isSystemPermission: true 
        }] 
      }],
      createdAt: new Date().toISOString()
    };
    
    // Store mock data for demo
    localStorage.setItem(API_CONFIG.AUTH.TOKEN_KEY, 'mock-jwt-token-for-demo');
    localStorage.setItem(API_CONFIG.AUTH.USER_KEY, JSON.stringify(mockUser));
    this.currentUserSubject.next(mockUser);
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
      const request: GoogleLoginRequest = { idToken: googleToken };
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

  async googleLoginWithUserData(googleLoginData: any): Promise<LoginResponse> {
    this.isLoadingSubject.next(true);
    
    try {
      const response = await this.http.post<ApiResponse<LoginResponse>>(
        `${this.apiUrl}/auth/google`,
        googleLoginData
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

  getCurrentUser(): UserDto | null {
    return this.currentUserSubject.value;
  }

  hasRole(roleName: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.some(role => role.name === roleName) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(roleName => this.hasRole(roleName));
  }

  hasPermission(permissionKey: string): boolean {
    const user = this.getCurrentUser();
    if (!user?.roles) return false;
    
    return user.roles.some(role => 
      role.permissions?.some(permission => permission.permissionKey === permissionKey)
    );
  }

  // Role-based navigation helpers
  getRedirectUrlForUser(): string {
    const user = this.getCurrentUser();
    if (!user) return '/auth/login';
    
    // If user has only 'User' role, redirect to public area
    if (user.roles.length === 1 && user.roles[0].name === 'User') {
      return '/public/dashboard';
    }
    
    // If user has admin roles (Admin, SuperAdmin, etc.), redirect to admin area
    const adminRoles = ['Admin', 'SuperAdmin', 'Administrator'];
    if (user.roles.some(role => adminRoles.includes(role.name))) {
      return '/admin/dashboard';
    }
    
    // Default fallback
    return '/public/dashboard';
  }

  isAdminUser(): boolean {
    const adminRoles = ['Admin', 'SuperAdmin', 'Administrator'];
    return this.hasAnyRole(adminRoles);
  }

  // Private Helper Methods
  private handleAuthSuccess(loginResponse: LoginResponse): void {
    const user = loginResponse.user;

    // Store auth data
    localStorage.setItem(API_CONFIG.AUTH.TOKEN_KEY, loginResponse.token);
    localStorage.setItem(API_CONFIG.AUTH.REFRESH_TOKEN_KEY, loginResponse.refreshToken);
    localStorage.setItem(API_CONFIG.AUTH.USER_KEY, JSON.stringify(user));
    
    // Update current user - use next() to immediately update the observable
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem(API_CONFIG.AUTH.TOKEN_KEY);
    localStorage.removeItem(API_CONFIG.AUTH.REFRESH_TOKEN_KEY);
    localStorage.removeItem(API_CONFIG.AUTH.USER_KEY);
  }

  private getStoredToken(): string | null {
    return localStorage.getItem(API_CONFIG.AUTH.TOKEN_KEY);
  }

  private getStoredUser(): UserDto | null {
    const userJson = localStorage.getItem(API_CONFIG.AUTH.USER_KEY);
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
