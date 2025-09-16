import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-public-settings',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <h1 class="text-2xl font-heading font-bold text-secondary-900 mb-4">
          Settings
        </h1>
        <div class="bg-secondary-50 border border-secondary-200 rounded-lg p-4">
          <div class="flex items-center">
            <svg class="w-5 h-5 text-secondary-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"></path>
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
            </svg>
            <div>
              <h3 class="text-sm font-medium text-secondary-800">User Preferences</h3>
              <p class="text-sm text-secondary-700 mt-1">
                Customizable user settings and preferences will be available soon.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PublicSettingsComponent {}