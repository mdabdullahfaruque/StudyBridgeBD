import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserDto } from '../../models/api.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="bg-white shadow-sm border-b border-secondary-200 fixed w-full top-0 z-30">
      <div class="flex items-center justify-between px-4 py-3">
        <!-- Left Side -->
        <div class="flex items-center space-x-4">
          <!-- Sidebar Toggle -->
          <button 
            (click)="toggleSidebar.emit()"
            class="p-2 rounded-lg text-secondary-600 hover:bg-secondary-100 lg:hidden">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
          
          <!-- Logo -->
          <div class="flex items-center space-x-3">
            <div class="w-8 h-8 bg-gradient-to-r from-primary-500 to-primary-600 rounded-lg flex items-center justify-center">
              <span class="text-white font-bold text-lg">S</span>
            </div>
            <h1 class="text-xl font-heading font-semibold text-secondary-900 hidden sm:block">
              StudyBridge
            </h1>
          </div>
        </div>
        
        <!-- Right Side -->
        <div class="flex items-center space-x-4">
          <!-- Search (Hidden on small screens) -->
          <div class="hidden md:block">
            <div class="relative">
              <input 
                type="text"
                placeholder="Search..."
                class="input w-64 pl-10 pr-4 py-2 text-sm">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="w-4 h-4 text-secondary-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
                </svg>
              </div>
            </div>
          </div>
          
          <!-- Notifications -->
          <button class="p-2 rounded-lg text-secondary-600 hover:bg-secondary-100 relative">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-5 5-5-5h5V7a5 5 0 0110 0v10z"></path>
            </svg>
            <span class="absolute top-1 right-1 w-2 h-2 bg-error-500 rounded-full"></span>
          </button>
          
          <!-- User Menu -->
          <div class="relative" *ngIf="user">
            <button 
              (click)="toggleUserMenu()"
              class="flex items-center space-x-2 p-2 rounded-lg hover:bg-secondary-100 transition-colors">
              <div class="w-8 h-8 bg-primary-100 rounded-full flex items-center justify-center">
                <img 
                  *ngIf="user.avatarUrl" 
                  [src]="user.avatarUrl" 
                  [alt]="user.displayName"
                  class="w-8 h-8 rounded-full object-cover">
                <span 
                  *ngIf="!user.avatarUrl" 
                  class="text-primary-600 font-medium text-sm">
                  {{ getUserInitials(user.displayName) }}
                </span>
              </div>
              <div class="hidden sm:block text-left">
                <p class="text-sm font-medium text-secondary-900">{{ user.displayName }}</p>
                <p class="text-xs text-secondary-500">{{ user.roles[0] || 'Student' }}</p>
              </div>
              <svg class="w-4 h-4 text-secondary-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
              </svg>
            </button>
            
            <!-- User Dropdown Menu -->
            <div 
              *ngIf="userMenuOpen"
              class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-secondary-200 py-1 z-50">
              <a 
                routerLink="/profile" 
                class="flex items-center px-4 py-2 text-sm text-secondary-700 hover:bg-secondary-50">
                <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
                </svg>
                Profile
              </a>
              <a 
                routerLink="/settings" 
                class="flex items-center px-4 py-2 text-sm text-secondary-700 hover:bg-secondary-50">
                <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                </svg>
                Settings
              </a>
              <hr class="my-1 border-secondary-200">
              <button 
                (click)="handleLogout()"
                class="flex items-center w-full px-4 py-2 text-sm text-error-600 hover:bg-error-50">
                <svg class="w-4 h-4 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
                </svg>
                Sign Out
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>
  `,
  styles: [`
    :host {
      display: block;
      height: 64px;
    }
  `]
})
export class HeaderComponent {
  @Input() user: UserDto | null = null;
  @Output() logout = new EventEmitter<void>();
  @Output() toggleSidebar = new EventEmitter<void>();

  userMenuOpen = false;

  toggleUserMenu(): void {
    this.userMenuOpen = !this.userMenuOpen;
  }

  handleLogout(): void {
    this.userMenuOpen = false;
    this.logout.emit();
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
