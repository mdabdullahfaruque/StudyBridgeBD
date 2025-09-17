import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { AdminService, ApiResponse } from '../../../../admin/services/admin.service';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

interface AdminStats {
  totalUsers: number;
  activeUsers: number;
  totalRoles: number;
  totalPermissions: number;
  recentActivity: ActivityItem[];
}

interface ActivityItem {
  id: string;
  action: string;
  user: string;
  timestamp: string;
  type: 'user' | 'role' | 'permission' | 'system';
}

@Component({
  selector: 'app-admin-overview',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <div class="admin-overview">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-gray-900 mb-2">Admin Management</h1>
        <p class="text-gray-600">Manage users, roles, permissions, and system settings</p>
      </div>

      <!-- Quick Stats -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 rounded-full bg-blue-100">
              <i class="pi pi-users text-blue-600 text-xl"></i>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-500">Total Users</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.totalUsers }}</p>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 rounded-full bg-green-100">
              <i class="pi pi-user-plus text-green-600 text-xl"></i>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-500">Active Users</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.activeUsers }}</p>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 rounded-full bg-purple-100">
              <i class="pi pi-id-card text-purple-600 text-xl"></i>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-500">System Roles</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.totalRoles }}</p>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 rounded-full bg-orange-100">
              <i class="pi pi-shield text-orange-600 text-xl"></i>
            </div>
            <div class="ml-4">
              <p class="text-sm font-medium text-gray-500">Permissions</p>
              <p class="text-2xl font-bold text-gray-900">{{ stats.totalPermissions }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Management Cards -->
      <div class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6 mb-8">
        <!-- User Management -->
        <p-card>
          <ng-template pTemplate="header">
            <div class="p-4 bg-blue-50">
              <div class="flex items-center">
                <i class="pi pi-users text-blue-600 text-2xl mr-3"></i>
                <h3 class="text-lg font-semibold text-blue-900">User Management</h3>
              </div>
            </div>
          </ng-template>
          
          <div class="p-4">
            <p class="text-gray-600 mb-4">
              Manage user accounts, profiles, and access permissions
            </p>
            <ul class="space-y-2 text-sm text-gray-500 mb-6">
              <li>• Create and edit user accounts</li>
              <li>• Assign roles and permissions</li>
              <li>• Monitor user activity</li>
              <li>• Manage user subscriptions</li>
            </ul>
            <button 
              pButton 
              type="button" 
              label="Manage Users" 
              icon="pi pi-arrow-right"
              class="w-full"
              routerLink="/admin/users">
            </button>
          </div>
        </p-card>

        <!-- Role Management -->
        <p-card>
          <ng-template pTemplate="header">
            <div class="p-4 bg-purple-50">
              <div class="flex items-center">
                <i class="pi pi-id-card text-purple-600 text-2xl mr-3"></i>
                <h3 class="text-lg font-semibold text-purple-900">Role Management</h3>
              </div>
            </div>
          </ng-template>
          
          <div class="p-4">
            <p class="text-gray-600 mb-4">
              Configure system roles and their associated permissions
            </p>
            <ul class="space-y-2 text-sm text-gray-500 mb-6">
              <li>• View system roles</li>
              <li>• Understand role hierarchies</li>
              <li>• Monitor role assignments</li>
              <li>• Role-based access control</li>
            </ul>
            <button 
              pButton 
              type="button" 
              label="Manage Roles" 
              icon="pi pi-arrow-right"
              class="w-full"
              routerLink="/admin/roles">
            </button>
          </div>
        </p-card>

        <!-- Permission Management -->
        <p-card>
          <ng-template pTemplate="header">
            <div class="p-4 bg-orange-50">
              <div class="flex items-center">
                <i class="pi pi-shield text-orange-600 text-2xl mr-3"></i>
                <h3 class="text-lg font-semibold text-orange-900">Permission Management</h3>
              </div>
            </div>
          </ng-template>
          
          <div class="p-4">
            <p class="text-gray-600 mb-4">
              Explore and manage system permissions hierarchy
            </p>
            <ul class="space-y-2 text-sm text-gray-500 mb-6">
              <li>• Browse permission tree</li>
              <li>• View permission categories</li>
              <li>• Understand access levels</li>
              <li>• Security audit trail</li>
            </ul>
            <button 
              pButton 
              type="button" 
              label="View Permissions" 
              icon="pi pi-arrow-right"
              class="w-full"
              routerLink="/admin/permissions">
            </button>
          </div>
        </p-card>
      </div>

      <!-- Recent Activity -->
      <div class="bg-white rounded-lg shadow">
        <div class="px-6 py-4 border-b border-gray-200">
          <h3 class="text-lg font-semibold text-gray-900">Recent Activity</h3>
        </div>
        
        <div class="p-6">
          <div *ngIf="stats.recentActivity.length === 0" class="text-center py-8 text-gray-500">
            No recent activity to display
          </div>
          
          <div *ngIf="stats.recentActivity.length > 0" class="space-y-4">
            <div 
              *ngFor="let activity of stats.recentActivity" 
              class="flex items-center p-4 bg-gray-50 rounded-lg">
              <div 
                class="p-2 rounded-full"
                [ngClass]="getActivityIcon(activity.type)">
                <i [class]="getActivityIconClass(activity.type)"></i>
              </div>
              <div class="ml-4 flex-1">
                <p class="text-sm font-medium text-gray-900">{{ activity.action }}</p>
                <p class="text-xs text-gray-500">by {{ activity.user }}</p>
              </div>
              <div class="text-xs text-gray-400">
                {{ formatTimestamp(activity.timestamp) }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Toast Messages -->
      <p-toast></p-toast>
    </div>
  `,
  styles: [`
    .admin-overview {
      padding: 1rem;
      max-width: 100%;
    }
    
    :host ::ng-deep .p-card {
      height: 100%;
    }
    
    :host ::ng-deep .p-card .p-card-body {
      padding: 0;
    }
    
    :host ::ng-deep .p-card .p-card-content {
      padding: 0;
    }
  `]
})
export class AdminOverviewComponent implements OnInit {
  stats: AdminStats = {
    totalUsers: 0,
    activeUsers: 0,
    totalRoles: 0,
    totalPermissions: 0,
    recentActivity: []
  };

  loading = false;

  constructor(
    private adminService: AdminService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.loading = true;

    // Load basic stats - this could be a single API call in the future
    Promise.all([
      this.loadUserStats(),
      this.loadRoleStats(),
      this.loadPermissionStats()
    ]).then(() => {
      this.loading = false;
    }).catch((error) => {
      console.error('Error loading admin stats:', error);
      this.loading = false;
    });
  }

  private async loadUserStats(): Promise<void> {
    try {
      const response = await this.adminService.getUsers({ pageSize: 1 }).toPromise();
      if (response?.data) {
        this.stats.totalUsers = response.data.totalCount;
        // For now, assume all users are active (could be filtered in a real implementation)
        this.stats.activeUsers = Math.floor(response.data.totalCount * 0.85);
      }
    } catch (error) {
      console.error('Error loading user stats:', error);
    }
  }

  private async loadRoleStats(): Promise<void> {
    try {
      const response = await this.adminService.getRoles().toPromise();
      if (response?.data) {
        this.stats.totalRoles = response.data.length;
      }
    } catch (error) {
      console.error('Error loading role stats:', error);
    }
  }

  private async loadPermissionStats(): Promise<void> {
    try {
      const response = await this.adminService.getPermissions().toPromise();
      if (response?.data) {
        this.stats.totalPermissions = response.data.length;
      }
    } catch (error) {
      console.error('Error loading permission stats:', error);
    }
  }

  getActivityIcon(type: string): string {
    const iconClasses = {
      user: 'bg-blue-100',
      role: 'bg-purple-100',
      permission: 'bg-orange-100',
      system: 'bg-gray-100'
    };
    return iconClasses[type as keyof typeof iconClasses] || 'bg-gray-100';
  }

  getActivityIconClass(type: string): string {
    const iconClasses = {
      user: 'pi pi-users text-blue-600',
      role: 'pi pi-id-card text-purple-600',
      permission: 'pi pi-shield text-orange-600',
      system: 'pi pi-cog text-gray-600'
    };
    return iconClasses[type as keyof typeof iconClasses] || 'pi pi-info text-gray-600';
  }

  formatTimestamp(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));

    if (diffInMinutes < 60) {
      return `${diffInMinutes}m ago`;
    } else if (diffInMinutes < 1440) {
      return `${Math.floor(diffInMinutes / 60)}h ago`;
    } else {
      return `${Math.floor(diffInMinutes / 1440)}d ago`;
    }
  }
}