/**
 * Role Form Component for Admin Dashboard
 * 
 * Handles both create and edit operations for roles using the reusable
 * app-dynamic-form component as mandated by the UI Components Guide.
 * 
 * Key Implementation Notes:
 * - Uses app-dynamic-form for consistent form handling
 * - Integrates app-tree-wrapper for permissions selection
 * - Supports both create mode (no ID) and edit mode (with ID)
 * - Follows reactive form pattern with proper validation
 * - Uses RolePermissionApiService for CRUD operations
 * 
 * This component demonstrates the proper usage of reusable UI components
 * as specified in the UI Components Guide documentation.
 */

import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';

// Reusable UI Components - MANDATORY per UI Components Guide
import { DynamicFormComponent, FormField, FormConfig } from '../../../../shared/dynamic-form/dynamic-form.component';
import { TreeWrapperComponent, TreeConfig } from '../../../../shared/tree-wrapper/tree-wrapper.component';

// API Service and Models
import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service';
import { RoleDto, PermissionDto, ApiResponse } from '../../../../shared/models/api.models';

// Toast Service for notifications
import { ToastService } from '../../../../shared/services/toast.service';

// PrimeNG Tree Node
import { TreeNode } from 'primeng/api';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [
    CommonModule,
    DynamicFormComponent,    // Using reusable dynamic form as required
    TreeWrapperComponent     // Using reusable tree wrapper for permissions
  ],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.scss'
})
export class RoleFormComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Inputs
  @Input() roleId: string | null = null;
  @Input() isEditMode: boolean = false;
  @Input() roleData: RoleDto | null = null;

  // Outputs
  @Output() onCancel = new EventEmitter<void>();
  @Output() onComplete = new EventEmitter<void>();

  // Component state signals
  isLoading = signal(false);
  isSaving = signal(false);
  
  // Form and data signals
  role = signal<RoleDto | null>(null);
  permissions = signal<PermissionDto[]>([]);
  selectedPermissions = signal<TreeNode[]>([]);
  
  // Computed properties
  pageTitle = computed(() => this.isEditMode ? 'Edit Role' : 'Create Role');
  submitButtonText = computed(() => this.isSaving() ? 'Saving...' : (this.isEditMode ? 'Update Role' : 'Create Role'));

  // Form configuration using app-dynamic-form
  formFields: FormField[] = [
    {
      key: 'name',
      type: 'text',
      label: 'Role Name',
      placeholder: 'Enter role name (e.g., Content Manager)',
      required: true,
      validation: {
        required: true,
        minLength: 2,
        maxLength: 100,
        pattern: '^[a-zA-Z0-9\\s\\-_]+$'
      },
      helpText: 'Unique name for this role. Only letters, numbers, spaces, hyphens, and underscores allowed.',
      colSpan: 6
    },
    {
      key: 'description',
      type: 'textarea',
      label: 'Description',
      placeholder: 'Describe the purpose and responsibilities of this role...',
      required: false,
      rows: 4,
      validation: {
        maxLength: 500
      },
      helpText: 'Optional description of the role\'s purpose and responsibilities (max 500 characters).',
      colSpan: 6
    },
    {
      key: 'isActive',
      type: 'toggle',
      label: 'Active Status',
      toggleLabel: 'Role is active and can be assigned to users',
      defaultValue: true,
      helpText: 'Toggle to enable/disable this role. Inactive roles cannot be assigned to users.',
      colSpan: 6
    },
    {
      key: 'systemRole',
      type: 'select',
      label: 'System Role Type',
      required: true,
      options: [
        { label: 'User Role (Custom)', value: 0 },
        { label: 'Admin Role (System)', value: 1 },
        { label: 'Super Admin (System)', value: 2 },
        { label: 'Moderator (System)', value: 3 },
        { label: 'Content Manager (System)', value: 4 },
        { label: 'Finance (System)', value: 5 },
        { label: 'Accounts (System)', value: 6 }
      ],
      defaultValue: 0,
      helpText: 'Select the type of role. System roles have predefined permissions.',
      colSpan: 6
    }
  ];

  formConfig: FormConfig = {
    title: '',  // Will be set dynamically
    description: 'Configure role details and assign permissions below.',
    layout: 'grid',
    columns: 2,
    showSubmitButton: true,
    showResetButton: false,
    submitButtonText: 'Create Role',
    validationMode: 'onChange',
    actionButtonAlignment: 'right'
  };

  // Tree configuration for permissions selection
  permissionsTreeConfig: TreeConfig = {
    selectionMode: 'checkbox',
    showHeader: true,
    showCounts: true,
    showControls: true,
    headerTitle: 'Role Permissions',
    expandAll: false,
    minimal: false
  };

  permissionsTreeData: TreeNode[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private rolePermissionApiService: RolePermissionApiService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeComponent(): void {
    // Set form config based on edit mode
    if (this.isEditMode && this.roleId) {
      this.formConfig.title = 'Edit Role';
      this.formConfig.submitButtonText = 'Update Role';
    } else {
      this.formConfig.title = 'Create New Role';
      this.formConfig.submitButtonText = 'Create Role';
    }

    this.loadInitialData();
  }

  private loadInitialData(): void {
    this.isLoading.set(true);

    // Always load permissions first
    this.rolePermissionApiService.getPermissions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (permissionsResponse) => {
          console.log('Permissions API response:', permissionsResponse);
          
          if (permissionsResponse.success && permissionsResponse.data) {
            console.log('Permissions data type:', typeof permissionsResponse.data);
            console.log('Permissions data structure:', permissionsResponse.data);
            
            // Ensure we have an array
            if (Array.isArray(permissionsResponse.data)) {
              this.permissions.set(permissionsResponse.data);
              this.buildPermissionsTree(permissionsResponse.data);
            } else {
              console.error('Expected permissions to be an array, got:', permissionsResponse.data);
              this.permissions.set([]);
              this.buildPermissionsTree([]);
            }
          } else {
            console.warn('No permissions data received or response not successful');
            this.permissions.set([]);
            this.buildPermissionsTree([]);
          }

          // If in edit mode, load the role data
          if (this.isEditMode && this.roleId) {
            this.loadRoleData();
          } else {
            this.isLoading.set(false);
          }
        },
        error: (error) => {
          console.error('Error loading permissions:', error);
          console.warn('Using mock permissions data for development');
          
          // Use mock data when API is not available
          const mockPermissions: PermissionDto[] = [
            {
              id: '1',
              permissionKey: 'users.view',
              displayName: 'View Users',
              description: 'Can view user list and details',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'User Management'
            },
            {
              id: '2',
              permissionKey: 'users.create',
              displayName: 'Create Users',
              description: 'Can create new users',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'User Management'
            },
            {
              id: '3',
              permissionKey: 'users.edit',
              displayName: 'Edit Users',
              description: 'Can edit user information',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'User Management'
            },
            {
              id: '4',
              permissionKey: 'users.delete',
              displayName: 'Delete Users',
              description: 'Can delete users',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'User Management'
            },
            {
              id: '5',
              permissionKey: 'roles.view',
              displayName: 'View Roles',
              description: 'Can view roles and permissions',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'Role Management'
            },
            {
              id: '6',
              permissionKey: 'roles.create',
              displayName: 'Create Roles',
              description: 'Can create new roles',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'Role Management'
            },
            {
              id: '7',
              permissionKey: 'roles.edit',
              displayName: 'Edit Roles',
              description: 'Can edit role information',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'Role Management'
            },
            {
              id: '8',
              permissionKey: 'system.view',
              displayName: 'View System',
              description: 'Can view system settings',
              permissionType: 1,
              isSystemPermission: true,
              menuName: 'System'
            }
          ];
          
          this.permissions.set(mockPermissions);
          this.buildPermissionsTree(mockPermissions);
          this.isLoading.set(false);
          
          this.handleError('Error loading permissions (using mock data)', error.message);
        }
      });
  }

  private loadRoleData(): void {
    // If role data is passed directly from parent, use it
    if (this.roleData) {
      console.log('Using role data from parent:', this.roleData);
      this.role.set(this.roleData);
      this.populateFormWithRoleData(this.roleData);
      this.selectRolePermissions(this.roleData.permissions || []);
      this.isLoading.set(false);
      return;
    }

    // Fallback: try to load from API if roleId is provided
    if (!this.roleId) {
      this.isLoading.set(false);
      return;
    }
    
    this.rolePermissionApiService.getRoleById(this.roleId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (roleResponse) => {
          if (roleResponse.success && roleResponse.data) {
            const role = roleResponse.data.role;
            this.role.set(role);
            this.populateFormWithRoleData(role);
            this.selectRolePermissions(role.permissions || []);
          } else {
            this.handleError('Failed to load role', roleResponse.message);
          }
        },
        error: (error) => {
          console.error('Error loading role from API:', error);
          // Create mock data for development if API fails
          if (this.roleId) {
            const mockRole: RoleDto = {
              id: this.roleId,
              name: 'Sample Role',
              description: 'Sample role for testing',
              isActive: true,
              systemRole: 0,
              permissions: []
            };
            this.role.set(mockRole);
            this.populateFormWithRoleData(mockRole);
          }
          this.handleError('Error loading role (using mock data)', error.message);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
  }

  /**
   * Populate form fields with role data for editing
   */
  private populateFormWithRoleData(roleData: RoleDto): void {
    // Update form field values with role data
    this.formFields = this.formFields.map(field => {
      switch (field.key) {
        case 'name':
          return { ...field, value: roleData.name };
        case 'description':
          return { ...field, value: roleData.description || '' };
        case 'isActive':
          return { ...field, value: roleData.isActive };
        case 'systemRole':
          return { ...field, value: roleData.systemRole };
        default:
          return field;
      }
    });

    console.log('Form populated with role data:', this.formFields);
  }

  private buildPermissionsTree(permissions: PermissionDto[]): void {
    // Add type checking to ensure permissions is an array
    if (!Array.isArray(permissions)) {
      console.warn('Permissions is not an array:', permissions);
      this.permissionsTreeData = [];
      return;
    }

    // Group permissions by menu/category for better tree structure
    const groupedPermissions = this.groupPermissionsByMenu(permissions);
    
    this.permissionsTreeData = Object.keys(groupedPermissions).map(menuName => ({
      key: `menu-${menuName}`,
      label: menuName || 'General Permissions',
      data: { type: 'menu', name: menuName },
      children: groupedPermissions[menuName].map(permission => ({
        key: permission.id,
        label: permission.displayName,
        data: { type: 'permission', permission: permission }
      }))
    }));
  }

  private groupPermissionsByMenu(permissions: PermissionDto[]): { [key: string]: PermissionDto[] } {
    return permissions.reduce((groups, permission) => {
      const menuName = permission.menuName || 'General';
      if (!groups[menuName]) {
        groups[menuName] = [];
      }
      groups[menuName].push(permission);
      return groups;
    }, {} as { [key: string]: PermissionDto[] });
  }

  private selectRolePermissions(rolePermissions: PermissionDto[]): void {
    const selectedNodes: TreeNode[] = [];
    
    rolePermissions.forEach(permission => {
      const node = this.findPermissionNode(permission.id);
      if (node) {
        selectedNodes.push(node);
      }
    });
    
    this.selectedPermissions.set(selectedNodes);
  }

  private findPermissionNode(permissionId: string): TreeNode | null {
    for (const menuNode of this.permissionsTreeData) {
      if (menuNode.children) {
        for (const permissionNode of menuNode.children) {
          if (permissionNode.key === permissionId) {
            return permissionNode;
          }
        }
      }
    }
    return null;
  }

  // Event handlers
  onFormSubmit(formData: any): void {
    if (this.isSaving()) return;

    const selectedPermissionIds = this.selectedPermissions()
      .filter(node => node.data?.type === 'permission')
      .map(node => node.data.permission.id);

    const roleRequest = {
      name: formData.name,
      description: formData.description || '',
      isActive: formData.isActive ?? true,
      systemRole: formData.systemRole ?? 0,
      permissionIds: selectedPermissionIds
    };

    console.log('Submitting role with data:', roleRequest);
    this.isSaving.set(true);

    const operation = this.isEditMode && this.roleId
      ? this.rolePermissionApiService.updateRole(this.roleId, roleRequest)
      : this.rolePermissionApiService.createRole(roleRequest);

    operation
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            const action = this.isEditMode ? 'updated' : 'created';
            this.toastService.success('Success', response.data.message || `Role "${roleRequest.name}" has been ${action} successfully`);
            this.onComplete.emit();
          } else {
            this.handleError(`Failed to ${this.isEditMode ? 'update' : 'create'} role`, response.message || 'Unknown error');
          }
        },
        error: (error) => {
          console.error('Error submitting role:', error);
          this.handleError(`Error ${this.isEditMode ? 'updating' : 'creating'} role`, error.message || 'Network error');
        },
        complete: () => {
          this.isSaving.set(false);
        }
      });
  }

  onPermissionSelectionChange(selectedNodes: TreeNode[]): void {
    this.selectedPermissions.set(selectedNodes);
  }

  onCancelClick(): void {
    this.onCancel.emit();
  }

  private handleError(title: string, message?: string): void {
    this.isLoading.set(false);
    this.isSaving.set(false);
    this.toastService.error(title, message || 'Please try again later');
  }

  // Getters for template
  get formInitialData(): any {
    const roleData = this.role();
    if (!roleData) return {};

    return {
      name: roleData.name,
      description: roleData.description,
      isActive: roleData.isActive,
      systemRole: roleData.systemRole
    };
  }

  get isFormDisabled(): boolean {
    return this.isLoading() || this.isSaving();
  }
}