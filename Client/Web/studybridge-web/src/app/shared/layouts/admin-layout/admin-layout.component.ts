import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { MenuItem } from 'primeng/api';
import { AuthService } from '../../services/auth.service';
import { MenuService } from '../../services/menu.service';
import { UserDto } from '../../models/api.models';
import { MenuItem as AppMenuItem } from '../../models/menu.models';

export interface AdminMenuItem {
  id: string;
  label: string;
  icon: string;
  route: string;
  permission?: string;
  badge?: string;
  children?: AdminMenuItem[];
  isActive?: boolean;
  isExpanded?: boolean;
}

export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  permissions: string[];
  avatar?: string;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ButtonModule,
    MenuModule,
    AvatarModule,
    BadgeModule,
    TooltipModule
  ],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent implements OnInit {
  // Signals for reactive state management
  currentUser = signal<AdminUser | null>(null);
  menuItems = signal<AdminMenuItem[]>([]);
  isSidebarCollapsed = signal(false);
  isMobileSidebarVisible = signal(false);
  isUserMenuVisible = signal(false);
  isLoading = signal(true);

  // Computed values
  userInitials = computed(() => {
    const user = this.currentUser();
    if (!user) return 'U';
    return user.displayName 
      ? user.displayName.split(' ').map(n => n[0]).join('').toUpperCase()
      : user.email[0].toUpperCase();
  });

  hasPermission = computed(() => {
    return (permission: string) => {
      const user = this.currentUser();
      return user?.permissions.includes(permission) || false;
    };
  });

  // User menu items
  userMenuItems: MenuItem[] = [
    {
      label: 'Profile',
      icon: 'pi pi-user',
      command: () => this.navigateToProfile()
    },
    {
      label: 'Settings',
      icon: 'pi pi-cog',
      command: () => this.navigateToSettings()
    },
    { separator: true },
    {
      label: 'Logout',
      icon: 'pi pi-sign-out',
      command: () => this.logout()
    }
  ];

  constructor(
    private authService: AuthService,
    private menuService: MenuService,
    private router: Router
  ) {}

  async ngOnInit() {
    try {
      this.isLoading.set(true);
      await this.initializeAdminData();
    } catch (error) {
      console.error('Failed to initialize admin layout:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async initializeAdminData() {
    // Get current user from auth service
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    // Load admin user data with permissions
    const adminUser = await this.loadAdminUser(user);
    this.currentUser.set(adminUser);

    // Load dynamic menu from backend API
    await this.loadUserMenusFromApi();
  }

  private async loadAdminUser(user: UserDto): Promise<AdminUser> {
    // TODO: Replace with actual API call to get user permissions and roles
    // For now, simulate based on user info
    const roleNames = user.roles?.map(role => role.name) || ['SuperAdmin']; // Default to SuperAdmin for testing
    const permissions = this.getPermissionsForRoles(roleNames);

    return {
      id: user.id || '',
      email: user.email || '',
      displayName: user.displayName || user.email || '',
      roles: roleNames,
      permissions
    };
  }

  private async loadUserMenusFromApi() {
    try {
      // Load admin menus from API using MenuService
      const adminMenus = await this.menuService.loadAdminMenus();
      
      if (adminMenus.length > 0) {
        const convertedMenus = this.convertAppMenuItemsToAdminMenuItems(adminMenus);
        this.menuItems.set(convertedMenus);
      } else {
        // Fallback to static menu if API fails
        this.loadFallbackMenu();
      }
    } catch (error) {
      console.error('Failed to load admin menus from API:', error);
      // Fallback to static menu
      this.loadFallbackMenu();
    }
  }

  private loadFallbackMenu() {
    const user = this.currentUser();
    if (user) {
      const menu = this.buildAdminMenu(user.permissions);
      this.menuItems.set(menu);
    }
  }

  private convertBackendMenusToAdminMenuItems(backendMenus: any[]): AdminMenuItem[] {
    return backendMenus.map(menu => ({
      id: menu.name,
      label: menu.displayName,
      icon: menu.icon || 'pi pi-circle',
      route: menu.route || '',
      permission: menu.requiredPermissions?.[0], // Use first permission as primary
      children: menu.children ? this.convertBackendMenusToAdminMenuItems(menu.children) : undefined,
      isActive: false,
      isExpanded: false
    }));
  }

  private convertAppMenuItemsToAdminMenuItems(appMenus: AppMenuItem[]): AdminMenuItem[] {
    return appMenus.map(menu => ({
      id: menu.name,
      label: menu.displayName,
      icon: menu.icon || 'pi pi-circle',
      route: menu.route || '',
      permission: menu.requiredPermissions?.[0], // Use first permission as primary
      children: menu.children ? this.convertAppMenuItemsToAdminMenuItems(menu.children) : undefined,
      isActive: false,
      isExpanded: false
    }));
  }

  private getPermissionsForRoles(roles: string[]): string[] {
    // TODO: Replace with actual API call to get permissions
    // For now, simulate based on roles
    const allPermissions: Record<string, string[]> = {
      'SuperAdmin': [
        'users.view', 'users.create', 'users.edit', 'users.delete', 'users.manage',
        'roles.view', 'roles.create', 'roles.edit', 'roles.delete', 'roles.manage',
        'permissions.view', 'permissions.create', 'permissions.edit', 'permissions.delete', 'permissions.manage',
        'menus.view', 'menus.create', 'menus.edit', 'menus.delete', 'menus.manage',
        'system.view', 'system.manage', 'system.logs', 'system.settings',
        'content.view', 'content.create', 'content.edit', 'content.delete', 'content.manage',
        'financials.view', 'financials.manage', 'reports.view', 'analytics.view'
      ],
      'Admin': [
        'users.view', 'users.create', 'users.edit', 'users.delete',
        'roles.view', 'roles.create', 'roles.edit', 
        'permissions.view', 'content.view', 'content.create', 'content.edit',
        'system.view', 'reports.view'
      ],
      'Finance': [
        'users.view', 'financials.view', 'financials.manage', 'reports.view'
      ],
      'ContentManager': [
        'users.view', 'content.view', 'content.create', 'content.edit', 'content.delete'
      ],
      'User': ['dashboard.view']
    };

    const userPermissions = new Set<string>();
    roles.forEach(role => {
      const rolePermissions = allPermissions[role] || [];
      rolePermissions.forEach(permission => userPermissions.add(permission));
    });

    return Array.from(userPermissions);
  }

  private buildAdminMenu(permissions: string[]): AdminMenuItem[] {
    const allMenuItems: AdminMenuItem[] = [
      {
        id: 'dashboard',
        label: 'Dashboard',
        icon: 'pi pi-home',
        route: '/admin/dashboard',
        permission: 'dashboard.view'
      },
      {
        id: 'admin-management',
        label: 'Admin Management',
        icon: 'pi pi-sitemap',
        route: '/admin/management',
        permission: 'admin.view'
      },
      {
        id: 'user-management',
        label: 'User Management',
        icon: 'pi pi-users',
        route: '/admin/users',
        permission: 'users.view',
        children: [
          {
            id: 'users-list',
            label: 'All Users',
            icon: 'pi pi-list',
            route: '/admin/users',
            permission: 'users.view'
          },
          {
            id: 'users-create',
            label: 'Add User',
            icon: 'pi pi-plus',
            route: '/admin/users/create',
            permission: 'users.create'
          },
          {
            id: 'users-roles',
            label: 'User Roles',
            icon: 'pi pi-key',
            route: '/admin/users/roles',
            permission: 'roles.view'
          }
        ]
      },
      {
        id: 'role-management',
        label: 'Role Management',
        icon: 'pi pi-key',
        route: '/admin/roles',
        permission: 'roles.view',
        children: [
          {
            id: 'roles-list',
            label: 'All Roles',
            icon: 'pi pi-list',
            route: '/admin/roles',
            permission: 'roles.view'
          },
          {
            id: 'roles-create',
            label: 'Create Role',
            icon: 'pi pi-plus',
            route: '/admin/roles/create',
            permission: 'roles.create'
          }
        ]
      },
      {
        id: 'permission-management',
        label: 'Permissions',
        icon: 'pi pi-shield',
        route: '/admin/permissions',
        permission: 'permissions.view',
        children: [
          {
            id: 'permissions-list',
            label: 'All Permissions',
            icon: 'pi pi-list',
            route: '/admin/permissions',
            permission: 'permissions.view'
          },
          {
            id: 'permissions-create',
            label: 'Create Permission',
            icon: 'pi pi-plus',
            route: '/admin/permissions/create',
            permission: 'permissions.create'
          }
        ]
      },
      {
        id: 'content-management',
        label: 'Content',
        icon: 'pi pi-file-edit',
        route: '/admin/content',
        permission: 'content.view',
        children: [
          {
            id: 'content-vocabulary',
            label: 'Vocabulary',
            icon: 'pi pi-book',
            route: '/admin/content/vocabulary',
            permission: 'content.view'
          },
          {
            id: 'content-categories',
            label: 'Categories',
            icon: 'pi pi-tags',
            route: '/admin/content/categories',
            permission: 'content.view'
          }
        ]
      },
      {
        id: 'financial-management',
        label: 'Financials',
        icon: 'pi pi-dollar',
        route: '/admin/financials',
        permission: 'financials.view',
        children: [
          {
            id: 'financials-overview',
            label: 'Overview',
            icon: 'pi pi-chart-bar',
            route: '/admin/financials',
            permission: 'financials.view'
          },
          {
            id: 'financials-subscriptions',
            label: 'Subscriptions',
            icon: 'pi pi-credit-card',
            route: '/admin/financials/subscriptions',
            permission: 'financials.view'
          },
          {
            id: 'financials-reports',
            label: 'Reports',
            icon: 'pi pi-chart-line',
            route: '/admin/financials/reports',
            permission: 'reports.view'
          }
        ]
      },
      {
        id: 'system-management',
        label: 'System',
        icon: 'pi pi-cog',
        route: '/admin/system',
        permission: 'system.view',
        children: [
          {
            id: 'system-settings',
            label: 'Settings',
            icon: 'pi pi-sliders-h',
            route: '/admin/system/settings',
            permission: 'system.manage'
          },
          {
            id: 'system-logs',
            label: 'Logs',
            icon: 'pi pi-file',
            route: '/admin/system/logs',
            permission: 'system.logs'
          },
          {
            id: 'system-analytics',
            label: 'Analytics',
            icon: 'pi pi-chart-pie',
            route: '/admin/system/analytics',
            permission: 'analytics.view'
          }
        ]
      }
    ];

    return this.filterMenuByPermissions(allMenuItems, permissions);
  }

  private filterMenuByPermissions(items: AdminMenuItem[], permissions: string[]): AdminMenuItem[] {
    return items
      .filter(item => !item.permission || permissions.includes(item.permission))
      .map(item => ({
        ...item,
        children: item.children 
          ? this.filterMenuByPermissions(item.children, permissions)
          : undefined
      }))
      .filter(item => !item.children || item.children.length > 0);
  }

  // Event handlers
  toggleSidebar() {
    this.isSidebarCollapsed.set(!this.isSidebarCollapsed());
  }

  toggleMobileSidebar() {
    this.isMobileSidebarVisible.set(!this.isMobileSidebarVisible());
  }

  toggleUserMenu() {
    this.isUserMenuVisible.set(!this.isUserMenuVisible());
  }

  expandMenuItem(itemId: string) {
    const items = this.menuItems();
    const updatedItems = this.toggleMenuItemExpanded(items, itemId);
    this.menuItems.set(updatedItems);
  }

  private toggleMenuItemExpanded(items: AdminMenuItem[], targetId: string): AdminMenuItem[] {
    return items.map(item => {
      if (item.id === targetId) {
        return { ...item, isExpanded: !item.isExpanded };
      }
      if (item.children) {
        return {
          ...item,
          children: this.toggleMenuItemExpanded(item.children, targetId)
        };
      }
      return item;
    });
  }

  navigateToProfile() {
    this.router.navigate(['/admin/profile']);
    this.isUserMenuVisible.set(false);
  }

  navigateToSettings() {
    this.router.navigate(['/admin/settings']);
    this.isUserMenuVisible.set(false);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  // Helper methods for templates
  hasChildren(item: AdminMenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  getMenuItemClass(item: AdminMenuItem): string {
    let classes = 'menu-item';
    if (item.isActive) classes += ' active';
    if (item.isExpanded) classes += ' expanded';
    return classes;
  }

  onMenuItemClick(item: AdminMenuItem, event: Event) {
    if (this.hasChildren(item)) {
      event.preventDefault();
      this.expandMenuItem(item.id);
    } else {
      // Close mobile sidebar on navigation
      this.isMobileSidebarVisible.set(false);
    }
  }

  // Handle outside clicks to close user menu
  onClickOutside(event: Event) {
    if (this.isUserMenuVisible()) {
      this.isUserMenuVisible.set(false);
    }
  }
}