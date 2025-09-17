import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { UserDto } from '../../models/api.models';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
  badge?: string;
  children?: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside 
      class="fixed inset-y-0 left-0 z-20 w-64 bg-white shadow-lg border-r border-secondary-200 transform transition-transform duration-300 lg:translate-x-0"
      [class.translate-x-0]="isOpen"
      [class.-translate-x-full]="!isOpen"
      style="top: 64px;">
      
      <!-- Overlay for mobile -->
      <div 
        *ngIf="isOpen"
        class="fixed inset-0 bg-black bg-opacity-50 lg:hidden z-10"
        (click)="close.emit()">
      </div>
      
      <!-- Sidebar Content -->
      <div class="relative z-20 h-full flex flex-col bg-white">
        <!-- User Info Section -->
        <div class="p-6 border-b border-secondary-200" *ngIf="user">
          <div class="flex items-center space-x-3">
            <div class="w-12 h-12 bg-gradient-to-r from-primary-500 to-primary-600 rounded-full flex items-center justify-center">
              <img 
                *ngIf="user.avatarUrl" 
                [src]="user.avatarUrl" 
                [alt]="user.displayName"
                class="w-12 h-12 rounded-full object-cover">
              <span 
                *ngIf="!user.avatarUrl" 
                class="text-white font-semibold text-lg">
                {{ getUserInitials(user.displayName) }}
              </span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-secondary-900 truncate">{{ user.displayName }}</p>
              <p class="text-xs text-secondary-500 truncate">{{ user.email }}</p>
              <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-primary-100 text-primary-800 mt-1">
                {{ user.roles[0] || 'Student' }}
              </span>
            </div>
          </div>
        </div>
        
        <!-- Navigation Menu -->
        <nav class="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
          <div *ngFor="let item of menuItems">
            <!-- Menu Item without children -->
            <a 
              *ngIf="!item.children && hasPermission(item)"
              [routerLink]="item.route"
              routerLinkActive="active"
              class="nav-item group">
              <div class="nav-icon" [innerHTML]="item.icon"></div>
              <span class="nav-label">{{ item.label }}</span>
              <span 
                *ngIf="item.badge" 
                class="ml-auto inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
                {{ item.badge }}
              </span>
            </a>
            
            <!-- Menu Item with children -->
            <div *ngIf="item.children && hasPermission(item)" class="space-y-1">
              <button 
                (click)="toggleSubmenu(item.label)"
                class="nav-item group w-full text-left">
                <div class="nav-icon" [innerHTML]="item.icon"></div>
                <span class="nav-label">{{ item.label }}</span>
                <svg 
                  class="ml-auto w-4 h-4 transition-transform duration-200"
                  [class.rotate-90]="openSubmenus[item.label]"
                  fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                </svg>
              </button>
              
              <!-- Submenu -->
              <div 
                *ngIf="openSubmenus[item.label]"
                class="ml-6 space-y-1 animate-slide-in">
                <a 
                  *ngFor="let child of item.children"
                  [routerLink]="child.route"
                  routerLinkActive="active"
                  class="nav-item-sub">
                  <div class="nav-icon-sm" [innerHTML]="child.icon"></div>
                  <span>{{ child.label }}</span>
                  <span 
                    *ngIf="child.badge" 
                    class="ml-auto inline-flex items-center px-1.5 py-0.5 rounded text-xs font-medium bg-primary-100 text-primary-800">
                    {{ child.badge }}
                  </span>
                </a>
              </div>
            </div>
          </div>
        </nav>
        
        <!-- Footer -->
        <div class="p-4 border-t border-secondary-200">
          <div class="text-center">
            <p class="text-xs text-secondary-500">StudyBridge v1.0.0</p>
            <p class="text-xs text-secondary-400 mt-1">© 2024 StudyBridge</p>
          </div>
        </div>
      </div>
    </aside>
  `,
  styles: [`
    .nav-item {
      @apply flex items-center w-full px-3 py-2.5 text-sm font-medium text-secondary-700 rounded-lg hover:bg-secondary-100 hover:text-secondary-900 transition-colors duration-200;
    }
    
    .nav-item.active {
      @apply bg-primary-50 text-primary-700 border-r-2 border-primary-500;
    }
    
    .nav-item-sub {
      @apply flex items-center w-full px-3 py-2 text-sm text-secondary-600 rounded-lg hover:bg-secondary-50 hover:text-secondary-900 transition-colors duration-200;
    }
    
    .nav-item-sub.active {
      @apply bg-primary-50 text-primary-600;
    }
    
    .nav-icon {
      @apply w-5 h-5 mr-3 text-secondary-500 group-hover:text-secondary-700;
    }
    
    .nav-icon-sm {
      @apply w-4 h-4 mr-3 text-secondary-400;
    }
    
    .nav-label {
      @apply flex-1;
    }
  `]
})
export class SidebarComponent {
  @Input() isOpen = false;
  @Input() user: UserDto | null = null;
  @Output() close = new EventEmitter<void>();

  openSubmenus: { [key: string]: boolean } = {};

  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v6a2 2 0 01-2 2H10a2 2 0 01-2-2V5z"></path></svg>',
      route: '/dashboard'
    },
    {
      label: 'Vocabulary',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path></svg>',
      route: '/vocabulary',
      badge: 'Coming Soon',
      children: [
        {
          label: 'Browse Words',
          icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>',
          route: '/vocabulary/browse'
        },
        {
          label: 'Learning Sets',
          icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path></svg>',
          route: '/vocabulary/sets'
        },
        {
          label: 'Categories',
          icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"></path></svg>',
          route: '/vocabulary/categories'
        }
      ]
    },
    {
      label: 'Learning',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"></path></svg>',
      route: '/learning',
      badge: 'Coming Soon'
    },
    {
      label: 'Progress',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path></svg>',
      route: '/progress',
      badge: 'Coming Soon'
    },
    {
      label: 'Profile',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>',
      route: '/profile'
    },
    {
      label: 'Settings',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path></svg>',
      route: '/settings'
    },
    {
      label: 'Admin',
      icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>',
      route: '/admin',
      roles: ['Administrator'],
      children: [
        {
          label: 'Users',
          icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path></svg>',
          route: '/admin/users'
        },
        {
          label: 'Roles',
          icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>',
          route: '/admin/roles'
        }
      ]
    }
  ];

  constructor(private router: Router) {
    // Close sidebar on route change (mobile)
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        if (window.innerWidth < 1024) {
          this.close.emit();
        }
      });
  }

  toggleSubmenu(label: string): void {
    this.openSubmenus[label] = !this.openSubmenus[label];
  }

  hasPermission(item: MenuItem): boolean {
    if (!item.roles || item.roles.length === 0) {
      return true;
    }
    
    if (!this.user) {
      return false;
    }
    
    return item.roles.some(role => this.user?.roles?.some(userRole => userRole.name === role));
  }

  getUserInitials(displayName: string): string {
    if (!displayName) return 'U';
    
    const names = displayName.trim().split(' ');
    if (names.length === 1) {
      return names[0].charAt(0).toUpperCase();
    }
    
    return (names[0].charAt(0) + names[names.length - 1].charAt(0)).toUpperCase();
  }
}
