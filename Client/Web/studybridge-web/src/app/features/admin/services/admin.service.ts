import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  isActive: boolean;
  isEmailVerified: boolean;
  roles: string[];
  permissions: string[];
  subscriptions?: UserSubscription[];
  createdAt: string;
  lastLoginAt?: string;
}

export interface UserSubscription {
  id: string;
  plan: string;
  status: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface SystemRole {
  id: number;
  name: string;
  displayName: string;
  description?: string;
  isActive: boolean;
}

export interface Permission {
  id: string;
  key: string;
  displayName: string;
  description?: string;
  category: string;
  type: PermissionType;
}

export enum PermissionType {
  Read = 0,
  Write = 1,
  Delete = 2,
  Execute = 3,
  Admin = 4
}

export interface Menu {
  id: string;
  name: string;
  displayName: string;
  icon?: string;
  url?: string;
  parentId?: string;
  order: number;
  isActive: boolean;
  type: MenuType;
  requiredPermissions: string[];
  children?: Menu[];
}

export enum MenuType {
  Page = 0,
  Group = 1,
  External = 2
}

export interface CreateUserRequest {
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  password?: string;
  roles?: string[];
}

export interface UpdateUserRequest {
  displayName?: string;
  firstName?: string;
  lastName?: string;
  isActive?: boolean;
}

export interface AssignRoleRequest {
  userId: string;
  roleNames: string[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface GetUsersRequest {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  role?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;
  private currentUserSubject = new BehaviorSubject<AdminUser | null>(null);
  private userMenusSubject = new BehaviorSubject<Menu[]>([]);

  constructor(private http: HttpClient) {}

  // User Management
  getUsers(request: GetUsersRequest = {}): Observable<ApiResponse<PaginatedResult<AdminUser>>> {
    let params = new HttpParams();
    
    if (request.pageNumber) params = params.set('pageNumber', request.pageNumber.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request.searchTerm) params = params.set('searchTerm', request.searchTerm);
    if (request.role) params = params.set('role', request.role);
    if (request.isActive !== undefined) params = params.set('isActive', request.isActive.toString());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<ApiResponse<PaginatedResult<AdminUser>>>(`${this.apiUrl}/users`, { params });
  }

  getUserById(id: string): Observable<ApiResponse<AdminUser>> {
    return this.http.get<ApiResponse<AdminUser>>(`${this.apiUrl}/users/${id}`);
  }

  createUser(request: CreateUserRequest): Observable<ApiResponse<AdminUser>> {
    return this.http.post<ApiResponse<AdminUser>>(`${this.apiUrl}/users`, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<ApiResponse<AdminUser>> {
    return this.http.put<ApiResponse<AdminUser>>(`${this.apiUrl}/users/${id}`, request);
  }

  deleteUser(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/users/${id}`);
  }

  activateUser(id: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/users/${id}/activate`, {});
  }

  deactivateUser(id: string): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/users/${id}/deactivate`, {});
  }

  // Role Management
  getRoles(): Observable<ApiResponse<SystemRole[]>> {
    return this.http.get<ApiResponse<SystemRole[]>>(`${this.apiUrl}/roles`);
  }

  getUserRoles(userId: string): Observable<ApiResponse<SystemRole[]>> {
    return this.http.get<ApiResponse<SystemRole[]>>(`${this.apiUrl}/users/${userId}/roles`);
  }

  assignRolesToUser(request: AssignRoleRequest): Observable<ApiResponse<void>> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/users/assign-roles`, request);
  }

  removeRoleFromUser(userId: string, roleName: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/users/${userId}/roles/${roleName}`);
  }

  // Permission Management
  getPermissions(): Observable<ApiResponse<Permission[]>> {
    return this.http.get<ApiResponse<Permission[]>>(`${this.apiUrl}/permissions`);
  }

  getUserPermissions(userId: string): Observable<ApiResponse<Permission[]>> {
    return this.http.get<ApiResponse<Permission[]>>(`${this.apiUrl}/users/${userId}/permissions`);
  }

  hasPermission(permission: string): Observable<ApiResponse<boolean>> {
    return this.http.get<ApiResponse<boolean>>(`${this.apiUrl}/permissions/check/${permission}`);
  }

  // Menu Management
  getUserMenus(): Observable<ApiResponse<Menu[]>> {
    return this.http.get<ApiResponse<Menu[]>>(`${this.apiUrl}/menus/user-menus`);
  }

  getAllMenus(): Observable<ApiResponse<Menu[]>> {
    return this.http.get<ApiResponse<Menu[]>>(`${this.apiUrl}/menus`);
  }

  // System Information
  getSystemInfo(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/system/info`);
  }

  getDatabaseStatus(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/system/database-status`);
  }

  // Dashboard Analytics
  getDashboardStats(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/dashboard/stats`);
  }

  getUserGrowthData(days: number = 30): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/dashboard/user-growth?days=${days}`);
  }

  // Reactive state management
  getCurrentUser(): Observable<AdminUser | null> {
    return this.currentUserSubject.asObservable();
  }

  setCurrentUser(user: AdminUser | null): void {
    this.currentUserSubject.next(user);
  }

  getUserMenus$(): Observable<Menu[]> {
    return this.userMenusSubject.asObservable();
  }

  setUserMenus(menus: Menu[]): void {
    this.userMenusSubject.next(menus);
  }

  // Utility methods
  buildMenuHierarchy(flatMenus: Menu[]): Menu[] {
    const menuMap = new Map<string, Menu>();
    const rootMenus: Menu[] = [];

    // First pass: create map of all menus
    flatMenus.forEach(menu => {
      menuMap.set(menu.id, { ...menu, children: [] });
    });

    // Second pass: build hierarchy
    flatMenus.forEach(menu => {
      const menuItem = menuMap.get(menu.id);
      if (!menuItem) return;

      if (menu.parentId) {
        const parent = menuMap.get(menu.parentId);
        if (parent) {
          parent.children = parent.children || [];
          parent.children.push(menuItem);
        }
      } else {
        rootMenus.push(menuItem);
      }
    });

    // Sort by order
    const sortMenus = (menus: Menu[]): Menu[] => {
      return menus
        .sort((a, b) => a.order - b.order)
        .map(menu => ({
          ...menu,
          children: menu.children ? sortMenus(menu.children) : undefined
        }));
    };

    return sortMenus(rootMenus);
  }

  hasAnyPermission(permissions: string[], userPermissions: string[]): boolean {
    if (!permissions || permissions.length === 0) return true;
    return permissions.some(permission => userPermissions.includes(permission));
  }

  filterMenusByPermissions(menus: Menu[], userPermissions: string[]): Menu[] {
    return menus
      .filter(menu => this.hasAnyPermission(menu.requiredPermissions, userPermissions))
      .map(menu => ({
        ...menu,
        children: menu.children 
          ? this.filterMenusByPermissions(menu.children, userPermissions)
          : undefined
      }))
      .filter(menu => !menu.children || menu.children.length > 0);
  }
}