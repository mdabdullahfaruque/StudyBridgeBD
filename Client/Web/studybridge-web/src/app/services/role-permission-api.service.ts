/**
 * Role and Permission API Service
 * Handles all role and permission management API calls
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, RoleDto, PermissionDto, PaginatedResponse } from '../models/api.models';
import { API_ENDPOINTS, buildApiUrl } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class RolePermissionApiService {
  constructor(private apiService: ApiService) {}

  // Role Management
  getRoles(): Observable<ApiResponse<RoleDto[]>> {
    return this.apiService.get<RoleDto[]>(
      API_ENDPOINTS.ADMIN.GET_ROLES.path
    );
  }

  getRoleById(id: number): Observable<ApiResponse<RoleDto>> {
    return this.apiService.get<RoleDto>(
      buildApiUrl(API_ENDPOINTS.ADMIN.GET_ROLE.path, { id })
    );
  }

  createRole(role: Omit<RoleDto, 'id'>): Observable<ApiResponse<RoleDto>> {
    return this.apiService.post<RoleDto>(
      API_ENDPOINTS.ADMIN.CREATE_ROLE.path,
      role,
      { showErrorToast: true }
    );
  }

  updateRole(id: number, role: Partial<RoleDto>): Observable<ApiResponse<RoleDto>> {
    return this.apiService.put<RoleDto>(
      buildApiUrl(API_ENDPOINTS.ADMIN.UPDATE_ROLE.path, { id }),
      role,
      { showErrorToast: true }
    );
  }

  deleteRole(id: number): Observable<ApiResponse<void>> {
    return this.apiService.delete<void>(
      buildApiUrl(API_ENDPOINTS.ADMIN.DELETE_ROLE.path, { id }),
      { showErrorToast: true }
    );
  }

  // Permission Management
  getPermissions(): Observable<ApiResponse<PermissionDto[]>> {
    return this.apiService.get<PermissionDto[]>(
      API_ENDPOINTS.ADMIN.GET_PERMISSIONS.path
    );
  }
}