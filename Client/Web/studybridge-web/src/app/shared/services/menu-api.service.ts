/**
 * Menu API Service
 * Handles all menu-related API calls for navigation and access control
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, MenuDto } from '../models/api.models';
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
   * Get public menus (for unauthenticated users)
   */
  getPublicMenus(): Observable<ApiResponse<MenuDto[]>> {
    return this.apiService.get<MenuDto[]>(
      API_ENDPOINTS.PUBLIC.GET_VOCABULARY.path, // This should be updated to correct public menu endpoint
      { 
        showErrorToast: false
      }
    );
  }
}