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
        
        // Get current user and redirect based on IsPublicUser flag
        const currentUser = this.authService.getCurrentUser();
        console.log('DashboardRedirectComponent - Current user:', currentUser);
        
        if (currentUser) {
          let redirectUrl: string;
          if (currentUser.isPublicUser === true) {
            redirectUrl = '/public/dashboard';
            console.log('Redirecting public user to:', redirectUrl);
          } else {
            redirectUrl = '/admin/dashboard';
            console.log('Redirecting admin user to:', redirectUrl);
          }
          
          this.router.navigate([redirectUrl], { replaceUrl: true });
        } else {
          console.log('No current user found, redirecting to login');
          this.router.navigate(['/auth/login'], { replaceUrl: true });
        }
      } else {
        this.loadingMessage = 'Redirecting to login...';
        // User is not authenticated - redirect to login
        this.router.navigate(['/auth/login'], { replaceUrl: true });
      }
    });
  }
}