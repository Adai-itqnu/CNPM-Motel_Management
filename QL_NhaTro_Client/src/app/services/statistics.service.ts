import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStats {
  totalRooms: number;
  occupiedRooms: number;
  availableRooms: number;
  monthlyRevenue: number;
  pendingBookings: number;
}

export interface RoomStatus {
  occupied: number;
  available: number;
  maintenance: number;
  total: number;
}

export interface RevenueChart {
  monthlyData: MonthlyRevenue[];
}

export interface MonthlyRevenue {
  month: number;
  monthName: string;
  revenue: number;
}

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
  private apiUrl = 'http://localhost:5001/api/statistics';

  constructor(private http: HttpClient) {}

  getSummary(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/summary`);
  }

  getRoomStatus(): Observable<RoomStatus> {
    return this.http.get<RoomStatus>(`${this.apiUrl}/room-status`);
  }

  getRevenueChart(months: number = 6): Observable<RevenueChart> {
    return this.http.get<RevenueChart>(`${this.apiUrl}/revenue-chart?months=${months}`);
  }
}
