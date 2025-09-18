/**
 * API Configuration for StudyBridge Application
 * Centralized configuration for all API endpoints and settings
 */

export const API_CONFIG = {
  // Base API URL - will be overridden by environment
  BASE_URL: 'http://localhost:5000/api',
  
  // Request timeout (in milliseconds)
  TIMEOUT: 30000,
  
  // Default retry attempts for failed requests
  DEFAULT_RETRY_ATTEMPTS: 3,
  
  // HTTP Status Codes
  STATUS_CODES: {
    OK: 200,
    CREATED: 201,
    NO_CONTENT: 204,
    BAD_REQUEST: 400,
    UNAUTHORIZED: 401,
    FORBIDDEN: 403,
    NOT_FOUND: 404,
    CONFLICT: 409,
    UNPROCESSABLE_ENTITY: 422,
    INTERNAL_SERVER_ERROR: 500,
    SERVICE_UNAVAILABLE: 503
  },
  
  // Authentication
  AUTH: {
    TOKEN_KEY: 'studybridge_token',
    REFRESH_TOKEN_KEY: 'studybridge_refresh_token',
    USER_KEY: 'studybridge_user',
    TOKEN_HEADER: 'Authorization',
    TOKEN_PREFIX: 'Bearer '
  },
  
  // Pagination defaults
  PAGINATION: {
    DEFAULT_PAGE_SIZE: 10,
    MAX_PAGE_SIZE: 100,
    DEFAULT_PAGE: 1
  }
};

/**
 * API Endpoints Configuration
 * All backend API endpoints organized by feature area
 */
export const API_ENDPOINTS = {
  // Authentication endpoints
  AUTH: {
    LOGIN: { path: '/auth/login', method: 'POST' as const, requiresAuth: false },
    REGISTER: { path: '/auth/register', method: 'POST' as const, requiresAuth: false },
    GOOGLE_LOGIN: { path: '/auth/google-login', method: 'POST' as const, requiresAuth: false },
    CHANGE_PASSWORD: { path: '/auth/change-password', method: 'POST' as const, requiresAuth: true },
    REFRESH_TOKEN: { path: '/auth/refresh-token', method: 'POST' as const, requiresAuth: false }
  },
  
  // Profile endpoints  
  PROFILE: {
    GET: { path: '/profile', method: 'GET' as const, requiresAuth: true },
    UPDATE: { path: '/profile', method: 'PUT' as const, requiresAuth: true }
  },
  
  // Admin endpoints
  ADMIN: {
    // User Management
    GET_USERS: { 
      path: '/admin/users', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['users.view'] 
    },
    CREATE_USER: { 
      path: '/admin/users', 
      method: 'POST' as const, 
      requiresAuth: true, 
      permissions: ['users.create'] 
    },
    GET_USER: { 
      path: '/admin/users/:id', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['users.view'] 
    },
    UPDATE_USER: { 
      path: '/admin/users/:id', 
      method: 'PUT' as const, 
      requiresAuth: true, 
      permissions: ['users.edit'] 
    },
    DELETE_USER: { 
      path: '/admin/users/:id', 
      method: 'DELETE' as const, 
      requiresAuth: true, 
      permissions: ['users.delete'] 
    },
    ASSIGN_ROLE: { 
      path: '/admin/users/:userId/assign-role', 
      method: 'POST' as const, 
      requiresAuth: true, 
      permissions: ['users.manage'] 
    },
    
    // Role Management
    GET_ROLES: { 
      path: '/admin/roles', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['roles.view'] 
    },
    CREATE_ROLE: { 
      path: '/admin/roles', 
      method: 'POST' as const, 
      requiresAuth: true, 
      permissions: ['roles.create'] 
    },
    GET_ROLE: { 
      path: '/admin/roles/:id', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['roles.view'] 
    },
    UPDATE_ROLE: { 
      path: '/admin/roles/:id', 
      method: 'PUT' as const, 
      requiresAuth: true, 
      permissions: ['roles.edit'] 
    },
    DELETE_ROLE: { 
      path: '/admin/roles/:id', 
      method: 'DELETE' as const, 
      requiresAuth: true, 
      permissions: ['roles.delete'] 
    },
    
    // Permission Management
    GET_PERMISSIONS: { 
      path: '/admin/permissions', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['permissions.view'] 
    },
    
    // Menu Management
    GET_ALL_MENUS: { 
      path: '/admin/menus', 
      method: 'GET' as const, 
      requiresAuth: true, 
      permissions: ['system.view'] 
    },
    GET_USER_MENUS: { 
      path: '/menu/user-menus', 
      method: 'GET' as const, 
      requiresAuth: true 
    }
  },
  
  // Public endpoints (future vocabulary, learning modules)
  PUBLIC: {
    GET_VOCABULARY: { path: '/public/vocabulary', method: 'GET' as const, requiresAuth: false },
    GET_CATEGORIES: { path: '/public/categories', method: 'GET' as const, requiresAuth: false },
    GET_PUBLIC_MENUS: { 
      path: '/admin/public-menus', 
      method: 'GET' as const, 
      requiresAuth: true 
    }
  }
};

/**
 * API Error Messages
 */
export const API_ERROR_MESSAGES = {
  NETWORK_ERROR: 'Network error. Please check your internet connection.',
  UNAUTHORIZED: 'Your session has expired. Please log in again.',
  FORBIDDEN: 'You do not have permission to perform this action.',
  NOT_FOUND: 'The requested resource was not found.',
  VALIDATION_ERROR: 'Please check your input and try again.',
  SERVER_ERROR: 'An unexpected error occurred. Please try again later.',
  TIMEOUT: 'Request timed out. Please try again.',
  UNKNOWN_ERROR: 'An unknown error occurred. Please try again.'
};

/**
 * Helper function to build API URLs with parameters
 */
export function buildApiUrl(endpoint: string, params?: Record<string, string | number>): string {
  let url = endpoint;
  
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      url = url.replace(`:${key}`, String(value));
    });
  }
  
  return url;
}

/**
 * Helper function to build query string from parameters
 */
export function buildQueryString(params: Record<string, any>): string {
  const filteredParams = Object.entries(params)
    .filter(([_, value]) => value !== undefined && value !== null && value !== '')
    .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`)
    .join('&');
    
  return filteredParams ? `?${filteredParams}` : '';
}