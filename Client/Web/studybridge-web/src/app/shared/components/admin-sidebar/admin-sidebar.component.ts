import { Component, inject, computed, signal, Input, Output, EventEmitter, OnInit, OnDestroy, ElementRef, AfterViewInit } from '@angular/core';
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
export class AdminSidebarComponent implements OnInit, AfterViewInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  // Input for mobile open state
  @Input() isMobileOpen: boolean = false;

  // Output for mobile close event
  @Output() onCloseMobile = new EventEmitter<void>();

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

  ngOnInit(): void {
    this.setMobileHeight();
    window.addEventListener('resize', this.setMobileHeight.bind(this));
    window.addEventListener('orientationchange', this.setMobileHeight.bind(this));
  }

  ngAfterViewInit(): void {
    this.setMobileHeight();
  }

  ngOnDestroy(): void {
    window.removeEventListener('resize', this.setMobileHeight.bind(this));
    window.removeEventListener('orientationchange', this.setMobileHeight.bind(this));
  }

  // Set mobile height programmatically
  private setMobileHeight(): void {
    if (typeof window === 'undefined') return;
    
    // Update CSS custom property for all elements
    const vh = window.innerHeight;
    document.documentElement.style.setProperty('--mobile-vh', `${vh}px`);
    
    const isMobile = window.innerWidth <= 1023;
    if (isMobile && this.elementRef?.nativeElement) {
      const sidebar = this.elementRef.nativeElement.querySelector('.admin-sidebar');
      if (sidebar) {
        // Force height to actual viewport height with multiple approaches
        sidebar.style.setProperty('height', `${vh}px`, 'important');
        sidebar.style.setProperty('min-height', `${vh}px`, 'important');
        sidebar.style.setProperty('max-height', `${vh}px`, 'important');
        // Also set positioning to ensure full coverage
        sidebar.style.setProperty('top', '0px', 'important');
        sidebar.style.setProperty('bottom', '0px', 'important');
      }
    }
  }

  // Close mobile sidebar
  closeMobileSidebar(): void {
    this.onCloseMobile.emit();
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