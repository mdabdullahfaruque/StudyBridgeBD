import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TreeWrapperComponent } from '../../../../shared/tree-wrapper/tree-wrapper.component';
import { AdminService, Permission, ApiResponse } from '../../../../admin/services/admin.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { TreeNode } from 'primeng/api';

@Component({
  selector: 'app-permission-tree',
  standalone: true,
  imports: [
    CommonModule,
    TreeWrapperComponent,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <div class="admin-permission-tree">
      <div class="flex justify-between items-center mb-6">
        <h2 class="text-2xl font-bold text-gray-800">Permission Management</h2>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Permission Tree -->
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">System Permissions</h3>
          <app-tree-wrapper
            [data]="permissionTreeData"
            [loading]="loading"
            [selectionMode]="'checkbox'"
            [showCounts]="true"
            (selectionChanged)="onSelectionChanged($event)"
            (nodeExpanded)="onNodeExpanded($event)"
            (nodeCollapsed)="onNodeCollapsed($event)">
          </app-tree-wrapper>
        </div>

        <!-- Selected Permissions Panel -->
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-semibold mb-4">
            Selected Permissions 
            <span class="text-sm font-normal text-gray-500">
              ({{ selectedPermissions.length }})
            </span>
          </h3>
          
          <div *ngIf="selectedPermissions.length === 0" class="text-gray-500 text-center py-8">
            No permissions selected
          </div>

          <div *ngIf="selectedPermissions.length > 0" class="space-y-3">
            <div 
              *ngFor="let permission of selectedPermissions" 
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div class="flex-1">
                <div class="font-medium text-gray-900">
                  {{ permission.displayName }}
                </div>
                <div class="text-sm text-gray-500">
                  {{ permission.key }} - {{ permission.category }}
                </div>
                <div *ngIf="permission.description" class="text-xs text-gray-400 mt-1">
                  {{ permission.description }}
                </div>
              </div>
              <span 
                class="px-2 py-1 text-xs rounded-full"
                [class]="getPermissionTypeClass(permission.type)">
                {{ permission.type }}
              </span>
            </div>
          </div>

          <!-- Actions -->
          <div *ngIf="selectedPermissions.length > 0" class="mt-6 flex gap-2">
            <button 
              type="button" 
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
              Export Selection
            </button>
            <button 
              type="button" 
              class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
              (click)="clearSelection()">
              Clear Selection
            </button>
          </div>
        </div>
      </div>

      <!-- Permission Statistics -->
      <div class="mt-8 grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-lg shadow p-6 text-center">
          <div class="text-2xl font-bold text-blue-600">{{ totalPermissions }}</div>
          <div class="text-sm text-gray-500">Total Permissions</div>
        </div>
        <div class="bg-white rounded-lg shadow p-6 text-center">
          <div class="text-2xl font-bold text-green-600">{{ activePermissions }}</div>
          <div class="text-sm text-gray-500">Active Permissions</div>
        </div>
        <div class="bg-white rounded-lg shadow p-6 text-center">
          <div class="text-2xl font-bold text-purple-600">{{ categoryCount }}</div>
          <div class="text-sm text-gray-500">Categories</div>
        </div>
        <div class="bg-white rounded-lg shadow p-6 text-center">
          <div class="text-2xl font-bold text-orange-600">{{ selectedPermissions.length }}</div>
          <div class="text-sm text-gray-500">Selected</div>
        </div>
      </div>

      <!-- Toast Messages -->
      <p-toast></p-toast>
    </div>
  `,
  styles: [`
    .admin-permission-tree {
      padding: 1rem;
    }
    
    .permission-type-read {
      @apply bg-blue-100 text-blue-800;
    }
    
    .permission-type-write {
      @apply bg-green-100 text-green-800;
    }
    
    .permission-type-delete {
      @apply bg-red-100 text-red-800;
    }
    
    .permission-type-execute {
      @apply bg-purple-100 text-purple-800;
    }
    
    .permission-type-admin {
      @apply bg-orange-100 text-orange-800;
    }
  `]
})
export class PermissionTreeComponent implements OnInit {
  permissions: Permission[] = [];
  permissionTreeData: TreeNode[] = [];
  selectedPermissions: Permission[] = [];
  loading = false;

  // Statistics
  totalPermissions = 0;
  activePermissions = 0;
  categoryCount = 0;

  constructor(
    private adminService: AdminService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadPermissions();
  }

  loadPermissions(): void {
    this.loading = true;
    
    this.adminService.getPermissions().subscribe({
      next: (response: ApiResponse<Permission[]>) => {
        this.permissions = response.data || [];
        this.buildPermissionTree();
        this.calculateStatistics();
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading permissions:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load permissions'
        });
        this.loading = false;
      }
    });
  }

  buildPermissionTree(): void {
    // Group permissions by category
    const categories = this.groupPermissionsByCategory();
    
    this.permissionTreeData = Object.keys(categories).map(category => ({
      label: category,
      key: category,
      data: { type: 'category', category },
      icon: 'pi pi-folder',
      children: categories[category].map(permission => ({
        label: permission.displayName,
        key: permission.id,
        data: { type: 'permission', permission },
        icon: this.getPermissionIcon(permission.type),
        leaf: true
      })),
      expanded: false
    }));
  }

  groupPermissionsByCategory(): { [key: string]: Permission[] } {
    const groups: { [key: string]: Permission[] } = {};
    
    this.permissions.forEach(permission => {
      if (!groups[permission.category]) {
        groups[permission.category] = [];
      }
      groups[permission.category].push(permission);
    });
    
    return groups;
  }

  getPermissionIcon(type: any): string {
    switch (type) {
      case 'Read':
        return 'pi pi-eye';
      case 'Write':
        return 'pi pi-pencil';
      case 'Delete':
        return 'pi pi-trash';
      case 'Execute':
        return 'pi pi-cog';
      case 'Admin':
        return 'pi pi-shield';
      default:
        return 'pi pi-check';
    }
  }

  getPermissionTypeClass(type: any): string {
    switch (type) {
      case 'Read':
        return 'permission-type-read';
      case 'Write':
        return 'permission-type-write';
      case 'Delete':
        return 'permission-type-delete';
      case 'Execute':
        return 'permission-type-execute';
      case 'Admin':
        return 'permission-type-admin';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  onSelectionChanged(selectedNodes: TreeNode[]): void {
    this.selectedPermissions = [];
    
    selectedNodes.forEach(node => {
      if (node.data?.type === 'permission') {
        this.selectedPermissions.push(node.data.permission);
      } else if (node.data?.type === 'category') {
        // If category is selected, add all its permissions
        const categoryPermissions = this.permissions.filter(
          p => p.category === node.data.category
        );
        this.selectedPermissions.push(...categoryPermissions);
      }
    });
    
    // Remove duplicates
    this.selectedPermissions = this.selectedPermissions.filter(
      (permission, index, self) => 
        index === self.findIndex(p => p.id === permission.id)
    );
  }

  onNodeExpanded(event: any): void {
    console.log('Node expanded:', event.node);
  }

  onNodeCollapsed(event: any): void {
    console.log('Node collapsed:', event.node);
  }

  clearSelection(): void {
    this.selectedPermissions = [];
    // TODO: Clear tree selection
  }

  calculateStatistics(): void {
    this.totalPermissions = this.permissions.length;
    this.activePermissions = this.permissions.length; // All permissions are considered active
    this.categoryCount = new Set(this.permissions.map(p => p.category)).size;
  }
}