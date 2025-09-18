/**
 * Menu API Service
 * Handles all menu-related API calls for navigation and access control
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, MenuDto, CreateMenuRequest, UpdateMenuRequest } from '../models/api.models';
import { API_ENDPOINTS } from './api.config';

@Injectable({
  providedIn: 'root'
})
export class MenuApiService {
  constructor(private apiService: ApiService) {}

  /**
   * Get menus available to the current user based on their role and permissions
   */
  getUserMenus(): Observable<ApiResponse<MenuDto[]>> {
    return this.apiService.get<MenuDto[]>(
      API_ENDPOINTS.ADMIN.GET_USER_MENUS.path,
      { 
        showErrorToast: false // Don't show errors for menu loading
      }
    );
  }

  /**
   * Get all menus (admin only)
   */
  getAllMenus(): Observable<ApiResponse<MenuDto[]>> {
    return this.apiService.get<MenuDto[]>(
      API_ENDPOINTS.ADMIN.GET_ALL_MENUS.path
    );
  }

  /**
   * Get public menus (for authenticated regular users)
   */
  getPublicMenus(): Observable<ApiResponse<MenuDto[]>> {
    return this.apiService.get<MenuDto[]>(
      API_ENDPOINTS.PUBLIC.GET_PUBLIC_MENUS.path,
      { 
        showErrorToast: false
      }
    );
  }

  /**
   * Get menu by ID
   */
  getMenuById(id: string): Observable<ApiResponse<MenuDto>> {
    return this.apiService.get<MenuDto>(
      `${API_ENDPOINTS.ADMIN.GET_ALL_MENUS.path}/${id}`
    );
  }

  /**
   * Create a new menu
   */
  createMenu(request: CreateMenuRequest): Observable<ApiResponse<MenuDto>> {
    return this.apiService.post<MenuDto>(
      API_ENDPOINTS.ADMIN.GET_ALL_MENUS.path,
      request
    );
  }

  /**
   * Update an existing menu
   */
  updateMenu(id: string, request: UpdateMenuRequest): Observable<ApiResponse<MenuDto>> {
    return this.apiService.put<MenuDto>(
      `${API_ENDPOINTS.ADMIN.GET_ALL_MENUS.path}/${id}`,
      request
    );
  }

  /**
   * Delete a menu
   */
  deleteMenu(id: string): Observable<ApiResponse<void>> {
    return this.apiService.delete<void>(
      `${API_ENDPOINTS.ADMIN.GET_ALL_MENUS.path}/${id}`
    );
  }
}