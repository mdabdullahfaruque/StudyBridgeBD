import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableWrapperComponent, TableColumn, TableConfig } from '../../../../shared/table-wrapper/table-wrapper.component';
import { AdminService, SystemRole, ApiResponse } from '../../../../admin/services/admin.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    TableWrapperComponent,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <div class="admin-role-list">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold text-gray-800">Role Management</h2>
      </div>

      <!-- Roles Table -->
      <app-table-wrapper
        [columns]="columns"
        [config]="config"
        [data]="roles"
        [selectedItems]="selectedRoles"
        (selectedItemsChange)="onSelectionChanged($event)">
      </app-table-wrapper>

      <!-- Toast Messages -->
      <p-toast></p-toast>
    </div>
  `,
  styles: [`
    .admin-role-list {
      padding: 1rem;
    }
  `]
})
export class RoleListComponent implements OnInit {
  roles: SystemRole[] = [];
  selectedRoles: SystemRole[] = [];
  loading = false;

  columns: TableColumn[] = [
    {
      field: 'displayName',
      header: 'Role Name',
      sortable: true,
      filterable: true,
      type: 'text'
    },
    {
      field: 'name',
      header: 'System Name',
      sortable: true,
      filterable: true,
      type: 'text'
    },
    {
      field: 'description',
      header: 'Description',
      sortable: false,
      filterable: true,
      type: 'text'
    },
    {
      field: 'isActive',
      header: 'Status',
      sortable: true,
      filterable: true,
      type: 'boolean'
    }
  ];

  config: TableConfig = {
    selectionMode: 'multiple',
    paginator: false,
    globalFilterFields: ['displayName', 'name', 'description'],
    exportable: true
  };

  constructor(
    private adminService: AdminService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.loading = true;
    
    this.adminService.getRoles().subscribe({
      next: (response: ApiResponse<SystemRole[]>) => {
        this.roles = response.data || [];
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading roles:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load roles'
        });
        this.loading = false;
      }
    });
  }

  onSelectionChanged(selectedItems: SystemRole[]): void {
    this.selectedRoles = selectedItems;
  }
}