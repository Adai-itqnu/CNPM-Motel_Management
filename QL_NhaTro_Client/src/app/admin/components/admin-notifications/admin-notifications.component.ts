import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../../services/notification.service';
import { AdminSendNotificationRequest, UserForNotification } from '../../../models/notification.model';

@Component({
  selector: 'app-admin-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-notifications.component.html',
  styleUrls: ['./admin-notifications.component.css']
})
export class AdminNotificationsComponent implements OnInit {
  notification: AdminSendNotificationRequest = {
    targetType: 'all',
    userId: '',
    title: '',
    content: '',
    type: 'Admin',
    link: ''
  };

  users: UserForNotification[] = [];
  recentNotifications: any[] = [];
  
  isSending = false;
  isLoadingHistory = true;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadRecentNotifications();
  }

  loadUsers(): void {
    this.notificationService.getUsersForNotification().subscribe({
      next: (data) => {
        this.users = data;
      },
      error: (err) => {
        console.error('Error loading users:', err);
      }
    });
  }

  loadRecentNotifications(): void {
    this.isLoadingHistory = true;
    this.notificationService.getRecentSentNotifications().subscribe({
      next: (data) => {
        this.recentNotifications = data;
        this.isLoadingHistory = false;
      },
      error: (err) => {
        console.error('Error loading history:', err);
        this.isLoadingHistory = false;
      }
    });
  }

  onTargetChange(): void {
    if (this.notification.targetType === 'all') {
      this.notification.userId = '';
    }
  }

  sendNotification(): void {
    if (!this.notification.title || !this.notification.content) {
      alert('Vui lòng nhập tiêu đề và nội dung!');
      return;
    }

    if (this.notification.targetType === 'user' && !this.notification.userId) {
      alert('Vui lòng chọn người nhận!');
      return;
    }

    this.isSending = true;
    this.notificationService.adminSendNotification(this.notification).subscribe({
      next: (res: any) => {
        alert(res.message || 'Gửi thông báo thành công!');
        this.resetForm();
        this.loadRecentNotifications();
        this.isSending = false;
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
        this.isSending = false;
      }
    });
  }

  resetForm(): void {
    this.notification = {
      targetType: 'all',
      userId: '',
      title: '',
      content: '',
      type: 'Admin',
      link: ''
    };
  }

  getTypeIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'System': '🔔',
      'Payment': '💰',
      'Warning': '⚠️',
      'Admin': '📢'
    };
    return icons[type] || '📋';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  }
}
