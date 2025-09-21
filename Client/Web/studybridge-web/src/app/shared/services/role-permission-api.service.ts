/**
 * Role and Permission API Service
 * Handles all role and permission management API calls
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, RoleDto, PermissionDto } from '../models/api.models';
import { API_ENDPOINTS, buildApiUrl } from './api.config';

export interface CreateRoleRequest {
  name: string;
  description?: string;
  isActive?: boolean;
  systemRole?: number;
  permissionIds?: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
  isActive?: boolean;
  systemRole?: number;
  permissionIds?: string[];
}

export interface RoleResponse {
  role: RoleDto;
  message: string;
}

export interface RolesResponse {
  roles: RoleDto[];
  message: string;
}

export interface PermissionTreeResponse {
  permissionTree: PermissionTreeNode[];
  message: string;
}

export interface PermissionTreeNode {
  id: string;
  key: string;
  label: string;
  icon?: string;
  type: string;
  description: string;
  isActive: boolean;
  isSystemPermission: boolean;
  parentId?: string;
  children: PermissionTreeNode[];
  data?: PermissionData;
}

export interface PermissionData {
  menuId: string;
  menuName: string;
  permissionType: string;
  sortOrder: number;
}

@Injectable({
  providedIn: 'root'
})
export class RolePermissionApiService {
  constructor(private apiService: ApiService) {}

  // Role Management
  getRoles(): Observable<ApiResponse<RolesResponse>> {
    return this.apiService.get<RolesResponse>(
      API_ENDPOINTS.ADMIN.GET_ROLES.path
    );
  }

  getRoleById(id: string): Observable<ApiResponse<RoleResponse>> {
    return this.apiService.get<RoleResponse>(
      buildApiUrl(API_ENDPOINTS.ADMIN.GET_ROLE.path, { id })
    );
  }

  createRole(role: CreateRoleRequest): Observable<ApiResponse<RoleResponse>> {
    return this.apiService.post<RoleResponse>(
      API_ENDPOINTS.ADMIN.CREATE_ROLE.path,
      role,
      { showErrorToast: true }
    );
  }

  updateRole(id: string, role: UpdateRoleRequest): Observable<ApiResponse<RoleResponse>> {
    return this.apiService.put<RoleResponse>(
      buildApiUrl(API_ENDPOINTS.ADMIN.UPDATE_ROLE.path, { id }),
      role,
      { showErrorToast: true }
    );
  }

  deleteRole(id: string, forceDelete: boolean = false): Observable<ApiResponse<any>> {
    const url = buildApiUrl(API_ENDPOINTS.ADMIN.DELETE_ROLE.path, { id });
    const queryParam = forceDelete ? '?forceDelete=true' : '';
    
    return this.apiService.delete<any>(
      url + queryParam,
      { showErrorToast: true }
    );
  }

  // Permission Management
  getPermissions(): Observable<ApiResponse<PermissionTreeResponse>> {
    return this.apiService.get<PermissionTreeResponse>(
      API_ENDPOINTS.ADMIN.GET_PERMISSIONS.path
    );
  }
}