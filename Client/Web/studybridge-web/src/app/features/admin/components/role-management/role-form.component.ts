import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal, computed } from '@angular/core';import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal, computed } from '@angular/core';/**/**

import { CommonModule } from '@angular/common';

import { Subject, takeUntil } from 'rxjs';import { CommonModule } from '@angular/common';



import { DynamicFormComponent, FormField, FormConfig } from '../../../../shared/dynamic-form/dynamic-form.component';import { Subject, takeUntil } from 'rxjs'; * Role Form Component for Admin Dashboard * Role Form Component for Admin Dashboard

import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service';

import { RoleDto, PermissionDto, ApiResponse } from '../../../../shared/models/api.models';

import { ToastService } from '../../../../shared/services/toast.service';

import { TreeNode } from 'primeng/api';import { DynamicFormComponent, FormField, FormConfig } from '../../../../shared/dynamic-form/dynamic-form.component'; *  * 



@Component({import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service';

  selector: 'app-role-form',

  standalone: true,import { RoleDto, PermissionDto, ApiResponse } from '../../../../shared/models/api.models'; * Handles both create and edit operations for roles using the reusable * Handles both create and edit operations for roles using the reusable

  imports: [

    CommonModule,import { ToastService } from '../../../../shared/services/toast.service';

    DynamicFormComponent

  ],import { TreeNode } from 'primeng/api'; * app-dynamic-form component as mandated by the UI Components Guide. * app-dynamic-form component as mandated by the UI Components Guide.

  templateUrl: './role-form.component.html',

  styleUrl: './role-form.component.scss'

})

export class RoleFormComponent implements OnInit, OnDestroy {@Component({ */ * 

  private destroy$ = new Subject<void>();

  selector: 'app-role-form',

  @Input() roleId: string | null = null;

  @Input() isEditMode: boolean = false;  standalone: true, * Key Implementation Notes:

  @Input() roleData: RoleDto | null = null;

  imports: [

  @Output() onCancel = new EventEmitter<void>();

  @Output() onComplete = new EventEmitter<void>();    CommonModule,import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal, computed } from '@angular/core'; * - Uses app-dynamic-form for consistent form handling



  isLoading = signal(false);    DynamicFormComponent

  isSaving = signal(false);

    ],import { CommonModule } from '@angular/common'; * - Integrates app-tree-wrapper for permissions selection

  role = signal<RoleDto | null>(null);

  permissions = signal<PermissionDto[]>([]);  templateUrl: './role-form.component.html',

  selectedPermissions = signal<TreeNode[]>([]);

  formData = signal<any>({});  styleUrl: './role-form.component.scss'import { Subject, takeUntil } from 'rxjs'; * - Supports both create mode (no ID) and edit mode (with ID)

  

  pageTitle = computed(() => this.isEditMode ? 'Edit Role' : 'Create Role');})

  submitButtonText = computed(() => this.isSaving() ? 'Saving...' : (this.isEditMode ? 'Update Role' : 'Create Role'));

export class RoleFormComponent implements OnInit, OnDestroy { * - Follows reactive form pattern with proper validation

  formFields: FormField[] = [

    {  private destroy$ = new Subject<void>();

      key: 'name',

      type: 'text',// Reusable UI Components - MANDATORY per UI Components Guide * - Uses RolePermissionApiService for CRUD operations

      label: 'Role Name',

      placeholder: 'Enter role name (e.g., Content Manager)',  @Input() roleId: string | null = null;

      required: true,

      validation: {  @Input() isEditMode: boolean = false;import { DynamicFormComponent, FormField, FormConfig } from '../../../../shared/dynamic-form/dynamic-form.component'; * 

        required: true,

        minLength: 2,  @Input() roleData: RoleDto | null = null;

        maxLength: 100,

        pattern: '^[a-zA-Z0-9\\s\\-_]+$' * This component demonstrates the proper usage of reusable UI components

      },

      helpText: 'Unique name for this role. Only letters, numbers, spaces, hyphens, and underscores allowed.',  @Output() onCancel = new EventEmitter<void>();

      colSpan: 6

    },  @Output() onComplete = new EventEmitter<void>();// API Service and Models * as specified in the UI Components Guide documentation.

    {

      key: 'description',

      type: 'textarea',

      label: 'Description',  isLoading = signal(false);import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service'; */

      placeholder: 'Describe the purpose and responsibilities of this role...',

      required: false,  isSaving = signal(false);

      rows: 4,

      validation: {  import { RoleDto, PermissionDto, ApiResponse } from '../../../../shared/models/api.models';

        maxLength: 500

      },  role = signal<RoleDto | null>(null);

      helpText: 'Optional description of the role\'s purpose and responsibilities (max 500 characters).',

      colSpan: 6  permissions = signal<PermissionDto[]>([]);import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, signal, computed } from '@angular/core';

    },

