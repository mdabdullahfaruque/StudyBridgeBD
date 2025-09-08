import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Toast, ToastType } from '../models/common.models';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  show(type: ToastType, title: string, message?: string, duration: number = 5000): void {
    const toast: Toast = {
      id: this.generateId(),
      type,
      title,
      message,
      duration
    };

    const currentToasts = this.toastsSubject.value;
    this.toastsSubject.next([...currentToasts, toast]);

    // Auto remove toast after duration
    if (duration > 0) {
      setTimeout(() => {
        this.remove(toast.id);
      }, duration);
    }
  }

  success(title: string, message?: string, duration?: number): void {
    this.show(ToastType.Success, title, message, duration);
  }

  error(title: string, message?: string, duration?: number): void {
    this.show(ToastType.Error, title, message, duration);
  }

  warning(title: string, message?: string, duration?: number): void {
    this.show(ToastType.Warning, title, message, duration);
  }

  info(title: string, message?: string, duration?: number): void {
    this.show(ToastType.Info, title, message, duration);
  }

  remove(id: string): void {
    const currentToasts = this.toastsSubject.value;
    const filteredToasts = currentToasts.filter(toast => toast.id !== id);
    this.toastsSubject.next(filteredToasts);
  }

  clear(): void {
    this.toastsSubject.next([]);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }
}
