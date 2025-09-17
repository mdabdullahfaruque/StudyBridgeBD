import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminService, AdminUser, GetUsersRequest } from '../../services/admin.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    TagModule,
    SelectModule,
    FormsModule,
    RouterLink
  ],
  template: `
    <div class="user-list-container p-6">
      <!-- Page Header -->
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">User Management</h1>
          <p class="text-gray-600 dark:text-gray-400 mt-1">Manage users, roles, and permissions</p>
        </div>
        <p-button 
          label="Add New User" 
          icon="pi pi-plus" 
          routerLink="/admin/users/create">
        </p-button>
      </div>

      <!-- Filters Card -->
      <p-card styleClass="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
          <!-- Search -->
          <div class="flex flex-col">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Search</label>
            <span class="p-input-icon-left">
              <i class="pi pi-search"></i>
              <input 
                type="text" 
                pInputText 
                placeholder="Search users..." 
                [(ngModel)]="searchTerm"
                (input)="onSearchChange()"
                class="w-full">
            </span>
          </div>

          <!-- Role Filter -->
          <div class="flex flex-col">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Filter by Role</label>
            <p-select 
              [options]="roleOptions" 
              [(ngModel)]="selectedRole"
              optionLabel="label" 
              optionValue="value"
              placeholder="All Roles" 
              [showClear]="true"
              (onChange)="onRoleChange()"
              styleClass="w-full">
            </p-select>
          </div>

          <!-- Status Filter -->
          <div class="flex flex-col">
            <label class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Filter by Status</label>
            <p-select 
              [options]="statusOptions" 
              [(ngModel)]="selectedStatus"
              optionLabel="label" 
              optionValue="value"
              placeholder="All Status" 
              [showClear]="true"
              (onChange)="onStatusChange()"
              styleClass="w-full">
            </p-select>
          </div>

          <!-- Actions -->
          <div class="flex flex-col justify-end">
            <p-button 
              label="Reset Filters" 
              icon="pi pi-filter-slash" 
              [text]="true"
              (click)="resetFilters()"
              styleClass="mb-2">
            </p-button>
          </div>
        </div>
      </p-card>

      <!-- Users Table -->
      <p-card>
        <p-table 
          [value]="users()" 
          [loading]="isLoading()"
          [paginator]="true" 
          [rows]="pageSize()"
          [totalRecords]="totalRecords()"
          [lazy]="true"
          (onLazyLoad)="loadUsers($event)"
          [rowsPerPageOptions]="[10, 25, 50]"
          styleClass="p-datatable-gridlines">
          
          <ng-template pTemplate="header">
            <tr>
              <th pSortableColumn="displayName">
                Name 
                <p-sortIcon field="displayName"></p-sortIcon>
              </th>
              <th pSortableColumn="email">
                Email 
                <p-sortIcon field="email"></p-sortIcon>
              </th>
              <th>Status</th>
              <th>Roles</th>
              <th pSortableColumn="createdAt">
                Created 
                <p-sortIcon field="createdAt"></p-sortIcon>
              </th>
              <th pSortableColumn="lastLoginAt">
                Last Login 
                <p-sortIcon field="lastLoginAt"></p-sortIcon>
              </th>
              <th>Actions</th>
            </tr>
          </ng-template>
          
          <ng-template pTemplate="body" let-user>
            <tr>
              <td>
                <div class="flex items-center space-x-3">
                  <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900 rounded-full flex items-center justify-center">
                    <span class="text-sm font-medium text-blue-600 dark:text-blue-400">
                      {{ getInitials(user.displayName) }}
                    </span>
                  </div>
                  <div>
                    <div class="font-medium text-gray-900 dark:text-white">{{ user.displayName }}</div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">{{ user.firstName }} {{ user.lastName }}</div>
                  </div>
                </div>
              </td>
              <td>
                <div class="text-gray-900 dark:text-white">{{ user.email }}</div>
                <div class="flex items-center space-x-1 mt-1">
                  <i 
                    class="pi text-xs"
                    [class.pi-check-circle]="user.isEmailVerified"
                    [class.pi-times-circle]="!user.isEmailVerified"
                    [class.text-green-500]="user.isEmailVerified"
                    [class.text-red-500]="!user.isEmailVerified">
                  </i>
                  <span class="text-xs" 
                        [class.text-green-600]="user.isEmailVerified"
                        [class.text-red-600]="!user.isEmailVerified">
                    {{ user.isEmailVerified ? 'Verified' : 'Not Verified' }}
                  </span>
                </div>
              </td>
              <td>
                <p-tag 
                  [value]="user.isActive ? 'Active' : 'Inactive'" 
                  [severity]="user.isActive ? 'success' : 'danger'">
                </p-tag>
              </td>
              <td>
                <div class="flex flex-wrap gap-1">
                  <span 
                    *ngFor="let role of user.roles" 
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                    {{ role }}
                  </span>
                </div>
              </td>
              <td>
                <div class="text-sm text-gray-900 dark:text-white">
                  {{ formatDate(user.createdAt) }}
                </div>
              </td>
              <td>
                <div class="text-sm text-gray-900 dark:text-white">
                  {{ user.lastLoginAt ? formatDate(user.lastLoginAt) : 'Never' }}
                </div>
              </td>
              <td>
                <div class="flex space-x-2">
                  <p-button 
                    icon="pi pi-eye" 
                    [text]="true" 
                    size="small"
                    [routerLink]="['/admin/users', user.id]">
                  </p-button>
                  <p-button 
                    icon="pi pi-pencil" 
                    [text]="true" 
                    size="small"
                    severity="secondary"
                    [routerLink]="['/admin/users', user.id, 'edit']">
                  </p-button>
                  <p-button 
                    [icon]="user.isActive ? 'pi pi-ban' : 'pi pi-check'" 
                    [text]="true" 
                    size="small"
                    [severity]="user.isActive ? 'danger' : 'success'"
                    (click)="toggleUserStatus(user)">
                  </p-button>
                </div>
              </td>
            </tr>
          </ng-template>
          
          <ng-template pTemplate="emptymessage">
            <tr>
              <td colspan="7" class="text-center py-8">
                <div class="text-gray-500 dark:text-gray-400">
                  <i class="pi pi-users text-3xl mb-3 block"></i>
                  <p class="text-lg mb-2">No users found</p>
                  <p class="text-sm">{{ searchTerm ? 'Try adjusting your search criteria' : 'Get started by adding your first user' }}</p>
                </div>
              </td>
            </tr>
          </ng-template>
        </p-table>
      </p-card>
    </div>
  `,
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit {
  // Signals for reactive state
  isLoading = signal(false);
  users = signal<AdminUser[]>([]);
  totalRecords = signal(0);
  pageSize = signal(10);
  
  // Filter states
  searchTerm = '';
  selectedRole: string | null = null;
  selectedStatus: boolean | null = null;

  // Options for dropdowns
  roleOptions = [
    { label: 'SuperAdmin', value: 'SuperAdmin' },
    { label: 'Admin', value: 'Admin' },
    { label: 'Finance', value: 'Finance' },
    { label: 'ContentManager', value: 'ContentManager' },
    { label: 'User', value: 'User' }
  ];

  statusOptions = [
    { label: 'Active', value: true },
    { label: 'Inactive', value: false }
  ];

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.loadUsers();
  }

  async loadUsers(event?: any) {
    try {
      this.isLoading.set(true);
      
      const request: GetUsersRequest = {
        pageNumber: event ? Math.floor(event.first / event.rows) + 1 : 1,
        pageSize: event ? event.rows : this.pageSize(),
        searchTerm: this.searchTerm || undefined,
        role: this.selectedRole || undefined,
        isActive: this.selectedStatus ?? undefined,
        sortBy: event?.sortField || 'createdAt',
        sortDirection: event?.sortOrder === -1 ? 'desc' : 'asc'
      };

      // TODO: Replace with actual API call
      // const response = await this.adminService.getUsers(request).toPromise();
      
      // Simulated data for now
      const mockUsers: AdminUser[] = Array.from({ length: 25 }, (_, i) => ({
        id: `user-${i + 1}`,
        email: `user${i + 1}@example.com`,
        displayName: `User ${i + 1}`,
        firstName: `First${i + 1}`,
        lastName: `Last${i + 1}`,
        isActive: Math.random() > 0.3,
        isEmailVerified: Math.random() > 0.2,
        roles: i === 0 ? ['SuperAdmin'] : i < 3 ? ['Admin'] : ['User'],
        permissions: [],
        createdAt: new Date(Date.now() - Math.random() * 30 * 24 * 60 * 60 * 1000).toISOString(),
        lastLoginAt: Math.random() > 0.3 ? new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000).toISOString() : undefined
      }));

      // Apply filters (simulation)
      let filteredUsers = mockUsers;
      if (this.searchTerm) {
        filteredUsers = filteredUsers.filter(user => 
          user.displayName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
          user.email.toLowerCase().includes(this.searchTerm.toLowerCase())
        );
      }
      if (this.selectedRole) {
        filteredUsers = filteredUsers.filter(user => user.roles.includes(this.selectedRole!));
      }
      if (this.selectedStatus !== null) {
        filteredUsers = filteredUsers.filter(user => user.isActive === this.selectedStatus);
      }

      const pageNumber = request.pageNumber || 1;
      const pageSize = request.pageSize || 10;
      const startIndex = (pageNumber - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      const paginatedUsers = filteredUsers.slice(startIndex, endIndex);

      this.users.set(paginatedUsers);
      this.totalRecords.set(filteredUsers.length);
      
    } catch (error) {
      console.error('Failed to load users:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  onSearchChange() {
    // Debounce search
    setTimeout(() => {
      this.loadUsers();
    }, 500);
  }

  onRoleChange() {
    this.loadUsers();
  }

  onStatusChange() {
    this.loadUsers();
  }

  resetFilters() {
    this.searchTerm = '';
    this.selectedRole = null;
    this.selectedStatus = null;
    this.loadUsers();
  }

  async toggleUserStatus(user: AdminUser) {
    try {
      if (user.isActive) {
        // await this.adminService.deactivateUser(user.id).toPromise();
      } else {
        // await this.adminService.activateUser(user.id).toPromise();
      }
      
      // Update local state
      const updatedUsers = this.users().map(u => 
        u.id === user.id ? { ...u, isActive: !u.isActive } : u
      );
      this.users.set(updatedUsers);
      
    } catch (error) {
      console.error('Failed to toggle user status:', error);
    }
  }

  getInitials(name: string): string {
    return name ? name.split(' ').map(n => n[0]).join('').toUpperCase() : 'U';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(date);
  }
}