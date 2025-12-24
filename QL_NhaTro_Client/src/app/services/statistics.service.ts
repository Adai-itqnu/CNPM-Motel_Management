import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

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
  providedIn: 'root',
})
export class StatisticsService {
  constructor(private http: HttpService) {}

  getSummary(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>('statistics/summary');
  }

  getRoomStatus(): Observable<RoomStatus> {
    return this.http.get<RoomStatus>('statistics/room-status');
  }

  getRevenueChart(months: number = 6): Observable<RevenueChart> {
    return this.http.get<RevenueChart>(`statistics/revenue-chart?months=${months}`);
  }
}