    {  selectedPermissions = signal<TreeNode[]>([]);

      key: 'isActive',

      type: 'toggle',  formData = signal<any>({});// Toast Service for notificationsimport { CommonModule } from '@angular/common';

      label: 'Active Status',

      toggleLabel: 'Role is active and can be assigned to users',  

      defaultValue: true,

      helpText: 'Toggle to enable/disable this role. Inactive roles cannot be assigned to users.',  pageTitle = computed(() => this.isEditMode ? 'Edit Role' : 'Create Role');import { ToastService } from '../../../../shared/services/toast.service';import { Subject, takeUntil } from 'rxjs';

      colSpan: 12

    }  submitButtonText = computed(() => this.isSaving() ? 'Saving...' : (this.isEditMode ? 'Update Role' : 'Create Role'));

  ];



  formConfig: FormConfig = {

    title: '',  formFields: FormField[] = [

    description: 'Configure role details and assign permissions below.',

    layout: 'grid',    {// PrimeNG Tree Node// Reusable UI Components - MANDATORY per UI Components Guide

    columns: 2,

    showSubmitButton: true,      key: 'name',

    showResetButton: false,

    submitButtonText: 'Create Role',      type: 'text',import { TreeNode } from 'primeng/api';import { DynamicFormComponent, FormField, FormConfig } from '../../../../shared/dynamic-form/dynamic-form.component';

    validationMode: 'onChange',

    actionButtonAlignment: 'right'      label: 'Role Name',

  };

      placeholder: 'Enter role name (e.g., Content Manager)',

  permissionsTreeData: TreeNode[] = [];

      required: true,

  constructor(

    private rolePermissionApiService: RolePermissionApiService,      validation: {@Component({// API Service and Models

    private toastService: ToastService

  ) {}        required: true,



  ngOnInit(): void {        minLength: 2,  selector: 'app-role-form',import { RolePermissionApiService } from '../../../../shared/services/role-permission-api.service';

    this.initializeComponent();

  }        maxLength: 100,



  ngOnDestroy(): void {        pattern: '^[a-zA-Z0-9\\s\\-_]+$'  standalone: true,import { RoleDto, PermissionDto, ApiResponse } from '../../../../shared/models/api.models';

    this.destroy$.next();

    this.destroy$.complete();      },

  }

      helpText: 'Unique name for this role. Only letters, numbers, spaces, hyphens, and underscores allowed.',  imports: [

  private initializeComponent(): void {

    if (this.isEditMode && this.roleId) {      colSpan: 6

      this.formConfig.title = 'Edit Role';

      this.formConfig.submitButtonText = 'Update Role';    },    CommonModule,// Toast Service for notifications

    } else {

      this.formConfig.title = 'Create New Role';    {

      this.formConfig.submitButtonText = 'Create Role';

    }      key: 'description',    DynamicFormComponentimport { ToastService } from '../../../../shared/services/toast.service';



    this.loadInitialData();      type: 'textarea',

  }

      label: 'Description',  ],

  private loadInitialData(): void {

    this.isLoading.set(true);      placeholder: 'Describe the purpose and responsibilities of this role...',



    this.rolePermissionApiService.getPermissions()      required: false,  templateUrl: './role-form.component.html',// PrimeNG Tree Node

      .pipe(takeUntil(this.destroy$))

      .subscribe({      rows: 4,

        next: (permissionsResponse) => {

          if (permissionsResponse.success && permissionsResponse.data) {      validation: {  styleUrl: './role-form.component.scss'import { TreeNode } from 'primeng/api';

            if (Array.isArray(permissionsResponse.data)) {

              this.permissions.set(permissionsResponse.data);        maxLength: 500

              this.buildPermissionsTree(permissionsResponse.data);

            } else {      },})

              this.permissions.set([]);

              this.buildPermissionsTree([]);      helpText: 'Optional description of the role\'s purpose and responsibilities (max 500 characters).',

            }

          } else {      colSpan: 6export class RoleFormComponent implements OnInit, OnDestroy {@Component({

            this.permissions.set([]);

            this.buildPermissionsTree([]);    },

          }

    {  private destroy$ = new Subject<void>();  selector: 'app-role-form',

          if (this.isEditMode && this.roleId) {

            this.loadRoleData();      key: 'isActive',

          } else {

            this.formData.set({      type: 'toggle',  standalone: true,

              name: '',

              description: '',      label: 'Active Status',

              isActive: true

            });      toggleLabel: 'Role is active and can be assigned to users',  // Inputs  imports: [

            this.isLoading.set(false);

          }      defaultValue: true,

        },

        error: (error) => {      helpText: 'Toggle to enable/disable this role. Inactive roles cannot be assigned to users.',  @Input() roleId: string | null = null;    CommonModule,

          console.error('Error loading permissions:', error);

                colSpan: 12

          const mockPermissions: PermissionDto[] = [

            {    }  @Input() isEditMode: boolean = false;    DynamicFormComponent    // Using reusable dynamic form as required

              id: '1',

              permissionKey: 'users.view',  ];

              displayName: 'View Users',

              description: 'Can view user list and details',  @Input() roleData: RoleDto | null = null;  ],

              permissionType: 1,

              isSystemPermission: true,  formConfig: FormConfig = {

              menuName: 'User Management'

            }    title: '',  templateUrl: './role-form.component.html',

          ];

              description: 'Configure role details and assign permissions below.',

          this.permissions.set(mockPermissions);

          this.buildPermissionsTree(mockPermissions);    layout: 'grid',  // Outputs  styleUrl: './role-form.component.scss'

          

          if (!this.isEditMode) {    columns: 2,

            this.formData.set({

              name: '',    showSubmitButton: true,  @Output() onCancel = new EventEmitter<void>();})

              description: '',

              isActive: true    showResetButton: false,

            });

          }    submitButtonText: 'Create Role',  @Output() onComplete = new EventEmitter<void>();export class RoleFormComponent implements OnInit, OnDestroy {

          

          this.isLoading.set(false);    validationMode: 'onChange',

          this.handleError('Error loading permissions (using mock data)', error.message);

        }    actionButtonAlignment: 'right'  private destroy$ = new Subject<void>();

      });

  }  };



  private loadRoleData(): void {  // Component state signals

    if (this.roleData) {

      this.role.set(this.roleData);  permissionsTreeData: TreeNode[] = [];

      this.populateFormWithRoleData(this.roleData);

      this.selectRolePermissions(this.roleData.permissions || []);  isLoading = signal(false);  // Inputs

      this.isLoading.set(false);

      return;  constructor(

    }

    private rolePermissionApiService: RolePermissionApiService,  isSaving = signal(false);  @Input() roleId: string | null = null;

    if (!this.roleId) {

      this.isLoading.set(false);    private toastService: ToastService

      return;

    }  ) {}    @Input() isEditMode: boolean = false;

    

    this.rolePermissionApiService.getRoleById(this.roleId)

      .pipe(takeUntil(this.destroy$))

      .subscribe({  ngOnInit(): void {  // Form and data signals  @Input() roleData: RoleDto | null = null;

        next: (roleResponse) => {

          if (roleResponse.success && roleResponse.data) {    this.initializeComponent();

            const role = roleResponse.data.role;

            this.role.set(role);  }  role = signal<RoleDto | null>(null);

            this.populateFormWithRoleData(role);

            this.selectRolePermissions(role.permissions || []);

          } else {

            this.handleError('Failed to load role', roleResponse.message);  ngOnDestroy(): void {  permissions = signal<PermissionDto[]>([]);  // Outputs

          }

        },    this.destroy$.next();

        error: (error) => {

          console.error('Error loading role from API:', error);    this.destroy$.complete();  selectedPermissions = signal<TreeNode[]>([]);  @Output() onCancel = new EventEmitter<void>();

          if (this.roleId) {

            const mockRole: RoleDto = {  }

              id: this.roleId,

              name: 'Sample Role',  formData = signal<any>({});  @Output() onComplete = new EventEmitter<void>();

              description: 'Sample role for testing',

              isActive: true,  private initializeComponent(): void {

              permissions: []

            };    if (this.isEditMode && this.roleId) {  

            this.role.set(mockRole);

            this.populateFormWithRoleData(mockRole);      this.formConfig.title = 'Edit Role';

          }

          this.handleError('Error loading role (using mock data)', error.message);      this.formConfig.submitButtonText = 'Update Role';  // Computed properties  // Component state signals

        },

        complete: () => {    } else {

          this.isLoading.set(false);

        }      this.formConfig.title = 'Create New Role';  pageTitle = computed(() => this.isEditMode ? 'Edit Role' : 'Create Role');  isLoading = signal(false);

      });

  }      this.formConfig.submitButtonText = 'Create Role';



  private populateFormWithRoleData(roleData: RoleDto): void {    }  submitButtonText = computed(() => this.isSaving() ? 'Saving...' : (this.isEditMode ? 'Update Role' : 'Create Role'));  isSaving = signal(false);

    const initialData = {

      name: roleData.name,

      description: roleData.description || '',

      isActive: roleData.isActive    this.loadInitialData();  

    };

      }

    this.formData.set(initialData);

  }  // Form configuration using app-dynamic-form  // Form and data signals



  private buildPermissionsTree(permissions: PermissionDto[]): void {  private loadInitialData(): void {

    if (!Array.isArray(permissions)) {

      this.permissionsTreeData = [];    this.isLoading.set(true);  formFields: FormField[] = [  role = signal<RoleDto | null>(null);

      return;

    }



    const groupedPermissions = this.groupPermissionsByMenu(permissions);    this.rolePermissionApiService.getPermissions()    {  permissions = signal<PermissionDto[]>([]);

    

    this.permissionsTreeData = Object.keys(groupedPermissions).map(menuName => ({      .pipe(takeUntil(this.destroy$))

      key: `menu-${menuName}`,

      label: menuName || 'General Permissions',      .subscribe({      key: 'name',  selectedPermissions = signal<TreeNode[]>([]);

      data: { type: 'menu', name: menuName },

      children: groupedPermissions[menuName].map(permission => ({        next: (permissionsResponse) => {

        key: permission.id,

        label: permission.displayName,          if (permissionsResponse.success && permissionsResponse.data) {      type: 'text',  formData = signal<any>({});

        data: { type: 'permission', permission: permission }

      }))            if (Array.isArray(permissionsResponse.data)) {

    }));

  }              this.permissions.set(permissionsResponse.data);      label: 'Role Name',  



  private groupPermissionsByMenu(permissions: PermissionDto[]): { [key: string]: PermissionDto[] } {              this.buildPermissionsTree(permissionsResponse.data);

    return permissions.reduce((groups, permission) => {

      const menuName = permission.menuName || 'General';            } else {      placeholder: 'Enter role name (e.g., Content Manager)',  // Computed properties

      if (!groups[menuName]) {

        groups[menuName] = [];              this.permissions.set([]);

      }

      groups[menuName].push(permission);              this.buildPermissionsTree([]);      required: true,  pageTitle = computed(() => this.isEditMode ? 'Edit Role' : 'Create Role');

      return groups;

    }, {} as { [key: string]: PermissionDto[] });            }

  }

          } else {      validation: {  submitButtonText = computed(() => this.isSaving() ? 'Saving...' : (this.isEditMode ? 'Update Role' : 'Create Role'));

  private selectRolePermissions(rolePermissions: PermissionDto[]): void {

    const selectedNodes: TreeNode[] = [];            this.permissions.set([]);

    

    rolePermissions.forEach(permission => {            this.buildPermissionsTree([]);        required: true,

      const node = this.findPermissionNode(permission.id);

      if (node) {          }

        selectedNodes.push(node);

      }        minLength: 2,  // Form configuration using app-dynamic-form

    });

              if (this.isEditMode && this.roleId) {

    this.selectedPermissions.set(selectedNodes);

  }            this.loadRoleData();        maxLength: 100,  formFields: FormField[] = [



  private findPermissionNode(permissionId: string): TreeNode | null {          } else {

    for (const menuNode of this.permissionsTreeData) {

      if (menuNode.children) {            this.formData.set({        pattern: '^[a-zA-Z0-9\\s\\-_]+$'    {

        for (const permissionNode of menuNode.children) {

          if (permissionNode.key === permissionId) {              name: '',

            return permissionNode;

          }              description: '',      },      key: 'name',

        }

      }              isActive: true

    }

    return null;            });      helpText: 'Unique name for this role. Only letters, numbers, spaces, hyphens, and underscores allowed.',      type: 'text',

  }

            this.isLoading.set(false);

  onFormSubmit(formData: any): void {

    if (this.isSaving()) return;          }      colSpan: 6      label: 'Role Name',



    const selectedPermissionIds = this.selectedPermissions()        },

      .filter(node => node.data?.type === 'permission')

      .map(node => node.data.permission.id);        error: (error) => {    },      placeholder: 'Enter role name (e.g., Content Manager)',



    const roleRequest = {          console.error('Error loading permissions:', error);

      name: formData.name,

      description: formData.description || '',              {      required: true,

      isActive: formData.isActive ?? true,

      permissionIds: selectedPermissionIds          const mockPermissions: PermissionDto[] = [

    };

            {      key: 'description',      validation: {

    this.isSaving.set(true);

              id: '1',

    const operation = this.isEditMode && this.roleId

      ? this.rolePermissionApiService.updateRole(this.roleId, roleRequest)              permissionKey: 'users.view',      type: 'textarea',        required: true,

      : this.rolePermissionApiService.createRole(roleRequest);

              displayName: 'View Users',

    operation

      .pipe(takeUntil(this.destroy$))              description: 'Can view user list and details',      label: 'Description',        minLength: 2,

      .subscribe({

        next: (response) => {              permissionType: 1,

          if (response.success && response.data) {

            const action = this.isEditMode ? 'updated' : 'created';              isSystemPermission: true,      placeholder: 'Describe the purpose and responsibilities of this role...',        maxLength: 100,

            this.toastService.success('Success', response.data.message || `Role "${roleRequest.name}" has been ${action} successfully`);

            this.onComplete.emit();              menuName: 'User Management'

          } else {

            this.handleError(`Failed to ${this.isEditMode ? 'update' : 'create'} role`, response.message || 'Unknown error');            }      required: false,        pattern: '^[a-zA-Z0-9\\s\\-_]+$'

          }

        },          ];

        error: (error) => {

          this.handleError(`Error ${this.isEditMode ? 'updating' : 'creating'} role`, error.message || 'Network error');                rows: 4,      },

        },

        complete: () => {          this.permissions.set(mockPermissions);

          this.isSaving.set(false);

        }          this.buildPermissionsTree(mockPermissions);      validation: {      helpText: 'Unique name for this role. Only letters, numbers, spaces, hyphens, and underscores allowed.',

      });

  }          



  onPermissionSelectionChange(selectedNodes: TreeNode[]): void {          if (!this.isEditMode) {        maxLength: 500      colSpan: 6

    this.selectedPermissions.set(selectedNodes);

  }            this.formData.set({



  onCancelClick(): void {              name: '',      },    },

    this.onCancel.emit();

  }              description: '',



  private handleError(title: string, message?: string): void {              isActive: true      helpText: 'Optional description of the role\'s purpose and responsibilities (max 500 characters).',    {

    this.isLoading.set(false);

    this.isSaving.set(false);            });

    this.toastService.error(title, message || 'Please try again later');

  }          }      colSpan: 6      key: 'description',



  get formInitialData(): any {          

    return this.formData();

  }          this.isLoading.set(false);    },      type: 'textarea',



  get isFormDisabled(): boolean {          this.handleError('Error loading permissions (using mock data)', error.message);

    return this.isLoading() || this.isSaving();

  }        }    {      label: 'Description',

}
      });

  }      key: 'isActive',      placeholder: 'Describe the purpose and responsibilities of this role...',



  private loadRoleData(): void {      type: 'toggle',      required: false,

    if (this.roleData) {

      this.role.set(this.roleData);      label: 'Active Status',      rows: 4,

      this.populateFormWithRoleData(this.roleData);

      this.selectRolePermissions(this.roleData.permissions || []);      toggleLabel: 'Role is active and can be assigned to users',      validation: {

      this.isLoading.set(false);

      return;      defaultValue: true,        maxLength: 500

    }

      helpText: 'Toggle to enable/disable this role. Inactive roles cannot be assigned to users.',      },

    if (!this.roleId) {

      this.isLoading.set(false);      colSpan: 12      helpText: 'Optional description of the role\'s purpose and responsibilities (max 500 characters).',

      return;

    }    }      colSpan: 6

    

    this.rolePermissionApiService.getRoleById(this.roleId)  ];    },

      .pipe(takeUntil(this.destroy$))

      .subscribe({    {

        next: (roleResponse) => {

          if (roleResponse.success && roleResponse.data) {  formConfig: FormConfig = {      key: 'isActive',

            const role = roleResponse.data.role;

            this.role.set(role);    title: '',  // Will be set dynamically      type: 'toggle',

            this.populateFormWithRoleData(role);

            this.selectRolePermissions(role.permissions || []);    description: 'Configure role details and assign permissions below.',      label: 'Active Status',

          } else {

            this.handleError('Failed to load role', roleResponse.message);    layout: 'grid',      toggleLabel: 'Role is active and can be assigned to users',

          }

        },    columns: 2,      defaultValue: true,

        error: (error) => {

          console.error('Error loading role from API:', error);    showSubmitButton: true,      helpText: 'Toggle to enable/disable this role. Inactive roles cannot be assigned to users.',

          if (this.roleId) {

            const mockRole: RoleDto = {    showResetButton: false,      colSpan: 12

              id: this.roleId,

              name: 'Sample Role',    submitButtonText: 'Create Role',    }

              description: 'Sample role for testing',

              isActive: true,    validationMode: 'onChange',  ];

              permissions: []

            };    actionButtonAlignment: 'right'

            this.role.set(mockRole);

            this.populateFormWithRoleData(mockRole);  };  formConfig: FormConfig = {

          }

          this.handleError('Error loading role (using mock data)', error.message);    title: '',  // Will be set dynamically

        },

        complete: () => {  permissionsTreeData: TreeNode[] = [];    description: 'Configure role details and assign permissions below.',

          this.isLoading.set(false);

        }    layout: 'grid',

      });

  }  constructor(    columns: 2,



  private populateFormWithRoleData(roleData: RoleDto): void {    private rolePermissionApiService: RolePermissionApiService,    showSubmitButton: true,

    const initialData = {

      name: roleData.name,    private toastService: ToastService    showResetButton: false,

      description: roleData.description || '',

      isActive: roleData.isActive  ) {}    submitButtonText: 'Create Role',

    };

        validationMode: 'onChange',

    this.formData.set(initialData);

  }  ngOnInit(): void {    actionButtonAlignment: 'right'



  private buildPermissionsTree(permissions: PermissionDto[]): void {    this.initializeComponent();  };

    if (!Array.isArray(permissions)) {

      this.permissionsTreeData = [];  }

      return;

    }  permissionsTreeData: TreeNode[] = [];



    const groupedPermissions = this.groupPermissionsByMenu(permissions);  ngOnDestroy(): void {

    

    this.permissionsTreeData = Object.keys(groupedPermissions).map(menuName => ({    this.destroy$.next();  constructor(

      key: `menu-${menuName}`,

      label: menuName || 'General Permissions',    this.destroy$.complete();    private rolePermissionApiService: RolePermissionApiService,

      data: { type: 'menu', name: menuName },

      children: groupedPermissions[menuName].map(permission => ({  }    private toastService: ToastService

        key: permission.id,

        label: permission.displayName,  ) {}

        data: { type: 'permission', permission: permission }

      }))  private initializeComponent(): void {

    }));

  }    // Set form config based on edit mode  ngOnInit(): void {



  private groupPermissionsByMenu(permissions: PermissionDto[]): { [key: string]: PermissionDto[] } {    if (this.isEditMode && this.roleId) {    this.initializeComponent();

    return permissions.reduce((groups, permission) => {

      const menuName = permission.menuName || 'General';      this.formConfig.title = 'Edit Role';  }

      if (!groups[menuName]) {

        groups[menuName] = [];      this.formConfig.submitButtonText = 'Update Role';

      }

      groups[menuName].push(permission);    } else {  ngOnDestroy(): void {

      return groups;

    }, {} as { [key: string]: PermissionDto[] });      this.formConfig.title = 'Create New Role';    this.destroy$.next();

  }

      this.formConfig.submitButtonText = 'Create Role';    this.destroy$.complete();

  private selectRolePermissions(rolePermissions: PermissionDto[]): void {

    const selectedNodes: TreeNode[] = [];    }  }

    

    rolePermissions.forEach(permission => {

      const node = this.findPermissionNode(permission.id);

      if (node) {    this.loadInitialData();  private initializeComponent(): void {

        selectedNodes.push(node);

      }  }    // Set form config based on edit mode

    });

        if (this.isEditMode && this.roleId) {

    this.selectedPermissions.set(selectedNodes);

  }  private loadInitialData(): void {      this.formConfig.title = 'Edit Role';



  private findPermissionNode(permissionId: string): TreeNode | null {    this.isLoading.set(true);      this.formConfig.submitButtonText = 'Update Role';

    for (const menuNode of this.permissionsTreeData) {

      if (menuNode.children) {    } else {

        for (const permissionNode of menuNode.children) {

          if (permissionNode.key === permissionId) {    // Always load permissions first      this.formConfig.title = 'Create New Role';

            return permissionNode;

          }    this.rolePermissionApiService.getPermissions()      this.formConfig.submitButtonText = 'Create Role';

        }

      }      .pipe(takeUntil(this.destroy$))    }

    }

    return null;      .subscribe({

  }

        next: (permissionsResponse) => {    this.loadInitialData();

  onFormSubmit(formData: any): void {

    if (this.isSaving()) return;          if (permissionsResponse.success && permissionsResponse.data) {  }



    const selectedPermissionIds = this.selectedPermissions()            if (Array.isArray(permissionsResponse.data)) {

      .filter(node => node.data?.type === 'permission')

      .map(node => node.data.permission.id);              this.permissions.set(permissionsResponse.data);  private loadInitialData(): void {



    const roleRequest = {              this.buildPermissionsTree(permissionsResponse.data);    this.isLoading.set(true);

      name: formData.name,

      description: formData.description || '',            } else {

      isActive: formData.isActive ?? true,

      permissionIds: selectedPermissionIds              this.permissions.set([]);    // Always load permissions first

    };

              this.buildPermissionsTree([]);    this.rolePermissionApiService.getPermissions()

    this.isSaving.set(true);

            }      .pipe(takeUntil(this.destroy$))

    const operation = this.isEditMode && this.roleId

      ? this.rolePermissionApiService.updateRole(this.roleId, roleRequest)          } else {      .subscribe({

      : this.rolePermissionApiService.createRole(roleRequest);

            this.permissions.set([]);        next: (permissionsResponse) => {

    operation

      .pipe(takeUntil(this.destroy$))            this.buildPermissionsTree([]);          console.log('Permissions API response:', permissionsResponse);

      .subscribe({

        next: (response) => {          }          

          if (response.success && response.data) {

            const action = this.isEditMode ? 'updated' : 'created';          if (permissionsResponse.success && permissionsResponse.data) {

            this.toastService.success('Success', response.data.message || `Role "${roleRequest.name}" has been ${action} successfully`);

            this.onComplete.emit();          // If in edit mode, load the role data            console.log('Permissions data type:', typeof permissionsResponse.data);

          } else {

            this.handleError(`Failed to ${this.isEditMode ? 'update' : 'create'} role`, response.message || 'Unknown error');          if (this.isEditMode && this.roleId) {            console.log('Permissions data structure:', permissionsResponse.data);

          }

        },            this.loadRoleData();            

        error: (error) => {

          this.handleError(`Error ${this.isEditMode ? 'updating' : 'creating'} role`, error.message || 'Network error');          } else {            // Ensure we have an array

        },

        complete: () => {            // Initialize empty form data for create mode            if (Array.isArray(permissionsResponse.data)) {

          this.isSaving.set(false);

        }            this.formData.set({              this.permissions.set(permissionsResponse.data);

      });

  }              name: '',              this.buildPermissionsTree(permissionsResponse.data);



  onPermissionSelectionChange(selectedNodes: TreeNode[]): void {              description: '',            } else {

    this.selectedPermissions.set(selectedNodes);

  }              isActive: true              console.error('Expected permissions to be an array, got:', permissionsResponse.data);



  onCancelClick(): void {            });              this.permissions.set([]);

    this.onCancel.emit();

  }            this.isLoading.set(false);              this.buildPermissionsTree([]);



  private handleError(title: string, message?: string): void {          }            }

    this.isLoading.set(false);

    this.isSaving.set(false);        },          } else {

    this.toastService.error(title, message || 'Please try again later');

  }        error: (error) => {            console.warn('No permissions data received or response not successful');



  get formInitialData(): any {          console.error('Error loading permissions:', error);            this.permissions.set([]);

    return this.formData();

  }                      this.buildPermissionsTree([]);



  get isFormDisabled(): boolean {          // Use mock data when API is not available          }

    return this.isLoading() || this.isSaving();

  }          const mockPermissions: PermissionDto[] = [

}
            {          // If in edit mode, load the role data

              id: '1',          if (this.isEditMode && this.roleId) {

              permissionKey: 'users.view',            this.loadRoleData();

              displayName: 'View Users',          } else {

              description: 'Can view user list and details',            // Initialize empty form data for create mode

              permissionType: 1,            this.formData.set({

              isSystemPermission: true,              name: '',

              menuName: 'User Management'              description: '',

            }              isActive: true

          ];            });

                      this.isLoading.set(false);

          this.permissions.set(mockPermissions);          }

          this.buildPermissionsTree(mockPermissions);        },

                  error: (error) => {

          // Initialize form data for create mode even in error case          console.error('Error loading permissions:', error);

          if (!this.isEditMode) {          console.warn('Using mock permissions data for development');

            this.formData.set({          

              name: '',          // Use mock data when API is not available

              description: '',          const mockPermissions: PermissionDto[] = [

              isActive: true            {

            });              id: '1',

          }              permissionKey: 'users.view',

                        displayName: 'View Users',

          this.isLoading.set(false);              description: 'Can view user list and details',

          this.handleError('Error loading permissions (using mock data)', error.message);              permissionType: 1,

        }              isSystemPermission: true,

      });              menuName: 'User Management'

  }            },

            {

  private loadRoleData(): void {              id: '2',

    // If role data is passed directly from parent, use it              permissionKey: 'users.create',

    if (this.roleData) {              displayName: 'Create Users',

      this.role.set(this.roleData);              description: 'Can create new users',

      this.populateFormWithRoleData(this.roleData);              permissionType: 1,

      this.selectRolePermissions(this.roleData.permissions || []);              isSystemPermission: true,

      this.isLoading.set(false);              menuName: 'User Management'

      return;            },

    }            {

              id: '3',

    // Fallback: try to load from API if roleId is provided              permissionKey: 'users.edit',

    if (!this.roleId) {              displayName: 'Edit Users',

      this.isLoading.set(false);              description: 'Can edit user information',

      return;              permissionType: 1,

    }              isSystemPermission: true,

                  menuName: 'User Management'

    this.rolePermissionApiService.getRoleById(this.roleId)            },

      .pipe(takeUntil(this.destroy$))            {

      .subscribe({              id: '4',

        next: (roleResponse) => {              permissionKey: 'users.delete',

          if (roleResponse.success && roleResponse.data) {              displayName: 'Delete Users',

            const role = roleResponse.data.role;              description: 'Can delete users',

            this.role.set(role);              permissionType: 1,

            this.populateFormWithRoleData(role);              isSystemPermission: true,

            this.selectRolePermissions(role.permissions || []);              menuName: 'User Management'

          } else {            },

            this.handleError('Failed to load role', roleResponse.message);            {

          }              id: '5',

        },              permissionKey: 'roles.view',

        error: (error) => {              displayName: 'View Roles',

          console.error('Error loading role from API:', error);              description: 'Can view roles and permissions',

          // Create mock data for development if API fails              permissionType: 1,

          if (this.roleId) {              isSystemPermission: true,

            const mockRole: RoleDto = {              menuName: 'Role Management'

              id: this.roleId,            },

              name: 'Sample Role',            {

              description: 'Sample role for testing',              id: '6',

              isActive: true,              permissionKey: 'roles.create',

              permissions: []              displayName: 'Create Roles',

            };              description: 'Can create new roles',

            this.role.set(mockRole);              permissionType: 1,

            this.populateFormWithRoleData(mockRole);              isSystemPermission: true,

          }              menuName: 'Role Management'

          this.handleError('Error loading role (using mock data)', error.message);            },

        },            {

        complete: () => {              id: '7',

          this.isLoading.set(false);              permissionKey: 'roles.edit',

        }              displayName: 'Edit Roles',

      });              description: 'Can edit role information',

  }              permissionType: 1,

              isSystemPermission: true,

  private populateFormWithRoleData(roleData: RoleDto): void {              menuName: 'Role Management'

    const initialData = {            },

      name: roleData.name,            {

      description: roleData.description || '',              id: '8',

      isActive: roleData.isActive              permissionKey: 'system.view',

    };              displayName: 'View System',

                  description: 'Can view system settings',

    this.formData.set(initialData);              permissionType: 1,

  }              isSystemPermission: true,

              menuName: 'System'

  private buildPermissionsTree(permissions: PermissionDto[]): void {            }

    if (!Array.isArray(permissions)) {          ];

      this.permissionsTreeData = [];          

      return;          this.permissions.set(mockPermissions);

    }          this.buildPermissionsTree(mockPermissions);

          

    const groupedPermissions = this.groupPermissionsByMenu(permissions);          // Initialize form data for create mode even in error case

              if (!this.isEditMode) {

    this.permissionsTreeData = Object.keys(groupedPermissions).map(menuName => ({            this.formData.set({

      key: `menu-${menuName}`,              name: '',

      label: menuName || 'General Permissions',              description: '',

      data: { type: 'menu', name: menuName },              isActive: true

      children: groupedPermissions[menuName].map(permission => ({            });

        key: permission.id,          }

        label: permission.displayName,          

        data: { type: 'permission', permission: permission }          this.isLoading.set(false);

      }))          this.handleError('Error loading permissions (using mock data)', error.message);

    }));        }

  }      });

  }

  private groupPermissionsByMenu(permissions: PermissionDto[]): { [key: string]: PermissionDto[] } {

    return permissions.reduce((groups, permission) => {  private loadRoleData(): void {

      const menuName = permission.menuName || 'General';    // If role data is passed directly from parent, use it

      if (!groups[menuName]) {    if (this.roleData) {

        groups[menuName] = [];      console.log('Using role data from parent:', this.roleData);

      }      this.role.set(this.roleData);

      groups[menuName].push(permission);      this.populateFormWithRoleData(this.roleData);

      return groups;      this.selectRolePermissions(this.roleData.permissions || []);

    }, {} as { [key: string]: PermissionDto[] });      this.isLoading.set(false);

  }      return;

    }

  private selectRolePermissions(rolePermissions: PermissionDto[]): void {

    const selectedNodes: TreeNode[] = [];    // Fallback: try to load from API if roleId is provided

        if (!this.roleId) {

    rolePermissions.forEach(permission => {      this.isLoading.set(false);

      const node = this.findPermissionNode(permission.id);      return;

      if (node) {    }

        selectedNodes.push(node);    

      }    this.rolePermissionApiService.getRoleById(this.roleId)

    });      .pipe(takeUntil(this.destroy$))

          .subscribe({

    this.selectedPermissions.set(selectedNodes);        next: (roleResponse) => {

  }          if (roleResponse.success && roleResponse.data) {

            const role = roleResponse.data.role;

  private findPermissionNode(permissionId: string): TreeNode | null {            this.role.set(role);

    for (const menuNode of this.permissionsTreeData) {            this.populateFormWithRoleData(role);

      if (menuNode.children) {            this.selectRolePermissions(role.permissions || []);

        for (const permissionNode of menuNode.children) {          } else {

          if (permissionNode.key === permissionId) {            this.handleError('Failed to load role', roleResponse.message);

            return permissionNode;          }

          }        },

        }        error: (error) => {

      }          console.error('Error loading role from API:', error);

    }          // Create mock data for development if API fails

    return null;          if (this.roleId) {

  }            const mockRole: RoleDto = {

              id: this.roleId,

  // Event handlers              name: 'Sample Role',

  onFormSubmit(formData: any): void {              description: 'Sample role for testing',

    if (this.isSaving()) return;              isActive: true,

              permissions: []

    const selectedPermissionIds = this.selectedPermissions()            };

      .filter(node => node.data?.type === 'permission')            this.role.set(mockRole);

      .map(node => node.data.permission.id);            this.populateFormWithRoleData(mockRole);

          }

    const roleRequest = {          this.handleError('Error loading role (using mock data)', error.message);

      name: formData.name,        },

      description: formData.description || '',        complete: () => {

      isActive: formData.isActive ?? true,          this.isLoading.set(false);

      permissionIds: selectedPermissionIds        }

    };      });

  }

    this.isSaving.set(true);

  /**

    const operation = this.isEditMode && this.roleId   * Populate form fields with role data for editing

      ? this.rolePermissionApiService.updateRole(this.roleId, roleRequest)   * Updates the formData signal to trigger change detection in the dynamic form

      : this.rolePermissionApiService.createRole(roleRequest);   */

  private populateFormWithRoleData(roleData: RoleDto): void {

    operation    const initialData = {

      .pipe(takeUntil(this.destroy$))      name: roleData.name,

      .subscribe({      description: roleData.description || '',

        next: (response) => {      isActive: roleData.isActive

          if (response.success && response.data) {    };

            const action = this.isEditMode ? 'updated' : 'created';    

            this.toastService.success('Success', response.data.message || `Role "${roleRequest.name}" has been ${action} successfully`);    this.formData.set(initialData);

            this.onComplete.emit();    console.log('Form data populated for role:', roleData, 'Initial data:', initialData);

          } else {  }

            this.handleError(`Failed to ${this.isEditMode ? 'update' : 'create'} role`, response.message || 'Unknown error');

          }  private buildPermissionsTree(permissions: PermissionDto[]): void {

        },    // Add type checking to ensure permissions is an array

        error: (error) => {    if (!Array.isArray(permissions)) {

          this.handleError(`Error ${this.isEditMode ? 'updating' : 'creating'} role`, error.message || 'Network error');      console.warn('Permissions is not an array:', permissions);

        },      this.permissionsTreeData = [];

        complete: () => {      return;

          this.isSaving.set(false);    }

        }

      });    // Group permissions by menu/category for better tree structure

  }    const groupedPermissions = this.groupPermissionsByMenu(permissions);

    

  onPermissionSelectionChange(selectedNodes: TreeNode[]): void {    this.permissionsTreeData = Object.keys(groupedPermissions).map(menuName => ({

    this.selectedPermissions.set(selectedNodes);      key: `menu-${menuName}`,

  }      label: menuName || 'General Permissions',

      data: { type: 'menu', name: menuName },

  onCancelClick(): void {      children: groupedPermissions[menuName].map(permission => ({

    this.onCancel.emit();        key: permission.id,

  }        label: permission.displayName,

        data: { type: 'permission', permission: permission }

  private handleError(title: string, message?: string): void {      }))

    this.isLoading.set(false);    }));

    this.isSaving.set(false);  }

    this.toastService.error(title, message || 'Please try again later');

  }  private groupPermissionsByMenu(permissions: PermissionDto[]): { [key: string]: PermissionDto[] } {

    return permissions.reduce((groups, permission) => {

  // Getters for template      const menuName = permission.menuName || 'General';

  get formInitialData(): any {      if (!groups[menuName]) {

    return this.formData();        groups[menuName] = [];

  }      }

      groups[menuName].push(permission);

  get isFormDisabled(): boolean {      return groups;

    return this.isLoading() || this.isSaving();    }, {} as { [key: string]: PermissionDto[] });

  }  }

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
    return this.formData();
  }

  get isFormDisabled(): boolean {
    return this.isLoading() || this.isSaving();
  }
}