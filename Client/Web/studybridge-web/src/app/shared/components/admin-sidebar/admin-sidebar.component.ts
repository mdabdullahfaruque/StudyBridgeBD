import { Component, inject, computed, signal, Input, Output, EventEmitter, OnInit, OnDestroy, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MenuService } from '../../services/menu.service';
import { UserDto } from '../../models/api.models';
import { MenuItem } from '../../models/menu.models';

interface MenuItemDisplay {
  label: string;
  icon: string;
  routerLink?: string;
  badge?: string;
  expanded?: boolean;
  children?: MenuItemDisplay[];
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
  private menuService = inject(MenuService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  // Input for mobile open state
  @Input() isMobileOpen: boolean = false;

  // Output for mobile close event
  @Output() onCloseMobile = new EventEmitter<void>();

  // Reactive signals
  isCollapsed = signal(false);
  user = computed(() => this.authService.getCurrentUser());
  
  // Menu signals from MenuService
  menuItems = this.menuService.adminMenusSignal;
  isLoadingMenus = this.menuService.isLoadingSignal;
  
  // Internal UI state for expanded items
  private expandedItems = signal<Set<string>>(new Set());
  
  // Error state for menu loading
  menuError = signal<string | null>(null);

  // Toggle sidebar collapse
  toggleSidebar(): void {
    this.isCollapsed.update(collapsed => !collapsed);
  }

  ngOnInit(): void {
    this.setMobileHeight();
    window.addEventListener('resize', this.setMobileHeight.bind(this));
    window.addEventListener('orientationchange', this.setMobileHeight.bind(this));
    
    // Load admin menus
    this.loadMenus();
  }

  private async loadMenus(): Promise<void> {
    try {
      this.menuError.set(null);
      await this.menuService.loadAdminMenus();
    } catch (error) {
      console.error('Failed to load admin menus:', error);
      this.menuError.set('Failed to load menu items. Please refresh the page.');
      // MenuService will provide fallback menus if API fails
    }
  }

  // Retry loading menus
  retryLoadMenus(): void {
    this.loadMenus();
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
    if (item.children && item.children.length > 0) {
      this.toggleExpandedItem(item.id);
    } else if (item.route) {
      this.router.navigate([item.route]);
    }
  }

  // Toggle expanded state for menu items
  toggleExpandedItem(itemId: string): void {
    const currentExpanded = this.expandedItems();
    const newExpanded = new Set(currentExpanded);
    
    if (newExpanded.has(itemId)) {
      newExpanded.delete(itemId);
    } else {
      newExpanded.add(itemId);
    }
    
    this.expandedItems.set(newExpanded);
  }

  // Check if menu item is expanded
  isItemExpanded(itemId: string): boolean {
    return this.expandedItems().has(itemId);
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

  // Check if user has permission to access a menu item
  // Note: The API already filters menus based on user permissions,
  // so menus returned from the API are already accessible to the current user
  hasPermission(item: MenuItem): boolean {
    return this.menuService.hasMenuPermission(item);
  }

  // TrackBy function for performance optimization
  trackByItemId(index: number, item: MenuItem): string {
    return item.id;
  }
}