/**
 * Generic API Service for StudyBridge Application
 * Provides a reusable, type-safe HTTP client with error handling,
 * authentication, loading states, and retry logic
 */

import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, retry, timeout, finalize } from 'rxjs/operators';
import { 
  ApiResponse, 
  ApiRequestOptions, 
  ApiError, 
  PaginatedResponse, 
  PaginationRequest,
  LoadingState 
} from '../models/api.models';
import { 
  API_CONFIG, 
  API_ENDPOINTS, 
  API_ERROR_MESSAGES, 
  buildApiUrl, 
  buildQueryString 
} from './api.config';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = API_CONFIG.BASE_URL;
  private loadingSubject = new BehaviorSubject<LoadingState>({});
  public loading$ = this.loadingSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  // Generic GET request
  get<T>(
    endpoint: string, 
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const loadingKey = `GET_${endpoint}`;
    this.setLoading(loadingKey, true);
    
    return this.http.get<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      { 
        headers: this.buildHeaders(options.headers),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT),
      retry(options.retries || 0),
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Generic POST request
  post<T>(
    endpoint: string, 
    data: any, 
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const loadingKey = `POST_${endpoint}`;
    this.setLoading(loadingKey, true);
    
    return this.http.post<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      data,
      { 
        headers: this.buildHeaders(options.headers),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT),
      retry(options.retries || 0),
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Generic PUT request
  put<T>(
    endpoint: string, 
    data: any, 
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const loadingKey = `PUT_${endpoint}`;
    this.setLoading(loadingKey, true);
    
    return this.http.put<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      data,
      { 
        headers: this.buildHeaders(options.headers),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT),
      retry(options.retries || 0),
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Generic DELETE request
  delete<T>(
    endpoint: string, 
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const loadingKey = `DELETE_${endpoint}`;
    this.setLoading(loadingKey, true);
    
    return this.http.delete<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      { 
        headers: this.buildHeaders(options.headers),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT),
      retry(options.retries || 0),
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Generic PATCH request
  patch<T>(
    endpoint: string, 
    data: any, 
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const loadingKey = `PATCH_${endpoint}`;
    this.setLoading(loadingKey, true);
    
    return this.http.patch<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      data,
      { 
        headers: this.buildHeaders(options.headers),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT),
      retry(options.retries || 0),
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Paginated GET request
  getPaginated<T>(
    endpoint: string,
    paginationRequest: PaginationRequest = {},
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<PaginatedResponse<T>>> {
    const params = {
      pageNumber: paginationRequest.pageNumber || API_CONFIG.PAGINATION.DEFAULT_PAGE,
      pageSize: Math.min(
        paginationRequest.pageSize || API_CONFIG.PAGINATION.DEFAULT_PAGE_SIZE,
        API_CONFIG.PAGINATION.MAX_PAGE_SIZE
      ),
      ...paginationRequest.sortBy && { sortBy: paginationRequest.sortBy },
      ...paginationRequest.sortDirection && { sortDirection: paginationRequest.sortDirection },
      ...paginationRequest.searchTerm && { searchTerm: paginationRequest.searchTerm },
      ...options.params
    };

    return this.get<PaginatedResponse<T>>(endpoint, { ...options, params });
  }

  // File upload
  uploadFile<T>(
    endpoint: string,
    file: File,
    additionalData: Record<string, any> = {},
    options: ApiRequestOptions = {}
  ): Observable<ApiResponse<T>> {
    const formData = new FormData();
    formData.append('file', file);
    
    Object.entries(additionalData).forEach(([key, value]) => {
      formData.append(key, value);
    });

    const loadingKey = `UPLOAD_${endpoint}`;
    this.setLoading(loadingKey, true);

    return this.http.post<ApiResponse<T>>(
      `${this.baseUrl}${endpoint}`,
      formData,
      { 
        headers: this.buildHeaders(options.headers, true),
        params: this.buildParams(options.params)
      }
    ).pipe(
      timeout(options.timeout || API_CONFIG.TIMEOUT * 3), // Longer timeout for uploads
      map(response => this.handleSuccess(response)),
      catchError(error => this.handleError(error, options)),
      finalize(() => this.setLoading(loadingKey, false))
    );
  }

  // Helper method to check if a specific operation is loading
  isLoading(operation: string): Observable<boolean> {
    return this.loading$.pipe(
      map(loadingState => loadingState[operation] || false)
    );
  }

  // Helper method to get current loading state
  getCurrentLoadingState(): LoadingState {
    return this.loadingSubject.value;
  }

  // Private helper methods
  private buildHeaders(customHeaders?: Record<string, string>, isFileUpload = false): HttpHeaders {
    let headers = new HttpHeaders();
    
    if (!isFileUpload) {
      headers = headers.set('Content-Type', 'application/json');
    }
    
    // Add authentication token if available
    const token = this.authService.getToken();
    if (token) {
      headers = headers.set(API_CONFIG.AUTH.TOKEN_HEADER, `${API_CONFIG.AUTH.TOKEN_PREFIX}${token}`);
    }

    // Add custom headers
    if (customHeaders) {
      Object.entries(customHeaders).forEach(([key, value]) => {
        headers = headers.set(key, value);
      });
    }

    return headers;
  }

  private buildParams(params?: Record<string, any>): HttpParams {
    let httpParams = new HttpParams();
    
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          if (Array.isArray(value)) {
            value.forEach(v => httpParams = httpParams.append(key, v));
          } else {
            httpParams = httpParams.set(key, value.toString());
          }
        }
      });
    }
    
    return httpParams;
  }

  private handleSuccess<T>(response: ApiResponse<T>): ApiResponse<T> {
    if (!response.success && response.errors?.length) {
      // Even if the HTTP request succeeded, the API might return success: false
      throw new Error(response.errors.join(', ') || response.message);
    }
    return response;
  }

  private handleError(error: HttpErrorResponse, options: ApiRequestOptions): Observable<never> {
    let apiError: ApiError;
    
    if (error.error && typeof error.error === 'object') {
      // Backend returned an error response
      apiError = {
        message: error.error.message || API_ERROR_MESSAGES.UNKNOWN_ERROR,
        statusCode: error.status,
        errors: error.error.errors || [],
        timestamp: error.error.timestamp || new Date().toISOString()
      };
    } else {
      // Network or other error
      apiError = {
        message: this.getErrorMessage(error.status),
        statusCode: error.status || 0,
        errors: [error.message || API_ERROR_MESSAGES.NETWORK_ERROR],
        timestamp: new Date().toISOString()
      };
    }

    // Handle specific error cases
    this.handleSpecificErrors(apiError);

    // Show error notification if not suppressed
    if (options.showErrorToast !== false && !options.suppressGlobalErrorHandling) {
      this.notificationService.showError(apiError.message);
    }

    return throwError(() => apiError);
  }

  private handleSpecificErrors(error: ApiError): void {
    switch (error.statusCode) {
      case API_CONFIG.STATUS_CODES.UNAUTHORIZED:
        this.authService.logout();
        break;
      case API_CONFIG.STATUS_CODES.FORBIDDEN:
        // Could redirect to access denied page
        break;
      default:
        // Other error handling
        break;
    }
  }

  private getErrorMessage(statusCode: number): string {
    switch (statusCode) {
      case API_CONFIG.STATUS_CODES.UNAUTHORIZED:
        return API_ERROR_MESSAGES.UNAUTHORIZED;
      case API_CONFIG.STATUS_CODES.FORBIDDEN:
        return API_ERROR_MESSAGES.FORBIDDEN;
      case API_CONFIG.STATUS_CODES.NOT_FOUND:
        return API_ERROR_MESSAGES.NOT_FOUND;
      case API_CONFIG.STATUS_CODES.BAD_REQUEST:
      case API_CONFIG.STATUS_CODES.UNPROCESSABLE_ENTITY:
        return API_ERROR_MESSAGES.VALIDATION_ERROR;
      case API_CONFIG.STATUS_CODES.INTERNAL_SERVER_ERROR:
      case API_CONFIG.STATUS_CODES.SERVICE_UNAVAILABLE:
        return API_ERROR_MESSAGES.SERVER_ERROR;
      case 0:
        return API_ERROR_MESSAGES.NETWORK_ERROR;
      default:
        return API_ERROR_MESSAGES.UNKNOWN_ERROR;
    }
  }

  private setLoading(key: string, loading: boolean): void {
    const currentState = this.loadingSubject.value;
    if (loading) {
      this.loadingSubject.next({ ...currentState, [key]: true });
    } else {
      const newState = { ...currentState };
      delete newState[key];
      this.loadingSubject.next(newState);
    }
  }
}