import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private REST_API_SERVER = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  // Create booking with deposit payment
  createDepositBooking(bookingData: {
    roomId: string;
    checkInDate: string;
    contactPhone: string;
    contactEmail: string;
    notes?: string;
  }): Observable<any> {

    return this.http.post(`${this.REST_API_SERVER}/booking/create-deposit`, bookingData);
  }

  // Get user's bookings
  getMyBookings(): Observable<any> {
    return this.http.get(`${this.REST_API_SERVER}/booking/my-bookings`);
  }
}
