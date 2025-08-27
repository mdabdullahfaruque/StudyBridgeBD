import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../../../shared/services/auth.service';
import { NotificationService } from '../../../../shared/services/notification.service';
import { User } from '../../../../shared/models/api.models';
import { HeaderComponent } from '../../../../shared/components/header/header';
import { LoadingComponent } from '../../../../shared/components/loading/loading';
import { ButtonComponent } from '../../../../shared/components/button/button';

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, HeaderComponent, LoadingComponent, ButtonComponent],
  templateUrl: './dashboard-home.html',
  styleUrl: './dashboard-home.scss'
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  currentUser$: Observable<User | null>;
  isLoading = false;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.loadUserProfile();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUserProfile(): void {
    this.isLoading = true;
    
    this.authService.getCurrentUser().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (user) => {
        this.isLoading = false;
        // User data is automatically updated in the service
      },
      error: (error) => {
        this.isLoading = false;
        this.notificationService.error(
          'Error Loading Profile',
          'Failed to load your profile information.'
        );
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    this.notificationService.info(
      'Signed Out',
      'You have been successfully signed out.'
    );
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }

  navigateToSettings(): void {
    this.router.navigate(['/settings']);
  }

  refreshProfile(): void {
    this.loadUserProfile();
  }
}
