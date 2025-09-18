import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MenuService } from '../../services/menu.service';
import { UserDto } from '../../models/api.models';
import { MenuItem as AppMenuItem } from '../../models/menu.models';
import { AdminSidebarComponent } from '../../components/admin-sidebar/admin-sidebar.component';
import { UserMenuComponent } from '../../components/user-menu/user-menu.component';

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
    AdminSidebarComponent,
    UserMenuComponent
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

  // User menu items - handled by sidebar component now

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
    // Use actual user roles from API response - no hardcoded defaults
    const roleNames = user.roles?.map(role => role.name) || [];
    
    // If no roles are found, this might indicate a data issue
    if (roleNames.length === 0) {
      console.warn('No roles found for admin user:', user.email);
    }
    
    const permissions = await this.loadUserPermissions(user);

    return {
      id: user.id || '',
      email: user.email || '',
      displayName: user.displayName || user.email || '',
      roles: roleNames,
      permissions
    };
  }

  private async loadUserPermissions(user: UserDto): Promise<string[]> {
    try {
      // TODO: Implement API call to get user-specific permissions
      // For now, derive permissions from roles
      const roleNames = user.roles?.map(role => role.name) || [];
      return this.getPermissionsForRoles(roleNames);
    } catch (error) {
      console.error('Failed to load user permissions:', error);
      return [];
    }
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
    // Return empty array - permissions should be loaded from API
    // This is a temporary placeholder until proper API integration
    console.warn('Using placeholder permissions. Implement API call to get role permissions.');
    
    // Basic fallback permissions to prevent complete failure
    if (roles.some(role => role.toLowerCase().includes('admin') || role.toLowerCase().includes('super'))) {
      return ['users.view', 'content.view', 'system.view'];
    }
    
    return [];
  }

  private buildAdminMenu(permissions: string[]): AdminMenuItem[] {
    console.warn('buildAdminMenu: This method should be replaced with dynamic menu loading from MenuService');
    
    // This is a minimal fallback menu - the real menu should come from the API
    const fallbackMenuItems: AdminMenuItem[] = [
      {
        id: 'dashboard',
        label: 'Dashboard',
        icon: 'pi pi-home',
        route: '/admin/dashboard',
        permission: 'dashboard.view'
      },
      {
        id: 'user-management',
        label: 'User Management',
        icon: 'pi pi-users',
        route: '/admin/users',
        permission: 'users.view'
      }
    ];

    return this.filterMenuByPermissions(fallbackMenuItems, permissions);
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