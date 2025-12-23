import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private REST_API_SERVER = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

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
      if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get(`${this.REST_API_SERVER}/users`, {
      headers: this.getHeaders(),
      params: httpParams
    });
  }

  // Lấy chi tiết người dùng
  getUser(id: string): Observable<any> {
    return this.http.get(`${this.REST_API_SERVER}/users/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Lấy lịch sử thuê phòng
  getRentalHistory(id: string): Observable<any> {
    return this.http.get(`${this.REST_API_SERVER}/users/${id}/rental-history`, {
      headers: this.getHeaders()
    });
  }

  // Lấy hóa đơn của người thuê
  getUserBills(id: string): Observable<any> {
    return this.http.get(`${this.REST_API_SERVER}/users/${id}/bills`, {
      headers: this.getHeaders()
    });
  }

  // Cập nhật thông tin
  updateUser(id: string, data: any): Observable<any> {
    return this.http.put(`${this.REST_API_SERVER}/users/${id}`, data, {
      headers: this.getHeaders()
    });
  }

  // Khóa/mở tài khoản
  toggleUserStatus(id: string): Observable<any> {
    return this.http.patch(`${this.REST_API_SERVER}/users/${id}/toggle-status`, {}, {
      headers: this.getHeaders()
    });
  }

}