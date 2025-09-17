import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../../shared/services/auth.service';
import { ProfileService } from '../../../../shared/services/profile.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserDto } from '../../../../shared/models/api.models';
import { UserProfile } from '../../../../shared/models/user.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="space-y-6">
      <!-- Page Header -->
      <div class="bg-white rounded-lg shadow-sm border border-secondary-200">
        <div class="px-6 py-4 border-b border-secondary-200">
          <h1 class="text-2xl font-bold text-secondary-900">Profile Settings</h1>
          <p class="text-sm text-secondary-600 mt-1">
            Manage your account information and preferences
          </p>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Profile Form -->
        <div class="lg:col-span-2">
          <div class="card">
            <div class="card-header">
              <h3 class="text-lg font-semibold text-secondary-900">Personal Information</h3>
              <p class="text-sm text-secondary-600">Update your personal details</p>
            </div>
            <div class="card-body">
              <form [formGroup]="profileForm" (ngSubmit)="onSubmit()" class="space-y-4">
                <!-- Avatar Section -->
                <div class="flex items-center space-x-4">
                  <div class="w-16 h-16 bg-gradient-to-r from-primary-600 to-primary-700 rounded-full flex items-center justify-center">
                    <span class="text-white text-xl font-semibold">
                      {{ getInitials() }}
                    </span>
                  </div>
                  <div>
                    <h4 class="text-lg font-medium text-secondary-900">{{ currentUser?.displayName }}</h4>
                    <p class="text-sm text-secondary-600">{{ currentUser?.email }}</p>
                  </div>
                </div>

                <!-- Form Fields -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label for="firstName" class="block text-sm font-medium text-secondary-700 mb-1">
                      First Name
                    </label>
                    <input
                      id="firstName"
                      type="text"
                      formControlName="firstName"
                      class="input"
                      [class.input-error]="isFieldInvalid('firstName')"
                      placeholder="Enter your first name">
                    <div *ngIf="isFieldInvalid('firstName')" class="mt-1 text-sm text-error-600">
                      <span *ngIf="profileForm.get('firstName')?.errors?.['required']">First name is required</span>
                    </div>
                  </div>

                  <div>
                    <label for="lastName" class="block text-sm font-medium text-secondary-700 mb-1">
                      Last Name
                    </label>
                    <input
                      id="lastName"
                      type="text"
                      formControlName="lastName"
                      class="input"
                      [class.input-error]="isFieldInvalid('lastName')"
                      placeholder="Enter your last name">
                    <div *ngIf="isFieldInvalid('lastName')" class="mt-1 text-sm text-error-600">
                      <span *ngIf="profileForm.get('lastName')?.errors?.['required']">Last name is required</span>
                    </div>
                  </div>
                </div>

                <div>
                  <label for="email" class="block text-sm font-medium text-secondary-700 mb-1">
                    Email Address
                  </label>
                  <input
                    id="email"
                    type="email"
                    formControlName="email"
                    class="input bg-secondary-50"
                    readonly
                    placeholder="Your email address">
                  <p class="text-xs text-secondary-500 mt-1">
                    Email cannot be changed. Contact support if you need to update your email.
                  </p>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label for="phone" class="block text-sm font-medium text-secondary-700 mb-1">
                      Phone Number
                    </label>
                    <input
                      id="phone"
                      type="tel"
                      formControlName="phone"
                      class="input"
                      placeholder="Enter your phone number">
                  </div>

                  <div>
                    <label for="country" class="block text-sm font-medium text-secondary-700 mb-1">
                      Country
                    </label>
                    <select
                      id="country"
                      formControlName="country"
                      class="input">
                      <option value="">Select your country</option>
                      <option value="BD">Bangladesh</option>
                      <option value="IN">India</option>
                      <option value="PK">Pakistan</option>
                      <option value="US">United States</option>
                      <option value="UK">United Kingdom</option>
                      <option value="CA">Canada</option>
                      <option value="AU">Australia</option>
                      <!-- Add more countries as needed -->
                    </select>
                  </div>
                </div>

                <div>
                  <label for="dateOfBirth" class="block text-sm font-medium text-secondary-700 mb-1">
                    Date of Birth
                  </label>
                  <input
                    id="dateOfBirth"
                    type="date"
                    formControlName="dateOfBirth"
                    class="input">
                </div>

                <!-- Form Actions -->
                <div class="flex justify-end space-x-3 pt-4">
                  <button
                    type="button"
                    class="btn-outline btn-md"
                    (click)="resetForm()">
                    Cancel
                  </button>
                  <button
                    type="submit"
                    [disabled]="isLoading || profileForm.invalid"
                    class="btn-primary btn-md">
                    <span *ngIf="!isLoading">Save Changes</span>
                    <span *ngIf="isLoading" class="flex items-center">
                      <div class="loading-spinner mr-2"></div>
                      Saving...
                    </span>
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>

        <!-- Profile Statistics -->
        <div class="space-y-6">
          <!-- Account Info -->
          <div class="card">
            <div class="card-header">
              <h3 class="text-lg font-semibold text-secondary-900">Account Status</h3>
            </div>
            <div class="card-body space-y-4">
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Account Type</span>
                <span class="text-sm font-medium text-secondary-900">Student</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Member Since</span>
                <span class="text-sm font-medium text-secondary-900">
                  {{ getMemberSince() }}
                </span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Status</span>
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-success-100 text-success-800">
                  Active
                </span>
              </div>
            </div>
          </div>

          <!-- Learning Stats -->
          <div class="card">
            <div class="card-header">
              <h3 class="text-lg font-semibold text-secondary-900">Learning Progress</h3>
            </div>
            <div class="card-body space-y-4">
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Words Learned</span>
                <span class="text-sm font-medium text-secondary-900">0</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Study Streak</span>
                <span class="text-sm font-medium text-secondary-900">0 days</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-sm text-secondary-600">Total Study Time</span>
                <span class="text-sm font-medium text-secondary-900">0 hours</span>
              </div>
            </div>
          </div>

          <!-- Quick Actions -->
          <div class="card">
            <div class="card-header">
              <h3 class="text-lg font-semibold text-secondary-900">Account Actions</h3>
            </div>
            <div class="card-body space-y-3">
              <button class="btn-outline btn-sm w-full justify-start">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"></path>
                </svg>
                Change Password
              </button>
              
              <button class="btn-outline btn-sm w-full justify-start">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"></path>
                </svg>
                Preferences
              </button>
              
              <button class="btn-outline btn-sm w-full justify-start text-error-600 border-error-300 hover:bg-error-50">
                <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                </svg>
                Delete Account
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ProfileComponent implements OnInit, OnDestroy {
  profileForm: FormGroup;
  currentUser: UserDto | null = null;
  userProfile: UserProfile | null = null;
  isLoading = false;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private profileService: ProfileService,
    private toastService: ToastService
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: [{ value: '', disabled: true }],
      phone: [''],
      country: [''],
      dateOfBirth: ['']
    });
  }

  ngOnInit(): void {
    // Load current user
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (user) {
          this.profileForm.patchValue({
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            email: user.email
          });
          this.loadUserProfile();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async loadUserProfile(): Promise<void> {
    if (!this.currentUser) return;

    try {
      this.profileService.getProfile()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (response) => {
            if (response.data) {
              this.userProfile = response.data;
              
              // Update form with profile data
              this.profileForm.patchValue({
                phone: this.userProfile.phone || '',
                country: this.userProfile.country || '',
                dateOfBirth: this.userProfile.dateOfBirth ? new Date(this.userProfile.dateOfBirth).toISOString().split('T')[0] : ''
              });
            }
          },
          error: (error) => {
            console.error('Failed to load profile:', error);
            // Profile might not exist yet, which is fine for new users
          }
        });
    } catch (error) {
      console.error('Failed to load profile:', error);
    }
  }

  async onSubmit(): Promise<void> {
    if (this.profileForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      try {
        const formValue = this.profileForm.getRawValue();
        const profileData = {
          fullName: `${formValue.firstName} ${formValue.lastName}`.trim(),
          phone: formValue.phone,
          country: formValue.country,
          dateOfBirth: formValue.dateOfBirth ? new Date(formValue.dateOfBirth) : undefined,
          timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
          language: 'en',
          notificationsEnabled: true
        };

        this.profileService.updateProfile(profileData)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: (response) => {
              this.toastService.success('Profile Updated', 'Your profile has been updated successfully');
              if (response.data) {
                this.userProfile = response.data;
              }
            },
            error: (error) => {
              const message = error.error?.message || 'Failed to update profile';
              this.toastService.error('Update Failed', message);
            },
            complete: () => {
              this.isLoading = false;
            }
          });
      } catch (error: any) {
        const message = error.error?.message || 'Failed to update profile';
        this.toastService.error('Update Failed', message);
        this.isLoading = false;
      }
    }
  }

  resetForm(): void {
    if (this.currentUser) {
      this.profileForm.patchValue({
        firstName: this.currentUser.firstName || '',
        lastName: this.currentUser.lastName || '',
        email: this.currentUser.email
      });
      
      if (this.userProfile) {
        this.profileForm.patchValue({
          phone: this.userProfile.phone || '',
          country: this.userProfile.country || '',
          dateOfBirth: this.userProfile.dateOfBirth ? new Date(this.userProfile.dateOfBirth).toISOString().split('T')[0] : ''
        });
      }
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.profileForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getInitials(): string {
    if (!this.currentUser?.displayName) return 'U';
    return this.currentUser.displayName
      .split(' ')
      .map((name: string) => name.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getMemberSince(): string {
    // This would come from the user's creation date in a real app
    return 'Today';
  }
}
