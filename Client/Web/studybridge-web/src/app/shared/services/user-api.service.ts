/**
 * User API Service
 * Handles all user-related API calls
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { 
  ApiResponse, 
  UserDto, 
  UserProfileDto, 
  PaginatedResponse,
  UserQueryParameters,
  CreateUserRequest,
  UpdateUserRequest
} from '../models/api.models';
import { API_ENDPOINTS, buildApiUrl } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {
  constructor(private apiService: ApiService) {}

  // User CRUD operations
  getUsers(params?: UserQueryParameters): Observable<ApiResponse<PaginatedResponse<UserDto>>> {
    return this.apiService.get<PaginatedResponse<UserDto>>(
      API_ENDPOINTS.ADMIN.GET_USERS.path,
      { params }
    );
  }

  getUserById(id: number): Observable<ApiResponse<UserDto>> {
    return this.apiService.get<UserDto>(
      buildApiUrl(API_ENDPOINTS.ADMIN.GET_USER.path, { id })
    );
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<UserDto>> {
    return this.apiService.post<UserDto>(
      API_ENDPOINTS.ADMIN.CREATE_USER.path,
      request,
      { showErrorToast: true }
    );
  }

  updateUser(id: number, request: UpdateUserRequest): Observable<ApiResponse<UserDto>> {
    return this.apiService.put<UserDto>(
      buildApiUrl(API_ENDPOINTS.ADMIN.UPDATE_USER.path, { id }),
      request,
      { showErrorToast: true }
    );
  }

  deleteUser(id: number): Observable<ApiResponse<void>> {
    return this.apiService.delete<void>(
      buildApiUrl(API_ENDPOINTS.ADMIN.DELETE_USER.path, { id }),
      { showErrorToast: true }
    );
  }

  // Profile operations
  getProfile(): Observable<ApiResponse<UserProfileDto>> {
    return this.apiService.get<UserProfileDto>(
      API_ENDPOINTS.PROFILE.GET.path
    );
  }

  updateProfile(request: Partial<UserProfileDto>): Observable<ApiResponse<UserProfileDto>> {
    return this.apiService.put<UserProfileDto>(
      API_ENDPOINTS.PROFILE.UPDATE.path,
      request,
      { showErrorToast: true }
    );
  }

  // Role Assignment
  assignRole(userId: number, roleId: number): Observable<ApiResponse<void>> {
    return this.apiService.post<void>(
      buildApiUrl(API_ENDPOINTS.ADMIN.ASSIGN_ROLE.path, { userId }),
      { roleId },
      { showErrorToast: true }
    );
  }
}