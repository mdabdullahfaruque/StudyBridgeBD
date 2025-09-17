import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserDto } from '../../models/api.models';

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
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent implements OnInit {
  // Signals for reactive state management
  currentUser = signal<UserDto | null>(null);
  topMenuItems = signal<PublicMenuItem[]>([]);
  isUserMenuVisible = signal(false);
  isMobileMenuVisible = signal(false);
  isVocabularyMenuOpen = signal(false);
  isLoading = signal(true);

  // Computed values
  userInitials = computed(() => {
    const user = this.currentUser();
    if (!user) return 'U';
    return user.displayName 
      ? user.displayName.split(' ').map((n: string) => n[0]).join('').toUpperCase()
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

  toggleVocabularyMenu() {
    this.isVocabularyMenuOpen.set(!this.isVocabularyMenuOpen());
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