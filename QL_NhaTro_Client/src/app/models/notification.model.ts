export interface Notification {
  id: string;
  title: string;
  content: string;
  type: 'System' | 'Payment' | 'Warning' | 'Admin';
  isRead: boolean;
  link?: string;
  createdAt: string;
  senderName?: string;
}

export interface NotificationResponse {
  notifications: Notification[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AdminSendNotificationRequest {
  targetType: 'all' | 'user';
  userId?: string;
  title: string;
  content: string;
  type?: string;
  link?: string;
}

export interface UserForNotification {
  id: string;
  fullName: string;
  email: string;
  roomName?: string;
}
