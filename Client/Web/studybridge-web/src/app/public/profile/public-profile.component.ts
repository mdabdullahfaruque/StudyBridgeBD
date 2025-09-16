import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-public-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <h1 class="text-2xl font-heading font-bold text-secondary-900 mb-4">
          My Profile
        </h1>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-secondary-700 mb-1">Display Name</label>
            <p class="text-secondary-900">{{ user?.displayName || 'Student' }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-secondary-700 mb-1">Email</label>
            <p class="text-secondary-900">{{ user?.email }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-secondary-700 mb-1">Role</label>
            <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-100 text-primary-800">
              {{ user?.roles[0] || 'User' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PublicProfileComponent {
  user: any;

  constructor(private authService: AuthService) {
    this.user = this.authService.getCurrentUser();
  }
}