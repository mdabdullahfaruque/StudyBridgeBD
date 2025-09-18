/**
 * Role List Component for Admin Dashboard
 * 
 * Reference implementation for admin table components using the standardized
 * app-table-wrapper component as mandated by the UI Components Guide.
 * 
 * Key Implementation Notes:
 * - Backend returns nested API response: response.data.roles (not direct array)
 * - SystemRole field comes as boolean (true=system, false=custom) not number
 * - Uses 'status' column type for styled badges in table display
 * - Processes raw data into display-friendly format before table binding
 * 
 * This component serves as the reference implementation for all admin table components.
 * See docs/ADMIN_TABLE_IMPLEMENTATION.md for the complete pattern guide.
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// PrimeNG Table Wrapper - MANDATORY per UI Components Guide
import { TableWrapperComponent, TableColumn, TableConfig } from '../../../../shared/table-wrapper/table-wrapper.component';

// API Service and Models
import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service';
import { RoleDto, ApiResponse } from '../../../../shared/models/api.models';

// Toast Service for notifications
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    TableWrapperComponent  // Using existing PrimeNG table wrapper as required
  ],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.scss'
})
export class RoleListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Table data and configuration
  roles: RoleDto[] = [];
  processedRoles: any[] = [];
  selectedRoles: RoleDto[] = [];
  loading = false;

  // Table column configuration per UI Components Guide
  columns: TableColumn[] = [
    {
      field: 'name',
      header: 'Role Name',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '200px'
    },
    {
      field: 'description',
      header: 'Description',
      type: 'text',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '300px'
    },
    {
      field: 'systemRoleDisplay',
      header: 'System Role',
      type: 'status',
      sortable: true,
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'System Role', value: 'System Role' },
        { label: 'Custom Role', value: 'Custom Role' }
      ],
      width: '150px'
    },
    {
      field: 'statusDisplay',
      header: 'Status',
      type: 'status',
      sortable: true,
      filterable: true,
      filterType: 'dropdown',
      filterOptions: [
        { label: 'Active', value: 'Active' },
        { label: 'Inactive', value: 'Inactive' }
      ],
      width: '120px'
    },
    {
      field: 'permissionsCount',
      header: 'Permissions Count',
      type: 'number',
      sortable: true,
      width: '150px'
    }
  ];

  // Table configuration per UI Components Guide
  tableConfig: TableConfig = {
    serverSide: false,           // Client-side for now, can be changed to server-side later
    loading: false,
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [5, 10, 25, 50],
    selectionMode: 'multiple',
    sortMode: 'single',
    globalFilterFields: ['name', 'description'],
    filterMode: 'lenient',
    resizableColumns: true,
    reorderableColumns: true,
    exportable: true,
    responsive: true,
    striped: true,
    size: 'normal',
    showGridlines: true,
    showHeader: true
  };

  constructor(
    private rolePermissionApiService: RolePermissionApiService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.processedRoles = []; // Initialize empty array
    this.loadRoles();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Load roles from backend API
   */
  private loadRoles(): void {
    this.loading = true;
    this.tableConfig = { ...this.tableConfig, loading: true };

    this.rolePermissionApiService.getRoles()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success && response.data) {
            // Handle nested structure where roles are in response.data.roles
            if (response.data.roles && Array.isArray(response.data.roles)) {
              this.roles = response.data.roles;
            } else if (Array.isArray(response.data)) {
              this.roles = response.data;
            } else {
              console.error('Unexpected data structure:', response.data);
              this.handleError('Failed to load roles', 'Unexpected data structure received from API');
              return;
            }
            
            this.processRolesForDisplay();
            this.toastService.success('Success', `Loaded ${this.roles.length} roles`);
          } else {
            this.handleError('Failed to load roles', response.message || 'Unknown error from API');
          }
        },
        error: (error) => {
          this.handleError('Error loading roles', error.message || 'Unknown error occurred');
        },
        complete: () => {
          this.loading = false;
          this.tableConfig = { ...this.tableConfig, loading: false };
        }
      });
  }

  /**
   * Process roles data for display in the table
   */
  private processRolesForDisplay(): void {
    // Ensure roles is a valid array before processing
    if (!Array.isArray(this.roles)) {
      console.warn('Roles data is not an array, initializing empty array:', this.roles);
      this.processedRoles = [];
      return;
    }

    this.processedRoles = this.roles.map(role => ({
      ...role,
      systemRoleDisplay: this.getSystemRoleDisplay(role.systemRole),
      statusDisplay: this.getStatusDisplay(role.isActive),
      permissionsCount: this.getPermissionsCount(role)
    }));
  }

  /**
   * Handle API errors with user feedback
   */
  private handleError(title: string, message?: string): void {
    this.loading = false;
    this.tableConfig = { ...this.tableConfig, loading: false };
    
    this.toastService.error(title, message || 'Please try again later');
  }

  /**
   * Handle table selection changes
   */
  onSelectionChange(selectedRoles: RoleDto[]): void {
    this.selectedRoles = selectedRoles;
  }

  /**
   * Handle row selection events
   */
  onRowSelect(event: any): void {
    const selectedRole = event.data as RoleDto;
    console.log('Selected role:', selectedRole);
  }

  /**
   * Handle table export events
   */
  onExport(format: string): void {
    this.toastService.info('Export', `Exporting roles in ${format.toUpperCase()} format`);
  }

  /**
   * Refresh roles data
   */
  refreshRoles(): void {
    this.loadRoles();
  }

  /**
   * Navigate to create new role
   */
  createNewRole(): void {
    this.router.navigate(['/admin/roles/create']);
  }

  /**
   * Navigate to edit role
   */
  editRole(role: RoleDto): void {
    this.router.navigate(['/admin/roles/edit', role.id]);
  }

  /**
   * Delete selected role
   */
  deleteRole(role: RoleDto): void {
    if (confirm(`Are you sure you want to delete the role "${role.name}"?`)) {
      this.rolePermissionApiService.deleteRole(Number(role.id))
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.toastService.success('Success', `Role "${role.name}" has been deleted`);
              this.loadRoles(); // Reload the list
            } else {
              this.handleError('Failed to delete role', response.message);
            }
          },
          error: (error) => {
            this.handleError('Error deleting role', error.message);
          }
        });
    }
  }

  /**
   * Get permissions count for display
   */
  getPermissionsCount(role: any): number {
    // Handle both 'permissions' and 'Permissions' property names
    const permissions = role.permissions || role.Permissions || [];
    return Array.isArray(permissions) ? permissions.length : 0;
  }

  /**
   * Get formatted system role display
   */
  getSystemRoleDisplay(systemRole: any): string {
    // Handle boolean from backend (where true = system role, false = custom role)
    if (typeof systemRole === 'boolean') {
      return systemRole ? 'System Role' : 'Custom Role';
    }
    
    // Handle legacy number format
    const systemRoleMap: { [key: number]: string } = {
      0: 'User',
      1: 'Admin', 
      2: 'SuperAdmin',
      3: 'Moderator',
      4: 'ContentManager',
      5: 'Finance',
      6: 'Accounts'
    };
    
    return systemRoleMap[systemRole] || `Role ${systemRole}`;
  }

  /**
   * Get status display with proper styling
   */
  getStatusDisplay(isActive: boolean): string {
    return isActive ? 'Active' : 'Inactive';
  }

  /**
   * Get status CSS class for styling
   */
  getStatusClass(isActive: boolean): string {
    return isActive ? 'status-active' : 'status-inactive';
  }
}