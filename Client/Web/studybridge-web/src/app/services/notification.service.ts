/**
 * Notification Service
 * Handles user notifications, toasts, and alerts
 */

import { Injectable } from '@angular/core';

export interface NotificationOptions {
  duration?: number;
  position?: 'top' | 'bottom';
  type?: 'success' | 'error' | 'warning' | 'info';
  closable?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications: Array<{
    id: string;
    message: string;
    type: string;
    timestamp: Date;
  }> = [];

  constructor() {}

  /**
   * Show a success notification
   */
  showSuccess(message: string, options?: NotificationOptions): void {
    this.showNotification(message, { ...options, type: 'success' });
  }

  /**
   * Show an error notification
   */
  showError(message: string, options?: NotificationOptions): void {
    this.showNotification(message, { ...options, type: 'error' });
  }

  /**
   * Show a warning notification
   */
  showWarning(message: string, options?: NotificationOptions): void {
    this.showNotification(message, { ...options, type: 'warning' });
  }

  /**
   * Show an info notification
   */
  showInfo(message: string, options?: NotificationOptions): void {
    this.showNotification(message, { ...options, type: 'info' });
  }

  /**
   * Show a generic notification
   */
  showNotification(message: string, options: NotificationOptions = {}): void {
    const notification = {
      id: this.generateId(),
      message,
      type: options.type || 'info',
      timestamp: new Date()
    };

    this.notifications.push(notification);

    // Log to console for now (in a real app, this would show actual toast notifications)
    console.log(`[${notification.type.toUpperCase()}] ${message}`, notification);

    // Auto-remove after duration
    const duration = options.duration || 5000;
    setTimeout(() => {
      this.removeNotification(notification.id);
    }, duration);
  }

  /**
   * Remove a specific notification
   */
  removeNotification(id: string): void {
    const index = this.notifications.findIndex(n => n.id === id);
    if (index > -1) {
      this.notifications.splice(index, 1);
    }
  }

  /**
   * Clear all notifications
   */
  clearAll(): void {
    this.notifications = [];
  }

  /**
   * Get all current notifications
   */
  getNotifications() {
    return [...this.notifications];
  }

  /**
   * Generate a unique ID for notifications
   */
  private generateId(): string {
    return `notification_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}