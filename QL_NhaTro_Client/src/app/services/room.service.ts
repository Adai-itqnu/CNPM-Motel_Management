import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Room, CreateRoomDto, UpdateRoomDto } from '../models/room.model';

@Injectable({
  providedIn: 'root'
})
export class RoomService {
  private REST_API_SERVER = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Lấy danh sách phòng với filter
  getRooms(params?: {
    search?: string;
    status?: string;
    floor?: number;
    minPrice?: number;
    maxPrice?: number;
    page?: number;
    pageSize?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    
    if (params) {
      if (params.search) httpParams = httpParams.set('search', params.search);
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.floor) httpParams = httpParams.set('floor', params.floor.toString());
      if (params.minPrice) httpParams = httpParams.set('minPrice', params.minPrice.toString());
      if (params.maxPrice) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
      if (params.page) httpParams = httpParams.set('page', params.page.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get(`${this.REST_API_SERVER}/rooms`, {
      headers: this.getHeaders(),
      params: httpParams
    });
  }

  // Lấy chi tiết phòng
  getRoom(id: string): Observable<any> {
    return this.http.get(`${this.REST_API_SERVER}/rooms/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Tạo phòng mới
  createRoom(room: CreateRoomDto): Observable<any> {
    return this.http.post(`${this.REST_API_SERVER}/rooms`, room, {
      headers: this.getHeaders()
    });
  }

  // Cập nhật phòng
  updateRoom(id: string, room: UpdateRoomDto): Observable<any> {
    return this.http.put(`${this.REST_API_SERVER}/rooms/${id}`, room, {
      headers: this.getHeaders()
    });
  }

  // Xóa phòng
  deleteRoom(id: string): Observable<any> {
    return this.http.delete(`${this.REST_API_SERVER}/rooms/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Thêm tiện ích
  addAmenity(roomId: string, amenityName: string): Observable<any> {
    return this.http.post(`${this.REST_API_SERVER}/rooms/${roomId}/amenities`, 
      { amenityName },
      { headers: this.getHeaders() }
    );
  }

  // Xóa tiện ích
  deleteAmenity(amenityId: number): Observable<any> {
    return this.http.delete(`${this.REST_API_SERVER}/rooms/amenities/${amenityId}`, {
      headers: this.getHeaders()
    });
  }

  // Tải lên hình ảnh phòng
  uploadRoomImage(roomId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${localStorage.getItem('token')}`
      // Don't set Content-Type here, let the browser set it with the correct boundary
    });

    return this.http.post(`${this.REST_API_SERVER}/rooms/${roomId}/upload-image`, 
      formData,
      { headers }
    );
  }

  // Thêm hình ảnh (URL)
  addImage(roomId: string, imageData: any): Observable<any> {
    return this.http.post(`${this.REST_API_SERVER}/rooms/${roomId}/images`, 
      imageData,
      { headers: this.getHeaders() }
    );
  }

  // Xóa hình ảnh
  deleteImage(roomId: string, imageId: number): Observable<any> {
    return this.http.delete(`${this.REST_API_SERVER}/rooms/${roomId}/images/${imageId}`, {
      headers: this.getHeaders()
    });
  }

  // Đặt ảnh làm ảnh đại diện
  setPrimaryImage(roomId: string, imageId: number): Observable<any> {
    return this.http.put(
      `${this.REST_API_SERVER}/rooms/${roomId}/images/${imageId}/set-primary`,
      {},
      { headers: this.getHeaders() }
    );
  }
}