import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { Subject, takeUntil, filter } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { HeaderComponent } from './header/header.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { ToastContainerComponent } from '../toast-container/toast-container.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    HeaderComponent,
    SidebarComponent,
    ToastContainerComponent
  ],
  template: `
    <div class="min-h-screen bg-secondary-50">
      <!-- Header -->
      <app-header 
        [user]="currentUser" 
        (logout)="handleLogout()"
        (toggleSidebar)="toggleSidebar()">
      </app-header>
      
      <!-- Main Content Area -->
      <div class="flex">
        <!-- Sidebar -->
        <app-sidebar 
          [isOpen]="sidebarOpen"
          [user]="currentUser"
          (close)="closeSidebar()">
        </app-sidebar>
        
        <!-- Main Content -->
        <main class="flex-1 transition-all duration-300" 
              [class.ml-64]="sidebarOpen"
              [class.ml-0]="!sidebarOpen">
          <div class="p-6">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
      
      <!-- Toast Notifications -->
      <app-toast-container></app-toast-container>
    </div>
  `
})
export class LayoutComponent implements OnInit, OnDestroy {
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
}
