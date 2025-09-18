import { Component, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserDto } from '../../models/api.models';

interface MenuItem {
  label: string;
  icon: string;
  routerLink?: string;
  badge?: string;
  expanded?: boolean;
  children?: MenuItem[];
}

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-sidebar.component.html',
  styleUrls: ['./admin-sidebar.component.scss']
})
export class AdminSidebarComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Reactive signals
  isCollapsed = signal(false);
  user = computed(() => this.authService.getCurrentUser());
  menuItems = computed<MenuItem[]>(() => {
    const user = this.user();
    if (!user) return [];
    
    return this.getMenuItemsForUser(user);
  });

  // Toggle sidebar collapse
  toggleSidebar(): void {
    this.isCollapsed.update(collapsed => !collapsed);
  }

  // Handle menu item click
  onMenuClick(item: MenuItem): void {
    if (item.children) {
      item.expanded = !item.expanded;
    } else if (item.routerLink) {
      this.router.navigate([item.routerLink]);
    }
  }

  // Handle logout
  onLogout(): void {
    this.authService.logout();
  }

  // Get user initials for avatar
  getUserInitials(): string {
    const user = this.user();
    if (!user?.displayName) return 'U';
    
    return user.displayName
      .split(' ')
      .map((name: string) => name.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  // Get menu items based on user role
  private getMenuItemsForUser(user: UserDto): MenuItem[] {
    const baseMenuItems: MenuItem[] = [
      {
        label: 'Dashboard',
        icon: 'pi pi-home',
        routerLink: '/admin/dashboard'
      },
      {
        label: 'User Management',
        icon: 'pi pi-users',
        children: [
          {
            label: 'All Users',
            icon: 'pi pi-list',
            routerLink: '/admin/users'
          },
          {
            label: 'User Roles',
            icon: 'pi pi-shield',
            routerLink: '/admin/roles'
          }
        ]
      },
      {
        label: 'Vocabulary',
        icon: 'pi pi-book',
        children: [
          {
            label: 'Word Management',
            icon: 'pi pi-pencil',
            routerLink: '/admin/vocabulary/words'
          },
          {
            label: 'Categories',
            icon: 'pi pi-tags',
            routerLink: '/admin/vocabulary/categories'
          },
          {
            label: 'Import/Export',
            icon: 'pi pi-upload',
            routerLink: '/admin/vocabulary/import-export'
          }
        ]
      },
      {
        label: 'Learning Progress',
        icon: 'pi pi-chart-line',
        children: [
          {
            label: 'User Progress',
            icon: 'pi pi-chart-bar',
            routerLink: '/admin/progress/users'
          },
          {
            label: 'Analytics',
            icon: 'pi pi-analytics',
            routerLink: '/admin/progress/analytics'
          }
        ]
      },
      {
        label: 'Reports',
        icon: 'pi pi-file-pdf',
        children: [
          {
            label: 'Usage Reports',
            icon: 'pi pi-calendar',
            routerLink: '/admin/reports/usage'
          },
          {
            label: 'Performance Reports',
            icon: 'pi pi-chart-pie',
            routerLink: '/admin/reports/performance'
          }
        ]
      }
    ];

    console.warn('AdminSidebarComponent: This component uses hardcoded menus. Replace with MenuService integration.');
    
    // Return basic admin menu including role management
    return [
      {
        label: 'Dashboard',
        icon: 'pi pi-home',
        routerLink: '/admin/dashboard'
      },
      {
        label: 'User Management',
        icon: 'pi pi-users',
        children: [
          {
            label: 'All Users',
            icon: 'pi pi-list',
            routerLink: '/admin/users'
          },
          {
            label: 'User Roles',
            icon: 'pi pi-shield',
            routerLink: '/admin/roles'
          }
        ]
      },
      {
        label: 'Role Management',
        icon: 'pi pi-key',
        routerLink: '/admin/roles'
      }
    ];
  }
}