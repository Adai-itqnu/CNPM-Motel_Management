import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { 
  Notification, 
  NotificationResponse, 
  AdminSendNotificationRequest,
  UserForNotification 
} from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = 'http://localhost:5001/api/notification';
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get notifications with pagination
  getNotifications(page: number = 1, pageSize: number = 20): Observable<NotificationResponse> {
    return this.http.get<NotificationResponse>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`);
  }

  // Get unread count
  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`).pipe(
      tap(res => this.unreadCountSubject.next(res.count))
    );
  }

  // Refresh unread count
  refreshUnreadCount(): void {
    this.getUnreadCount().subscribe();
  }

  // Mark single notification as read
  markAsRead(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/read`, {}).pipe(
      tap(() => {
        const current = this.unreadCountSubject.value;
        if (current > 0) {
          this.unreadCountSubject.next(current - 1);
        }
      })
    );
  }

  // Mark all notifications as read
  markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => this.unreadCountSubject.next(0))
    );
  }

  // Delete notification
  deleteNotification(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // === Admin APIs ===

  // Send notification from admin
  adminSendNotification(request: AdminSendNotificationRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/admin/send`, request);
  }

  // Get users for notification dropdown
  getUsersForNotification(): Observable<UserForNotification[]> {
    return this.http.get<UserForNotification[]>(`${this.apiUrl}/admin/users`);
  }

  // Get recent sent notifications (admin)
  getRecentSentNotifications(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/admin/sent`);
  }

  // Update unread count directly (for external updates)
  updateUnreadCount(count: number): void {
    this.unreadCountSubject.next(count);
  }
}
