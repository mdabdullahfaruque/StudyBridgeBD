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
    try {
      const token = this.tokenService.getToken();
      const user = this.getStoredUser();
      
      if (token && user && !this.tokenService.isTokenExpired()) {
        this.currentUserSubject.next(user);
      } else {
        this.clearAuthData();
        // No mock user - user needs to login properly
      }
    } catch (error) {
      console.error('Error initializing auth state:', error);
      // Clear any corrupted data and start fresh
      this.clearAuthData();
    }
  }



  // Authentication Methods
  async login(credentials: LoginRequest): Promise<any> {
    console.log('Login called with:', credentials);
    this.isLoadingSubject.next(true);
    
    try {
      console.log('Making API call to:', `${this.apiUrl}/auth/login`);
      const response = await this.http.post<any>(
        `${this.apiUrl}/auth/login`,
        credentials
      ).toPromise();

      console.log('API response received:', response);

      if (response?.data) {
        console.log('Response data exists, calling handleAuthSuccess');
        this.handleAuthSuccess(response.data);
        return response.data;
      }
      
      console.error('No response data found');
      throw new Error('Invalid response format');
    } catch (error) {
      console.error('Login error:', error);
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
    return user?.roles?.some(role => role === roleName) ?? false;
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(roleName => this.hasRole(roleName));
  }

  hasPermission(permissionKey: string): boolean {
    const user = this.getCurrentUser();
    if (!user?.permissions) return false;
    
    return user.permissions.includes(permissionKey);
  }

  // Role-based navigation helpers
  getRedirectUrlForUser(): string {
    const user = this.getCurrentUser();
    console.log('getRedirectUrlForUser - Current user:', user);
    
    if (!user) {
      return '/auth/login';
    }
    
    // Use IsPublicUser flag for redirection
    // true = public layout, false = admin layout
    if (user.isPublicUser) {
      return '/public/dashboard';
    } else {
      return '/admin/dashboard';
    }
  }

  /**
   * Check if user has admin role based on actual role data from API
   * This method dynamically determines admin roles instead of using hardcoded values
   */
  hasAdminRole(user?: UserDto): boolean {
    const currentUser = user || this.getCurrentUser();
    if (!currentUser?.roles) return false;
    
    // Check for admin-type roles dynamically
    // Any role that is not 'User' is considered administrative
    return currentUser.roles.some(role => 
      role && role.toLowerCase() !== 'user'
    );
  }

  isAdminUser(): boolean {
    return this.hasAdminRole();
  }

  /**
   * Navigate user to appropriate dashboard after login
   */
  async navigateToUserDashboard(): Promise<void> {
    console.log('navigateToUserDashboard called');
    
    // Get redirect URL first
    const redirectUrl = this.getRedirectUrlForUser();
    console.log('Navigating to:', redirectUrl);
    
    // Navigate immediately - don't wait for menu loading
    const navigationResult = await this.router.navigate([redirectUrl]);
    console.log('Navigation result:', navigationResult);

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
  private handleAuthSuccess(loginResponse: any): void {
    console.log('handleAuthSuccess called with:', loginResponse);
    
    // Backend sends user data directly in response, not in a nested 'user' object
    const user: UserDto = {
      id: loginResponse.userId,
      email: loginResponse.email,
      firstName: loginResponse.displayName?.split(' ')[0] || '',
      lastName: loginResponse.displayName?.split(' ').slice(1).join(' ') || '',
      displayName: loginResponse.displayName,
      isActive: true,
      emailConfirmed: true,
      roles: loginResponse.roles || [],
      permissions: loginResponse.permissions || [],
      subscriptions: loginResponse.subscriptions || [],
      createdAt: new Date().toISOString(),
      isPublicUser: loginResponse.isPublicUser ?? true
    };
    
    console.log('Constructed user object:', user);

    // Store auth data
    this.tokenService.setToken(loginResponse.token);
    this.tokenService.setRefreshToken(loginResponse.refreshToken || '');
    localStorage.setItem(API_CONFIG.AUTH.USER_KEY, JSON.stringify(user));
    
    // Update current user - use next() to immediately update the observable
    this.currentUserSubject.next(user);
    
    console.log('User state updated, current user now:', this.getCurrentUser());
  }

  private clearAuthData(): void {
    this.tokenService.clearTokens();
    localStorage.removeItem(API_CONFIG.AUTH.USER_KEY);
  }

  private getStoredUser(): UserDto | null {
    try {
      const userJson = localStorage.getItem(API_CONFIG.AUTH.USER_KEY);
      if (!userJson || userJson === 'undefined' || userJson === 'null') {
        return null;
      }
      return JSON.parse(userJson);
    } catch (error) {
      console.error('Error parsing stored user data:', error);
      // Clear corrupted data
      localStorage.removeItem(API_CONFIG.AUTH.USER_KEY);
      return null;
    }
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
