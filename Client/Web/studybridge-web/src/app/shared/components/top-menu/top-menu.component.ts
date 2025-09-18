import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserDto } from '../../models/api.models';

@Component({
  selector: 'app-top-menu',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './top-menu.component.html',
  styleUrl: './top-menu.component.scss'
})
export class TopMenuComponent {
  @Input() user: UserDto | null = null;
  @Input() menuItems: any[] = [];
  @Output() logout = new EventEmitter<void>();
  @Output() profileClick = new EventEmitter<void>();

  isUserMenuOpen = false;
  isMobileMenuOpen = false;

  onLogout(): void {
    this.logout.emit();
      this.toggleUserMenu(); // Close dropdown after logout
  }

  onProfileClick(): void {
    this.profileClick.emit();
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }

  getUserInitials(): string {
    if (!this.user?.displayName) return 'U';
    return this.user.displayName
      .split(' ')
      .map(name => name.charAt(0))
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}