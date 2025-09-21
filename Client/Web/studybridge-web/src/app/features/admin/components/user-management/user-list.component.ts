/**
 * User List Component for Admin Dashboard
 * 
 * Implementation following the standardized admin table pattern using the
 * app-table-wrapper component as mandated by the UI Components Guide.
 * 
 * Key Implementation Notes:
 * - Uses AdminService to fetch users with proper pagination
 * - User roles handled as array of strings from backend
 * - Uses 'status' column type for styled badges in table display
 * - Processes raw data into display-friendly format before table binding
 * 
 * Follows the reference pattern from role-list.component.ts
 * See docs/ADMIN_TABLE_IMPLEMENTATION.md for the complete pattern guide.
 */

import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Dialog for Modal
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

// PrimeNG Table Wrapper - MANDATORY per UI Components Guide
import { TableWrapperComponent, TableColumn, TableConfig } from '../../../../shared/table-wrapper/table-wrapper.component';

// User Form Component (we'll create this temporarily as a simple form)
// For now, we'll show a message about creating the form component

// API Service and Models
import { AdminService, AdminUser, PaginatedResult, ApiResponse, GetUsersRequest, GetUsersResponse } from '../../services/admin.service';

// Toast Service for notifications
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    ButtonModule,
    TableWrapperComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Data properties
  users: AdminUser[] = [];
  processedUsers: any[] = [];
  selectedUsers: AdminUser[] = [];
  loading = false;

  // Dialog state
  showUserDialog = signal(false);
  dialogTitle = signal('Create User');
  editingUser = signal<AdminUser | null>(null);

  // Table column configuration per UI Components Guide
  columns: TableColumn[] = [
    {
      field: 'email',
      header: 'Email',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '250px'
    },
    {
      field: 'firstName',
      header: 'First Name',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '150px'
    },
    {
      field: 'lastName',
      header: 'Last Name',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '150px'
    },
    {
      field: 'rolesDisplay',
      header: 'Roles',
      type: 'text',
      sortable: false,
      filterable: true,
      filterType: 'text',
      width: '200px'
    },
    {
      field: 'statusDisplay',
      header: 'Status',
      type: 'status',
      sortable: true,
      filterable: false,
      width: '120px'
    },
    {
      field: 'emailVerifiedDisplay',
      header: 'Email Verified',
      type: 'status',
      sortable: true,
      filterable: false,
      width: '140px'
    },
    {
      field: 'lastLoginAt',
      header: 'Last Login',
      type: 'date',
      sortable: true,
      filterable: true,
      filterType: 'date',
      width: '180px'
    },
    {
      field: 'createdAt',
      header: 'Created',
      type: 'date',
      sortable: true,
      filterable: true,
      filterType: 'date',
      width: '180px'
    },
    // Actions will be handled through row selection and toolbar buttons
  ];

  // Table configuration following UI Components Guide
  tableConfig: TableConfig = {
    selectionMode: 'multiple',
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [10, 25, 50],
    exportable: true,
    showGridlines: true,
    showHeader: true,
    resizableColumns: true,
    reorderableColumns: true
  };

  constructor(
    private adminService: AdminService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.processedUsers = []; // Initialize empty array
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load users from API using AdminService
   */
  private loadUsers(): void {
    this.loading = true;
    this.tableConfig = { ...this.tableConfig, loading: true };

    const request: GetUsersRequest = {
      pageNumber: 1,
      pageSize: 50, // Load more users initially
      sortBy: 'createdAt',
      sortDirection: 'desc'
    };

    this.adminService.getUsers(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<GetUsersResponse>) => {
          if (response.success && response.data?.users?.items) {
            this.users = response.data.users.items;
            this.processUsersForDisplay();
            this.toastService.success('Success', `Loaded ${this.users.length} users`);
          } else {
            this.users = [];
            this.processUsersForDisplay();
            this.handleError('Failed to load users', response.message || 'Unknown error from API');
          }
        },
        error: (error: any) => {
          console.error('API Call Error:', error);
          this.users = []; // Ensure users is always an array
          this.processUsersForDisplay();
          
          // Handle different types of errors
          let errorMessage = 'Unknown error occurred';
          if (error.status === 0) {
            errorMessage = 'Cannot connect to server. Please check if the backend is running.';
          } else if (error.status === 401) {
            errorMessage = 'Unauthorized access. Please check your permissions.';
          } else if (error.status === 404) {
            errorMessage = 'API endpoint not found.';
          } else if (error.message) {
            errorMessage = error.message;
          }
          
          this.handleError('Error loading users', errorMessage);
        },
        complete: () => {
          this.loading = false;
          this.tableConfig = { ...this.tableConfig, loading: false };
        }
      });
  }

  /**
   * Process users data for display in the table
   */
  private processUsersForDisplay(): void {
    // Ensure users is a valid array before processing
    if (!Array.isArray(this.users)) {
      console.warn('Users data is not an array, initializing empty array:', this.users);
      this.processedUsers = [];
      return;
    }

    try {
      this.processedUsers = this.users.map(user => ({
        ...user,
        rolesDisplay: this.getRolesDisplay(user.roles),
        statusDisplay: this.getStatusDisplay(user.isActive),
        emailVerifiedDisplay: this.getEmailVerifiedDisplay(user.emailConfirmed),
        lastLoginFormatted: user.lastLoginAt ? new Date(user.lastLoginAt).toLocaleDateString() : 'Never',
        createdAtFormatted: new Date(user.createdAt).toLocaleDateString()
      }));
    } catch (error) {
      console.error('Error processing users for display:', error);
      this.processedUsers = [];
      this.toastService.error('Error', 'Failed to process user data for display');
    }
  }

  /**
   * Format roles array for display
   * @param roles - Array of role strings
   */
  private getRolesDisplay(roles: string[]): string {
    if (!Array.isArray(roles) || roles.length === 0) {
      return 'No roles';
    }
    return roles.join(', ');
  }

  /**
   * Get status display string for table
   * @param isActive - User active status
   */
  private getStatusDisplay(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  /**
   * Get email verification status display string
   * @param emailConfirmed - Email verification status from backend
   */
  private getEmailVerifiedDisplay(emailConfirmed: boolean): string {
    return emailConfirmed ? 'Verified' : 'Unverified';
  }

  // Event handlers following the standard admin table pattern

  /**
   * Handle selection change events from table
   */
  onSelectionChange(selectedUsers: AdminUser[]): void {
    this.selectedUsers = selectedUsers;
    console.log('Selected users:', this.selectedUsers);
  }

  /**
   * Handle row select events from table
   */
  onRowSelect(event: any): void {
    const selectedUser = event.data as AdminUser;
    console.log('Row selected:', selectedUser);
    // Optional: Navigate to user details or perform other actions
  }

  /**
   * Handle export functionality
   */
  onExport(event: any): void {
    console.log('Export requested:', event);
    this.toastService.info('Export', 'User export functionality will be implemented');
  }

  // User management actions

  /**
   * Open dialog to create new user
   */
  createNewUser(): void {
    this.editingUser.set(null);
    this.dialogTitle.set('Create New User');
    this.showUserDialog.set(true);
  }

  /**
   * Open dialog to edit user
   */
  editUser(user: AdminUser): void {
    this.editingUser.set(user);
    this.dialogTitle.set('Edit User');
    this.showUserDialog.set(true);
  }

  /**
   * Close the user dialog
   */
  closeUserDialog(): void {
    this.showUserDialog.set(false);
    this.editingUser.set(null);
  }

  /**
   * Handle user form completion
   */
  onUserFormComplete(): void {
    this.closeUserDialog();
    this.loadUsers(); // Refresh the list
  }

  /**
   * Refresh the users list
   */
  refreshUsers(): void {
    this.loadUsers();
  }



  /**
   * Toggle user active status
   */
  toggleUserStatus(user: AdminUser): void {
    console.log('Toggling status for user:', user);
    const action = user.isActive ? 'deactivate' : 'activate';
    
    const serviceCall = user.isActive 
      ? this.adminService.deactivateUser(user.id)
      : this.adminService.activateUser(user.id);

    serviceCall.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.toastService.success('Success', `User ${action}d successfully`);
            this.loadUsers(); // Reload to get updated data
          } else {
            this.toastService.error('Error', `Failed to ${action} user`);
          }
        },
        error: (error) => {
          this.toastService.error('Error', `Failed to ${action} user: ${error.message}`);
        }
      });
  }

  /**
   * Reset user password
   */
  resetUserPassword(user: AdminUser): void {
    console.log('Resetting password for user:', user);
    // TODO: Implement password reset functionality
    this.toastService.info('Info', 'Password reset functionality will be implemented');
  }

  /**
   * Handle errors with consistent messaging
   */
  private handleError(title: string, message: string): void {
    console.error(`${title}: ${message}`);
    this.toastService.error(title, message);
    this.loading = false;
    this.tableConfig = { ...this.tableConfig, loading: false };
  }
}