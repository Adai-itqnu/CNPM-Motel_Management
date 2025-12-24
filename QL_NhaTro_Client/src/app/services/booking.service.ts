import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  constructor(private http: HttpService) {}

  // Create booking with deposit payment
  createDepositBooking(bookingData: {
    roomId: string;
    checkInDate: string;
    contactPhone: string;
    contactEmail: string;
    notes?: string;
  }): Observable<any> {
    return this.http.post('booking/create-deposit', bookingData);
  }

  // Get user's bookings
  getMyBookings(): Observable<any> {
    return this.http.get('booking/my-bookings');
  }

  // Get user's rooms (with active/draft contracts)
  getMyRooms(): Observable<any> {
    return this.http.get('booking/my-rooms');
  }

  // Check-in to a room
  checkIn(bookingId: string): Observable<any> {
    return this.http.post(`booking/${bookingId}/check-in`, {});
  }
}
