import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';

import { NotificationService, NotificationMessage } from '../../services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.html',
  styleUrl: './notifications.scss'
})
export class NotificationsComponent implements OnInit {
  notifications$: Observable<NotificationMessage[]>;

  constructor(private notificationService: NotificationService) {
    this.notifications$ = this.notificationService.notifications$;
  }

  ngOnInit(): void {}

  removeNotification(id: string): void {
    this.notificationService.remove(id);
  }

  clearAll(): void {
    this.notificationService.clear();
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'success':
        return '✓';
      case 'error':
        return '✗';
      case 'warning':
        return '⚠';
      case 'info':
      default:
        return 'ℹ';
    }
  }

  trackByNotificationId(index: number, notification: NotificationMessage): string {
    return notification.id;
  }
}
