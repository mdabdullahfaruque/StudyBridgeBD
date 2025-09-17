import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { Subject, takeUntil, filter } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

export interface LayoutConfig {
  showHeader?: boolean;
  showSidebar?: boolean;
  showFooter?: boolean;
  sidebarType?: 'admin' | 'public' | 'minimal';
  containerClass?: string;
}

@Component({
  selector: 'app-base-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SidebarComponent
  ],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <!-- Header -->
      <app-header 
        *ngIf="config.showHeader"
        [user]="currentUser" 
        [layoutType]="config.sidebarType || 'public'"
        (logout)="handleLogout()"
        (toggleSidebar)="toggleSidebar()">
      </app-header>
      
      <!-- Main Content Area -->
      <div class="flex">
        <!-- Sidebar -->
        <app-sidebar 
          *ngIf="config.showSidebar"
          [isOpen]="sidebarOpen"
          [user]="currentUser"
          [sidebarType]="config.sidebarType || 'public'"
          (close)="closeSidebar()">
        </app-sidebar>
        
        <!-- Main Content -->
        <main 
          class="flex-1 transition-all duration-300"
          [class]="getMainContentClass()">
          <div [class]="config.containerClass || 'p-6'">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
      
      <!-- Footer -->
      <footer *ngIf="config.showFooter" class="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto py-4 px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between items-center">
            <p class="text-sm text-gray-500 dark:text-gray-400">
              © 2025 StudyBridge. All rights reserved.
            </p>
            <div class="flex space-x-4 text-sm text-gray-500 dark:text-gray-400">
              <a href="#" class="hover:text-gray-700 dark:hover:text-gray-300">Privacy</a>
              <a href="#" class="hover:text-gray-700 dark:hover:text-gray-300">Terms</a>
              <a href="#" class="hover:text-gray-700 dark:hover:text-gray-300">Support</a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  `
})
export class BaseLayoutComponent implements OnInit, OnDestroy {
  @Input() config: LayoutConfig = {
    showHeader: true,
    showSidebar: true,
    showFooter: false,
    sidebarType: 'public',
    containerClass: 'p-6'
  };

  currentUser: any = null;
  sidebarOpen = true;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to current user
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });

    // Handle sidebar on mobile
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        if (window.innerWidth < 1024) {
          this.sidebarOpen = false;
        }
      });

    // Auto-open sidebar on desktop
    if (window.innerWidth >= 1024) {
      this.sidebarOpen = true;
    } else {
      this.sidebarOpen = false;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handleLogout(): void {
    this.authService.logout();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  getMainContentClass(): string {
    let classes = '';
    
    if (this.config.showSidebar) {
      classes += this.sidebarOpen ? 'lg:ml-64' : 'ml-0';
    }
    
    if (this.config.showHeader) {
      classes += ' pt-16'; // Account for fixed header
    }

    return classes;
  }
}