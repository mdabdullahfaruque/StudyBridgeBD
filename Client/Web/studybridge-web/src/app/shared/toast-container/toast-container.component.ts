import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { ToastService } from '../../services/toast.service';
import { Toast, ToastType } from '../../models/common.models';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-20 right-4 z-50 space-y-2 max-w-sm w-full">
      <div 
        *ngFor="let toast of toasts; trackBy: trackByToast"
        class="toast animate-slide-in"
        [ngClass]="getToastClasses(toast.type)">
        
        <div class="flex items-start p-4">
          <!-- Icon -->
          <div class="flex-shrink-0">
            <div [innerHTML]="getToastIcon(toast.type)" class="w-5 h-5"></div>
          </div>
          
          <!-- Content -->
          <div class="ml-3 flex-1">
            <p class="text-sm font-medium" [ngClass]="getTitleClasses(toast.type)">
              {{ toast.title }}
            </p>
            <p 
              *ngIf="toast.message" 
              class="mt-1 text-sm" 
              [ngClass]="getMessageClasses(toast.type)">
              {{ toast.message }}
            </p>
          </div>
          
          <!-- Close Button -->
          <div class="ml-4 flex-shrink-0">
            <button 
              (click)="removeToast(toast.id)"
              class="inline-flex text-gray-400 hover:text-gray-600 focus:outline-none focus:text-gray-600 transition-colors">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </button>
          </div>
        </div>
        
        <!-- Progress Bar (if duration is set) -->
        <div 
          *ngIf="toast.duration && toast.duration > 0"
          class="toast-progress"
          [style.animation]="'toast-progress ' + toast.duration + 'ms linear'">
        </div>
      </div>
    </div>
  `,
  styles: [`
    .toast-progress {
      height: 2px;
      background: currentColor;
      opacity: 0.3;
      transform-origin: left;
    }
    
    @keyframes toast-progress {
      from {
        transform: scaleX(1);
      }
      to {
        transform: scaleX(0);
      }
    }
  `]
})
export class ToastContainerComponent implements OnInit, OnDestroy {
  toasts: Toast[] = [];
  private destroy$ = new Subject<void>();

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    this.toastService.toasts$
      .pipe(takeUntil(this.destroy$))
      .subscribe(toasts => {
        this.toasts = toasts;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  removeToast(id: string): void {
    this.toastService.remove(id);
  }

  trackByToast(index: number, toast: Toast): string {
    return toast.id;
  }

  getToastClasses(type: ToastType): string {
    const baseClasses = 'relative overflow-hidden rounded-lg bg-white shadow-lg ring-1 ring-black ring-opacity-5';
    
    switch (type) {
      case ToastType.Success:
        return `${baseClasses} toast-success`;
      case ToastType.Error:
        return `${baseClasses} toast-error`;
      case ToastType.Warning:
        return `${baseClasses} toast-warning`;
      case ToastType.Info:
        return `${baseClasses} toast-info`;
      default:
        return baseClasses;
    }
  }

  getTitleClasses(type: ToastType): string {
    switch (type) {
      case ToastType.Success:
        return 'text-success-800';
      case ToastType.Error:
        return 'text-error-800';
      case ToastType.Warning:
        return 'text-warning-800';
      case ToastType.Info:
        return 'text-primary-800';
      default:
        return 'text-secondary-800';
    }
  }

  getMessageClasses(type: ToastType): string {
    switch (type) {
      case ToastType.Success:
        return 'text-success-600';
      case ToastType.Error:
        return 'text-error-600';
      case ToastType.Warning:
        return 'text-warning-600';
      case ToastType.Info:
        return 'text-primary-600';
      default:
        return 'text-secondary-600';
    }
  }

  getToastIcon(type: ToastType): string {
    switch (type) {
      case ToastType.Success:
        return '<svg class="text-success-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path></svg>';
      case ToastType.Error:
        return '<svg class="text-error-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path></svg>';
      case ToastType.Warning:
        return '<svg class="text-warning-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path></svg>';
      case ToastType.Info:
        return '<svg class="text-primary-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path></svg>';
      default:
        return '<svg class="text-secondary-500" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path></svg>';
    }
  }
}
