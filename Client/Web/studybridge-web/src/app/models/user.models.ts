export interface User {
  id: string;
  email: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
  avatarUrl?: string;
  roles: string[];
  isActive: boolean;
  lastLoginAt?: Date;
}

export interface UserProfile {
  id: string;
  userId: string;
  fullName?: string;
  dateOfBirth?: Date;
  phone?: string;
  country?: string;
  timeZone?: string;
  language?: string;
  notificationsEnabled: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
  firstName?: string;
  lastName?: string;
}

export interface GoogleLoginRequest {
  googleToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: Date;
  email: string;
  displayName: string;
  userId: string;
  roles: string[];
}

export interface ApiResponse<T> {
  data: T | null;
  message: string;
  statusCode: number;
  errors?: string[];
  timestamp: Date;
}
