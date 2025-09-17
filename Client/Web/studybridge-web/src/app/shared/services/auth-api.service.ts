/**
 * Authentication API Service
 * Handles all authentication-related API calls
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  ApiResponse, 
  LoginRequest, 
  LoginResponse, 
  RegisterRequest, 
  ChangePasswordRequest, 
  GoogleLoginRequest,
  UserDto
} from '../models/api.models';
import { API_ENDPOINTS } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  constructor(private apiService: ApiService) {}

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.apiService.post<LoginResponse>(
      API_ENDPOINTS.AUTH.LOGIN.path,
      request,
      { showErrorToast: true }
    );
  }

  register(request: RegisterRequest): Observable<ApiResponse<UserDto>> {
    return this.apiService.post<UserDto>(
      API_ENDPOINTS.AUTH.REGISTER.path,
      request,
      { showErrorToast: true }
    );
  }

  googleLogin(request: GoogleLoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.apiService.post<LoginResponse>(
      API_ENDPOINTS.AUTH.GOOGLE_LOGIN.path,
      request,
      { showErrorToast: true }
    );
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<void>> {
    return this.apiService.post<void>(
      API_ENDPOINTS.AUTH.CHANGE_PASSWORD.path,
      request,
      { showErrorToast: true }
    );
  }

  refreshToken(refreshToken: string): Observable<ApiResponse<LoginResponse>> {
    return this.apiService.post<LoginResponse>(
      API_ENDPOINTS.AUTH.REFRESH_TOKEN.path,
      { refreshToken },
      { 
        showErrorToast: false,
        suppressGlobalErrorHandling: true 
      }
    );
  }
}