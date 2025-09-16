import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.models';

@Component({
  selector: 'app-public-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="space-y-8">
      <!-- Welcome Section -->
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h1 class="text-2xl font-heading font-bold text-secondary-900">
              Welcome back, {{ currentUser()?.displayName || 'Student' }}!
            </h1>
            <p class="text-secondary-600 mt-1">
              Ready to improve your IELTS vocabulary today?
            </p>
          </div>
          <div class="hidden sm:block">
            <div class="w-16 h-16 bg-gradient-to-r from-primary-500 to-primary-600 rounded-full flex items-center justify-center">
              <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.746 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path>
              </svg>
            </div>
          </div>
        </div>
        
        <!-- Quick Stats -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
          <div class="bg-primary-50 rounded-lg p-4">
            <div class="flex items-center">
              <div class="w-8 h-8 bg-primary-100 rounded-lg flex items-center justify-center mr-3">
                <svg class="w-4 h-4 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path>
                </svg>
              </div>
              <div>
                <p class="text-sm text-primary-600 font-medium">Words Learned</p>
                <p class="text-lg font-semibold text-primary-900">{{ learnedWords() }}</p>
              </div>
            </div>
          </div>
          
          <div class="bg-success-50 rounded-lg p-4">
            <div class="flex items-center">
              <div class="w-8 h-8 bg-success-100 rounded-lg flex items-center justify-center mr-3">
                <svg class="w-4 h-4 text-success-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div>
                <p class="text-sm text-success-600 font-medium">Study Streak</p>
                <p class="text-lg font-semibold text-success-900">{{ studyStreak() }} days</p>
              </div>
            </div>
          </div>
          
          <div class="bg-warning-50 rounded-lg p-4">
            <div class="flex items-center">
              <div class="w-8 h-8 bg-warning-100 rounded-lg flex items-center justify-center mr-3">
                <svg class="w-4 h-4 text-warning-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div>
                <p class="text-sm text-warning-600 font-medium">Today's Goal</p>
                <p class="text-lg font-semibold text-warning-900">{{ todayProgress() }}/{{ dailyGoal() }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-secondary-900">Start Learning</h3>
            <div class="w-10 h-10 bg-primary-100 rounded-lg flex items-center justify-center">
              <svg class="w-5 h-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.828 14.828a4 4 0 01-5.656 0M9 10h1m4 0h1m-6 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
          </div>
          <p class="text-secondary-600 text-sm mb-4">
            Continue your vocabulary journey with personalized lessons.
          </p>
          <button 
            class="w-full bg-primary-600 text-white px-4 py-2 rounded-lg hover:bg-primary-700 transition-colors duration-200 text-sm font-medium">
            Start Session
            <span class="ml-2 text-xs bg-primary-500 px-2 py-0.5 rounded">Coming Soon</span>
          </button>
        </div>

        <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-secondary-900">Browse Words</h3>
            <div class="w-10 h-10 bg-secondary-100 rounded-lg flex items-center justify-center">
              <svg class="w-5 h-5 text-secondary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
              </svg>
            </div>
          </div>
          <p class="text-secondary-600 text-sm mb-4">
            Explore our comprehensive IELTS vocabulary database.
          </p>
          <button 
            routerLink="/public/vocabulary"
            class="w-full bg-secondary-600 text-white px-4 py-2 rounded-lg hover:bg-secondary-700 transition-colors duration-200 text-sm font-medium">
            Browse Vocabulary
            <span class="ml-2 text-xs bg-secondary-500 px-2 py-0.5 rounded">Coming Soon</span>
          </button>
        </div>

        <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
          <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-secondary-900">My Progress</h3>
            <div class="w-10 h-10 bg-success-100 rounded-lg flex items-center justify-center">
              <svg class="w-5 h-5 text-success-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
              </svg>
            </div>
          </div>
          <p class="text-secondary-600 text-sm mb-4">
            Track your learning progress and achievements.
          </p>
          <button 
            class="w-full bg-success-600 text-white px-4 py-2 rounded-lg hover:bg-success-700 transition-colors duration-200 text-sm font-medium">
            View Progress
            <span class="ml-2 text-xs bg-success-500 px-2 py-0.5 rounded">Coming Soon</span>
          </button>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200 p-6">
        <h3 class="text-lg font-semibold text-secondary-900 mb-4">Recent Activity</h3>
        <div class="space-y-3">
          <div class="flex items-center justify-between py-3 border-b border-secondary-100 last:border-b-0">
            <div class="flex items-center space-x-3">
              <div class="w-8 h-8 bg-primary-100 rounded-full flex items-center justify-center">
                <svg class="w-4 h-4 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
              <div>
                <p class="text-sm font-medium text-secondary-900">Account Created</p>
                <p class="text-xs text-secondary-500">Welcome to StudyBridge!</p>
              </div>
            </div>
            <p class="text-xs text-secondary-400">Today</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './public-dashboard.component.scss'
})
export class PublicDashboardComponent implements OnInit {
  currentUser = signal<User | null>(null);
  learnedWords = signal(0);
  studyStreak = signal(0);
  todayProgress = signal(0);
  dailyGoal = signal(10);

  constructor(private authService: AuthService) {}

  ngOnInit() {
    const user = this.authService.getCurrentUser();
    this.currentUser.set(user);

    // Initialize with demo data
    this.initializeDemoData();
  }

  private initializeDemoData() {
    // Simulate user progress data
    this.learnedWords.set(0);
    this.studyStreak.set(1);
    this.todayProgress.set(0);
  }
}