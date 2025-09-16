import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.models';

export interface PublicMenuItem {
  id: string;
  label: string;
  icon: string;
  route: string;
  badge?: string;
}

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ],
  template: `
    <div class="min-h-screen bg-gradient-to-br from-primary-50 to-secondary-100">
      <!-- Header -->
      <header class="bg-white shadow-lg border-b border-primary-200">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex items-center justify-between h-16">
            <!-- Logo -->
            <div class="flex items-center space-x-3">
              <div class="w-10 h-10 bg-gradient-to-r from-primary-500 to-primary-600 rounded-lg flex items-center justify-center">
                <span class="text-white font-bold text-xl">S</span>
              </div>
              <h1 class="text-2xl font-heading font-bold text-primary-900">StudyBridge</h1>
            </div>
            
            <!-- Top Navigation -->
            <nav class="hidden md:flex items-center space-x-8">
              <a 
                *ngFor="let item of topMenuItems()"
                [routerLink]="item.route"
                routerLinkActive="text-primary-600 font-semibold"
                class="flex items-center px-3 py-2 text-sm font-medium text-secondary-700 hover:text-primary-600 transition-colors duration-200">
                <span [innerHTML]="item.icon" class="w-5 h-5 mr-2"></span>
                {{ item.label }}
                <span 
                  *ngIf="item.badge" 
                  class="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
                  {{ item.badge }}
                </span>
              </a>
            </nav>

            <!-- User Menu -->
            <div class="flex items-center space-x-4">
              <div class="relative" *ngIf="currentUser()">
                <button 
                  (click)="toggleUserMenu()"
                  class="flex items-center space-x-2 p-2 rounded-lg hover:bg-primary-50 transition-colors">
                  <div class="w-8 h-8 bg-primary-500 rounded-full flex items-center justify-center">
                    <span class="text-white font-semibold text-sm">
                      {{ userInitials() }}
                    </span>
                  </div>
                  <div class="hidden sm:block text-left">
                    <p class="text-sm font-medium text-secondary-900">{{ currentUser()?.displayName }}</p>
                    <p class="text-xs text-primary-600">{{ currentUser()?.roles?.[0] || 'Student' }}</p>
                  </div>
                  <svg class="w-4 h-4 text-secondary-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
                  </svg>
                </button>
                
                <!-- User Dropdown -->
                <div 
                  *ngIf="isUserMenuVisible()"
                  class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-secondary-200 py-1 z-50">
                  <a 
                    routerLink="/public/profile" 
                    class="flex items-center px-4 py-2 text-sm text-secondary-700 hover:bg-secondary-50">
                    <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                    </svg>
                    Profile
                  </a>
                  <a 
                    routerLink="/public/settings" 
                    class="flex items-center px-4 py-2 text-sm text-secondary-700 hover:bg-secondary-50">
                    <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    </svg>
                    Settings
                  </a>
                  <hr class="my-1 border-secondary-200">
                  <button 
                    (click)="logout()"
                    class="flex items-center w-full px-4 py-2 text-sm text-error-600 hover:bg-error-50">
                    <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                    </svg>
                    Sign Out
                  </button>
                </div>
              </div>

              <!-- Mobile Menu Toggle -->
              <button 
                (click)="toggleMobileMenu()"
                class="md:hidden p-2 rounded-lg text-secondary-600 hover:bg-secondary-100">
                <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
                </svg>
              </button>
            </div>
          </div>
        </div>

        <!-- Mobile Menu -->
        <div 
          *ngIf="isMobileMenuVisible()"
          class="md:hidden bg-white border-t border-secondary-200">
          <nav class="px-4 py-4 space-y-2">
            <a 
              *ngFor="let item of topMenuItems()"
              [routerLink]="item.route"
              routerLinkActive="bg-primary-50 text-primary-700"
              class="flex items-center px-3 py-2 text-sm font-medium text-secondary-700 rounded-lg hover:bg-secondary-50">
              <span [innerHTML]="item.icon" class="w-5 h-5 mr-3"></span>
              {{ item.label }}
              <span 
                *ngIf="item.badge" 
                class="ml-auto inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
                {{ item.badge }}
              </span>
            </a>
          </nav>
        </div>
      </header>

      <!-- Main Content -->
      <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <router-outlet></router-outlet>
      </main>

      <!-- Footer -->
      <footer class="bg-white border-t border-secondary-200 mt-16">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div class="text-center">
            <p class="text-sm text-secondary-500">© 2024 StudyBridge. All rights reserved.</p>
            <p class="text-xs text-secondary-400 mt-2">IELTS Vocabulary Learning Platform</p>
          </div>
        </div>
      </footer>
    </div>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent implements OnInit {
  // Signals for reactive state management
  currentUser = signal<User | null>(null);
  topMenuItems = signal<PublicMenuItem[]>([]);
  isUserMenuVisible = signal(false);
  isMobileMenuVisible = signal(false);
  isLoading = signal(true);

  // Computed values
  userInitials = computed(() => {
    const user = this.currentUser();
    if (!user) return 'U';
    return user.displayName 
      ? user.displayName.split(' ').map(n => n[0]).join('').toUpperCase()
      : user.email[0].toUpperCase();
  });

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async ngOnInit() {
    try {
      this.isLoading.set(true);
      await this.initializePublicData();
    } catch (error) {
      console.error('Failed to initialize public layout:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async initializePublicData() {
    // Get current user from auth service
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }

    // Check if user should be in admin area
    if (this.authService.isAdminUser()) {
      this.router.navigate(['/admin/dashboard']);
      return;
    }

    this.currentUser.set(user);
    this.loadPublicMenus();
  }

  private loadPublicMenus() {
    // Define public menu items - only top-level navigation for users
    const publicMenus: PublicMenuItem[] = [
      {
        id: 'dashboard',
        label: 'Dashboard',
        icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v6a2 2 0 01-2 2H10a2 2 0 01-2-2V5z"></path></svg>',
        route: '/public/dashboard'
      },
      {
        id: 'vocabulary',
        label: 'Vocabulary',
        icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path></svg>',
        route: '/public/vocabulary',
        badge: 'Coming Soon'
      },
      {
        id: 'learning',
        label: 'Learning',
        icon: '<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"></path></svg>',
        route: '/public/learning',
        badge: 'Coming Soon'
      }
    ];

    this.topMenuItems.set(publicMenus);
  }

  // Event handlers
  toggleUserMenu() {
    this.isUserMenuVisible.set(!this.isUserMenuVisible());
  }

  toggleMobileMenu() {
    this.isMobileMenuVisible.set(!this.isMobileMenuVisible());
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  // Handle outside clicks to close menus
  onClickOutside(event: Event) {
    if (this.isUserMenuVisible()) {
      this.isUserMenuVisible.set(false);
    }
    if (this.isMobileMenuVisible()) {
      this.isMobileMenuVisible.set(false);
    }
  }
}