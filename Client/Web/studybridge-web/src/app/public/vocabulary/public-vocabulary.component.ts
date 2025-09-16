import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-public-vocabulary',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <h1 class="text-2xl font-heading font-bold text-secondary-900 mb-4">
          IELTS Vocabulary
        </h1>
        <div class="bg-warning-50 border border-warning-200 rounded-lg p-4">
          <div class="flex items-center">
            <svg class="w-5 h-5 text-warning-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"></path>
            </svg>
            <div>
              <h3 class="text-sm font-medium text-warning-800">Feature Coming Soon</h3>
              <p class="text-sm text-warning-700 mt-1">
                The vocabulary management system is currently in development. 
                This feature will include 2,100+ IELTS words with definitions, examples, and interactive learning tools.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PublicVocabularyComponent {}