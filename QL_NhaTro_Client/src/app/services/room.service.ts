import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateRoomDto, UpdateRoomDto } from '../models/room.model';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class RoomService {
  constructor(private http: HttpService) {}

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

    return this.http.get('rooms', httpParams);
  }

  // Lấy chi tiết phòng
  getRoom(id: string): Observable<any> {
    return this.http.get(`rooms/${id}`);
  }

  // Tạo phòng mới
  createRoom(room: CreateRoomDto): Observable<any> {
    return this.http.post('rooms', room);
  }

  // Cập nhật phòng
  updateRoom(id: string, room: UpdateRoomDto): Observable<any> {
    return this.http.put(`rooms/${id}`, room);
  }

  // Xóa phòng
  deleteRoom(id: string): Observable<any> {
    return this.http.delete(`rooms/${id}`);
  }

  // Thêm tiện ích
  addAmenity(roomId: string, amenityName: string): Observable<any> {
    return this.http.post(`rooms/${roomId}/amenities`, { amenityName });
  }

  // Xóa tiện ích
  deleteAmenity(amenityId: number): Observable<any> {
    return this.http.delete(`rooms/amenities/${amenityId}`);
  }

  // Tải lên hình ảnh phòng
  uploadRoomImage(roomId: string, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.postFormData(`rooms/${roomId}/images`, formData);
  }

  // Thêm hình ảnh (URL)
  addImage(roomId: string, imageData: any): Observable<any> {
    return this.http.post(`rooms/${roomId}/images`, imageData);
  }

  // Xóa hình ảnh
  deleteImage(roomId: string, imageId: number): Observable<any> {
    return this.http.delete(`rooms/${roomId}/images/${imageId}`);
  }

  // Đặt ảnh làm ảnh đại diện
  setPrimaryImage(roomId: string, imageId: number): Observable<any> {
    return this.http.put(`rooms/${roomId}/images/${imageId}/set-primary`, {});
  }
}
