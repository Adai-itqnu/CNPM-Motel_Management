import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(private http: HttpService) {}

  // Lấy danh sách người dùng
  getUsers(params?: {
    search?: string;
    role?: string;
    isActive?: boolean;
    page?: number;
    pageSize?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();

    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.role) httpParams = httpParams.set('role', params.role);
      if (params.isActive !== undefined)
        httpParams = httpParams.set('isActive', params.isActive.toString());
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get('users', httpParams);
  }

  // Lấy chi tiết người dùng
  getUser(id: string): Observable<any> {
    return this.http.get(`users/${id}`);
  }

  // Alias for getUser (for consistency)
  getUserById(id: string): Observable<any> {
    return this.getUser(id);
  }

  // Lấy lịch sử thuê phòng
  getRentalHistory(id: string): Observable<any> {
    return this.http.get(`users/${id}/rental-history`);
  }

  // Lấy hóa đơn của người thuê
  getUserBills(id: string): Observable<any> {
    return this.http.get(`users/${id}/bills`);
  }

  // Cập nhật thông tin
  updateUser(id: string, data: any): Observable<any> {
    return this.http.put(`users/${id}`, data);
  }

  // Cập nhật thông tin của user hiện tại (không cần admin)
  updateMyProfile(data: any): Observable<any> {
    return this.http.put('user/profile', data);
  }

  // Khóa/mở tài khoản
  toggleUserStatus(id: string): Observable<any> {
    return this.http.patch(`users/${id}/toggle-status`, {});
  }

  // Upload avatar
  uploadAvatar(formData: FormData): Observable<any> {
    return this.http.postFormData('user/upload-avatar', formData);
  }
}
