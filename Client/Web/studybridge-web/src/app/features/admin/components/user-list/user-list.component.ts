import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableWrapperComponent } from '../../../../shared/table-wrapper/table-wrapper.component';
import { DynamicFormComponent } from '../../../../shared/dynamic-form/dynamic-form.component';
import { AdminService } from '../../../../admin/services/admin.service';
import { AdminUser, CreateUserRequest, UpdateUserRequest, GetUsersRequest, PaginatedResult, ApiResponse } from '../../../../admin/services/admin.service';
import { TableConfiguration, TableColumn, TableAction } from '../../../../shared/models/table.models';
import { FormConfiguration, FormField } from '../../../../shared/models/form.models';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    TableWrapperComponent,
    DynamicFormComponent,
    DialogModule,
    ButtonModule,
    ToastModule,
    ConfirmDialogModule
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="admin-user-list">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold text-gray-800">User Management</h2>
        <button 
          pButton 
          type="button" 
          label="Add User" 
          icon="pi pi-plus" 
          class="p-button-primary"
          (click)="showCreateDialog()">
        </button>
      </div>

      <!-- Users Table -->
      <app-table-wrapper
        [configuration]="tableConfig"
        [data]="users"
        [loading]="loading"
        [totalRecords]="totalRecords"
        (lazyLoad)="onLazyLoad($event)"
        (actionClicked)="onActionClicked($event)"
        (selectionChanged)="onSelectionChanged($event)">
      </app-table-wrapper>

      <!-- Create/Edit User Dialog -->
      <p-dialog 
        [(visible)]="showDialog" 
        [header]="dialogTitle" 
        [modal]="true" 
        [draggable]="false" 
        [resizable]="false"
        [style]="{ width: '600px' }"
        (onHide)="hideDialog()">
        
        <app-dynamic-form
          *ngIf="formConfig"
          [configuration]="formConfig"
          [initialData]="selectedUser"
          (formSubmit)="onFormSubmit($event)"
          (formCancel)="hideDialog()">
        </app-dynamic-form>
      </p-dialog>

      <!-- Toast Messages -->
      <p-toast></p-toast>

      <!-- Confirmation Dialog -->
      <p-confirmDialog></p-confirmDialog>
    </div>
  `,
  styles: [`
    .admin-user-list {
      padding: 1rem;
    }
    
    :host ::ng-deep .p-dialog .p-dialog-content {
      padding: 0 !important;
    }
    
    :host ::ng-deep .p-dialog .p-dialog-header {
      background: #f8f9fa;
      border-bottom: 1px solid #dee2e6;
    }
  `]
})
export class UserListComponent implements OnInit {
  users: AdminUser[] = [];
  selectedUsers: AdminUser[] = [];
  selectedUser: AdminUser | null = null;
  loading = false;
  totalRecords = 0;
  showDialog = false;
  dialogTitle = '';
  formConfig: FormConfiguration | null = null;

  tableConfig: TableConfiguration = {
    columns: [
      {
        field: 'displayName',
        header: 'Name',
        sortable: true,
        filterable: true,
        type: 'text'
      },
      {
        field: 'email',
        header: 'Email',
        sortable: true,
        filterable: true,
        type: 'text'
      },
      {
        field: 'roles',
        header: 'Roles',
        sortable: false,
        filterable: true,
        type: 'custom',
        template: (value: string[]) => value?.join(', ') || 'No roles'
      },
      {
        field: 'isActive',
        header: 'Status',
        sortable: true,
        filterable: true,
        type: 'boolean',
        template: (value: boolean) => `<span class="p-tag ${value ? 'p-tag-success' : 'p-tag-danger'}">${value ? 'Active' : 'Inactive'}</span>`
      },
      {
        field: 'lastLoginDate',
        header: 'Last Login',
        sortable: true,
        filterable: false,
        type: 'date'
      },
      {
        field: 'createdAt',
        header: 'Created',
        sortable: true,
        filterable: false,
        type: 'date'
      }
    ],
    actions: [
      {
        label: 'Edit',
        icon: 'pi pi-pencil',
        action: 'edit',
        styleClass: 'p-button-text p-button-info'
      },
      {
        label: 'Delete',
        icon: 'pi pi-trash',
        action: 'delete',
        styleClass: 'p-button-text p-button-danger'
      },
      {
        label: 'View Details',
        icon: 'pi pi-eye',
        action: 'view',
        styleClass: 'p-button-text'
      }
    ],
    selectionMode: 'multiple',
    paginator: true,
    rows: 10,
    rowsPerPageOptions: [10, 25, 50],
    globalFilterFields: ['displayName', 'email'],
    exportable: true,
    lazy: true
  };

  constructor(
    private adminService: AdminService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(params?: any): void {
    this.loading = true;
    
    const queryParams: GetUsersRequest = {
      pageNumber: params?.first ? Math.floor(params.first / params.rows) + 1 : 1,
      pageSize: params?.rows || 10,
      searchTerm: params?.globalFilter || '',
      sortBy: params?.sortField || 'createdAt',
      sortDirection: (params?.sortOrder === 1 ? 'asc' : 'desc') as 'asc' | 'desc'
    };

    this.adminService.getUsers(queryParams).subscribe({
      next: (response: ApiResponse<PaginatedResult<AdminUser>>) => {
        this.users = response.data?.items || [];
        this.totalRecords = response.data?.totalCount || 0;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading users:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load users'
        });
        this.loading = false;
      }
    });
  }

  onLazyLoad(event: any): void {
    this.loadUsers(event);
  }

  onActionClicked(event: { action: string; data: AdminUser }): void {
    switch (event.action) {
      case 'edit':
        this.editUser(event.data);
        break;
      case 'delete':
        this.deleteUser(event.data);
        break;
      case 'view':
        this.viewUser(event.data);
        break;
    }
  }

  onSelectionChanged(selectedItems: AdminUser[]): void {
    this.selectedUsers = selectedItems;
  }

  showCreateDialog(): void {
    this.selectedUser = null;
    this.dialogTitle = 'Create New User';
    this.formConfig = this.getCreateUserFormConfig();
    this.showDialog = true;
  }

  editUser(user: AdminUser): void {
    this.selectedUser = user;
    this.dialogTitle = 'Edit User';
    this.formConfig = this.getEditUserFormConfig();
    this.showDialog = true;
  }

  viewUser(user: AdminUser): void {
    this.selectedUser = user;
    this.dialogTitle = 'User Details';
    this.formConfig = this.getViewUserFormConfig();
    this.showDialog = true;
  }

  deleteUser(user: AdminUser): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete user "${user.displayName}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.adminService.deleteUser(user.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User deleted successfully'
            });
            this.loadUsers();
          },
          error: (error: any) => {
            console.error('Error deleting user:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete user'
            });
          }
        });
      }
    });
  }

  onFormSubmit(formData: any): void {
    if (this.selectedUser) {
      // Update existing user
      const updateRequest: UpdateUserRequest = {
        displayName: formData.displayName,
        isActive: formData.isActive
      };

      this.adminService.updateUser(this.selectedUser.id, updateRequest).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User updated successfully'
          });
          this.hideDialog();
          this.loadUsers();
        },
        error: (error: any) => {
          console.error('Error updating user:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update user'
          });
        }
      });
    } else {
      // Create new user
      const createRequest: CreateUserRequest = {
        displayName: formData.displayName,
        email: formData.email,
        password: formData.password,
        roles: formData.roles || []
      };

      this.adminService.createUser(createRequest).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User created successfully'
          });
          this.hideDialog();
          this.loadUsers();
        },
        error: (error: any) => {
          console.error('Error creating user:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create user'
          });
        }
      });
    }
  }

  hideDialog(): void {
    this.showDialog = false;
    this.selectedUser = null;
    this.formConfig = null;
  }

  private getCreateUserFormConfig(): FormConfiguration {
    return {
      fields: [
        {
          key: 'displayName',
          type: 'text',
          label: 'Display Name',
          required: true,
          validators: {
            required: 'Display name is required',
            minLength: { value: 2, message: 'Display name must be at least 2 characters' }
          },
          grid: { col: 12 }
        },
        {
          key: 'email',
          type: 'email',
          label: 'Email Address',
          required: true,
          validators: {
            required: 'Email is required',
            email: 'Please enter a valid email address'
          },
          grid: { col: 12 }
        },
        {
          key: 'password',
          type: 'password',
          label: 'Password',
          required: true,
          validators: {
            required: 'Password is required',
            minLength: { value: 8, message: 'Password must be at least 8 characters' }
          },
          grid: { col: 12 }
        },
        {
          key: 'roles',
          type: 'multiselect',
          label: 'Roles',
          required: false,
          options: this.getRoleOptions(),
          grid: { col: 12 }
        }
      ],
      layout: 'vertical',
      submitButtonText: 'Create User',
      cancelButtonText: 'Cancel'
    };
  }

  private getEditUserFormConfig(): FormConfiguration {
    return {
      fields: [
        {
          key: 'displayName',
          type: 'text',
          label: 'Display Name',
          required: true,
          validators: {
            required: 'Display name is required',
            minLength: { value: 2, message: 'Display name must be at least 2 characters' }
          },
          grid: { col: 12 }
        },
        {
          key: 'email',
          type: 'email',
          label: 'Email Address',
          required: true,
          validators: {
            required: 'Email is required',
            email: 'Please enter a valid email address'
          },
          grid: { col: 12 }
        },
        {
          key: 'isActive',
          type: 'checkbox',
          label: 'Active',
          required: false,
          grid: { col: 12 }
        },
        {
          key: 'roles',
          type: 'multiselect',
          label: 'Roles',
          required: false,
          options: this.getRoleOptions(),
          grid: { col: 12 }
        }
      ],
      layout: 'vertical',
      submitButtonText: 'Update User',
      cancelButtonText: 'Cancel'
    };
  }

  private getViewUserFormConfig(): FormConfiguration {
    return {
      fields: [
        {
          key: 'displayName',
          type: 'text',
          label: 'Display Name',
          required: false,
          disabled: true,
          grid: { col: 6 }
        },
        {
          key: 'email',
          type: 'email',
          label: 'Email Address',
          required: false,
          disabled: true,
          grid: { col: 6 }
        },
        {
          key: 'isActive',
          type: 'checkbox',
          label: 'Active',
          required: false,
          disabled: true,
          grid: { col: 6 }
        },
        {
          key: 'lastLoginDate',
          type: 'date',
          label: 'Last Login',
          required: false,
          disabled: true,
          grid: { col: 6 }
        },
        {
          key: 'createdAt',
          type: 'date',
          label: 'Created At',
          required: false,
          disabled: true,
          grid: { col: 6 }
        },
        {
          key: 'roles',
          type: 'multiselect',
          label: 'Roles',
          required: false,
          disabled: true,
          options: this.getRoleOptions(),
          grid: { col: 12 }
        }
      ],
      layout: 'vertical',
      hideSubmitButton: true,
      cancelButtonText: 'Close'
    };
  }

  private getRoleOptions(): any[] {
    // This should be loaded from the admin service
    return [
      { label: 'Super Admin', value: 'SuperAdmin' },
      { label: 'Admin', value: 'Admin' },
      { label: 'Editor', value: 'Editor' },
      { label: 'Moderator', value: 'Moderator' },
      { label: 'User', value: 'User' }
    ];
  }
}