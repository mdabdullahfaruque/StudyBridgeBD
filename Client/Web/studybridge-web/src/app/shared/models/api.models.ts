/**
 * Generic API Response Models for StudyBridge Frontend
 * These models match the backend ApiResponse<T> structure
 */

// Generic API Response wrapper (matches backend ApiResponse<T> structure)
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

// HTTP Request Options
export interface ApiRequestOptions {
  showErrorToast?: boolean;
  showSuccessToast?: boolean;
  suppressGlobalErrorHandling?: boolean;
  params?: any;
  headers?: Record<string, string>;
  isFormData?: boolean;
  timeout?: number;
  retries?: number;
}

// API Error Response (for error handling)
export interface ApiError {
  message: string;
  statusCode: number;
  errors: string[];
  timestamp: string;
  path?: string;
  method?: string;
}

// Pagination Response
export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Pagination Request
export interface PaginationRequest {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  searchTerm?: string;
}

// File Upload
export interface FileUploadResponse {
  fileName: string;
  fileUrl: string;
  fileSize: number;
  contentType: string;
}

// Menu Models (matching backend DTOs)
export interface MenuDto {
  id: string;
  name: string;
  displayName: string;
  icon?: string;
  route?: string;
  parentId?: string;
  sortOrder: number;
  isActive: boolean;
  menuType: number;
  requiredPermissions: string[];
  children?: MenuDto[];
}

// User Models (matching backend DTOs)
export interface UserDto {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  displayName: string;
  avatarUrl?: string;
  isActive: boolean;
  emailConfirmed: boolean;
  roles: string[]; // Updated to match backend response
  permissions: string[];
  subscriptions: UserSubscriptionDto[];
  createdAt: string;
  lastLoginAt?: string;
  isPublicUser?: boolean; // Made optional as backend doesn't include this
}

export interface UserSubscriptionDto {
  id: string;
  plan: string;
  status: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  systemRole: number;
  permissions: PermissionDto[];
}

export interface PermissionDto {
  id: string;
  permissionKey: string;
  displayName: string;
  description?: string;
  permissionType: number;
  isSystemPermission: boolean;
  menuName?: string;
}

// Authentication Models
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: UserDto;
  expiresAt: string;
  isPublicUser: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  displayName?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

// Profile Models  
export interface ProfileUpdateRequest {
  firstName: string;
  lastName: string;
  displayName?: string;
  avatarUrl?: string;
}

// Admin Models
// Additional User-related DTOs
export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roleId?: number;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  email?: string;
  roleId?: number;
}

export interface UserProfileDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  profilePictureUrl?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  bio?: string;
  location?: string;
  website?: string;
  socialLinks?: { [key: string]: string };
}

export interface UserQueryParameters {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  roleId?: number;
  isActive?: boolean;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface AssignRoleRequest {
  roleId: string;
}

// API Endpoint Configuration
export interface ApiEndpoint {
  path: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  requiresAuth?: boolean;
  permissions?: string[];
}

// Loading State
export interface LoadingState {
  [key: string]: boolean;
}