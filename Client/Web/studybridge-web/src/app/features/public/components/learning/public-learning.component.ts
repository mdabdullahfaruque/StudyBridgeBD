import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-public-learning',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-6">
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <h1 class="text-2xl font-heading font-bold text-secondary-900 mb-4">
          Learning System
        </h1>
        <div class="bg-info-50 border border-info-200 rounded-lg p-4">
          <div class="flex items-center">
            <svg class="w-5 h-5 text-info-600 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <div>
              <h3 class="text-sm font-medium text-info-800">Advanced Learning Engine</h3>
              <p class="text-sm text-info-700 mt-1">
                Our Spaced Repetition System (SRS) with personalized learning algorithms is under development. 
                This will provide adaptive vocabulary learning tailored to your progress.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PublicLearningComponent {}