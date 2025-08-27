import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { ApiService } from './api';
import { User, LoginResponse, GoogleLoginRequest, AuthState } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authStateSubject = new BehaviorSubject<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: false
  });

  public authState$ = this.authStateSubject.asObservable();
  public isAuthenticated$ = this.authState$.pipe(map(state => state.isAuthenticated));
  public user$ = this.authState$.pipe(map(state => state.user));
  public isLoading$ = this.authState$.pipe(map(state => state.isLoading));

  constructor(private apiService: ApiService) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    const token = localStorage.getItem('token');
    const userData = localStorage.getItem('user');
    
    if (token && userData) {
      try {
        const user = JSON.parse(userData);
        this.updateAuthState({
          user,
          token,
          isAuthenticated: true,
          isLoading: false
        });
      } catch (error) {
        this.logout();
      }
    }
  }

  googleLogin(idToken: string): Observable<LoginResponse> {
    this.setLoading(true);
    
    const request: GoogleLoginRequest = { idToken };
    
    return this.apiService.post<LoginResponse>('auth/google', request).pipe(
      tap(response => {
        this.storeAuthData(response);
        this.updateAuthState({
          user: response.user,
          token: response.token,
          isAuthenticated: true,
          isLoading: false
        });
      }),
      tap({
        error: () => this.setLoading(false)
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    
    this.updateAuthState({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false
    });
  }

  getCurrentUser(): User | null {
    return this.authStateSubject.value.user;
  }

  isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  private storeAuthData(response: LoginResponse): void {
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(response.user));
  }

  private updateAuthState(newState: Partial<AuthState>): void {
    const currentState = this.authStateSubject.value;
    this.authStateSubject.next({ ...currentState, ...newState });
  }

  private setLoading(isLoading: boolean): void {
    this.updateAuthState({ isLoading });
  }
}
