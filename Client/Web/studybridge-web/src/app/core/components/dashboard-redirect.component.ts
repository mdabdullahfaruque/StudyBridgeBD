import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';

@Component({
  selector: 'app-dashboard-redirect',
  standalone: true,
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div class="text-center">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">{{ loadingMessage }}</p>
      </div>
    </div>
  `
})
export class DashboardRedirectComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  
  loadingMessage = 'Checking authentication...';

  ngOnInit(): void {
    // Check if user is authenticated
    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.loadingMessage = 'Redirecting to your dashboard...';
        // User is authenticated - redirect to appropriate dashboard
        const redirectUrl = this.authService.getRedirectUrlForUser();
        this.router.navigate([redirectUrl], { replaceUrl: true });
      } else {
        this.loadingMessage = 'Redirecting to login...';
        // User is not authenticated - redirect to login
        this.router.navigate(['/auth/login'], { replaceUrl: true });
      }
    });
  }
}