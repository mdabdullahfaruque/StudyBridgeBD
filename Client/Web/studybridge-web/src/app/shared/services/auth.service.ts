import { Injectable, signal, computed, Injector } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment.development';
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
import { TokenService } from './token.service';
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

  private menuService: any; // Lazy-loaded to avoid circular dependency

  constructor(
    private http: HttpClient,
    private router: Router,
    private authApiService: AuthApiService,
    private notificationService: NotificationService,
    private tokenService: TokenService,
    private injector: Injector
  ) {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = this.tokenService.getToken();
    const user = this.getStoredUser();
    
    if (token && user && !this.tokenService.isTokenExpired()) {
      this.currentUserSubject.next(user);
    } else {
      this.clearAuthData();
      // No mock user - user needs to login properly
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
    return this.tokenService.getToken();
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
    
    // Check for admin roles first (higher priority)
    const adminRoles = ['Admin', 'SuperAdmin', 'Administrator', 'SuperAdmin', 'Finance', 'Accounts', 'ContentManager'];
    const hasAdminRole = user.roles.some(role => adminRoles.includes(role.name));
    
    if (hasAdminRole) {
      return '/admin/dashboard';
    }
    
    // Default to public area for regular users
    return '/public/dashboard';
  }

  isAdminUser(): boolean {
    const adminRoles = ['Admin', 'SuperAdmin', 'Administrator', 'SuperAdmin', 'Finance', 'Accounts', 'ContentManager'];
    return this.hasAnyRole(adminRoles);
  }

  /**
   * Navigate user to appropriate dashboard after login
   */
  async navigateToUserDashboard(): Promise<void> {
    // Get redirect URL first
    const redirectUrl = this.getRedirectUrlForUser();
    
    // Navigate immediately - don't wait for menu loading
    this.router.navigate([redirectUrl]);

    // Load menus for the current user (async, don't block navigation)
    try {
      // Lazy-load MenuService to avoid circular dependency
      if (!this.menuService) {
        const { MenuService } = await import('./menu.service');
        this.menuService = this.injector.get(MenuService);
      }
      await this.menuService.loadMenusForCurrentUser();
    } catch (error) {
      console.error('Error loading menus:', error);
      // Menu loading failure should not prevent navigation
    }
  }

  // Private Helper Methods
  private handleAuthSuccess(loginResponse: LoginResponse): void {
    const user = loginResponse.user;

    // Store auth data
    this.tokenService.setToken(loginResponse.token);
    this.tokenService.setRefreshToken(loginResponse.refreshToken);
    localStorage.setItem(API_CONFIG.AUTH.USER_KEY, JSON.stringify(user));
    
    // Update current user - use next() to immediately update the observable
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    this.tokenService.clearTokens();
    localStorage.removeItem(API_CONFIG.AUTH.USER_KEY);
  }

  private getStoredUser(): UserDto | null {
    const userJson = localStorage.getItem(API_CONFIG.AUTH.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }

  // Token expiration is now handled by TokenService
  // private isTokenExpired method removed - use tokenService.isTokenExpired() instead

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
