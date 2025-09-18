import { Component, computed, signal, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserDto } from '../../models/api.models';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-menu.component.html',
  styleUrls: ['./user-menu.component.scss']
})
export class UserMenuComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Reactive signals
  isMenuOpen = signal(false);
  user = computed(() => this.authService.getCurrentUser());

  // Computed values
  userInitials = computed(() => {
    const currentUser = this.user();
    if (!currentUser?.displayName) return 'U';
    
    return currentUser.displayName
      .split(' ')
      .map((name: string) => name.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2);
  });

  userRole = computed(() => {
    const currentUser = this.user();
    if (!currentUser) return 'User';
    
    // Determine role based on IsPublicUser flag
    return currentUser.isPublicUser ? 'Student' : 'Admin';
  });

  // Toggle dropdown menu
  toggleMenu(): void {
    this.isMenuOpen.update(isOpen => !isOpen);
  }

  // Close menu
  closeMenu(): void {
    this.isMenuOpen.set(false);
  }

  // Navigate to profile
  navigateToProfile(): void {
    const currentUser = this.user();
    if (currentUser?.isPublicUser) {
      this.router.navigate(['/public/profile']);
    } else {
      this.router.navigate(['/admin/profile']);
    }
    this.closeMenu();
  }

  // Navigate to settings
  navigateToSettings(): void {
    const currentUser = this.user();
    if (currentUser?.isPublicUser) {
      this.router.navigate(['/public/settings']);
    } else {
      this.router.navigate(['/admin/settings']);
    }
    this.closeMenu();
  }

  // Handle logout
  logout(): void {
    this.authService.logout();
    this.closeMenu();
  }

  // Close menu when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    const menuElement = target.closest('.user-menu-container');
    
    if (!menuElement && this.isMenuOpen()) {
      this.closeMenu();
    }
  }

  // Handle keyboard navigation
  @HostListener('keydown.escape')
  onEscapeKey(): void {
    if (this.isMenuOpen()) {
      this.closeMenu();
    }
  }
}