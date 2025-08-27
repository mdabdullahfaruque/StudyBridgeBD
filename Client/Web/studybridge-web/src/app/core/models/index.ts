// Core Models
export interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

// API Response
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

// Auth State
export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}
