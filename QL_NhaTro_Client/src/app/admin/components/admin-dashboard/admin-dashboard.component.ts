import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StatisticsService, RoomDetails, MonthlyRevenue, RevenueChart } from '../../../services/statistics.service';
import { AuthService } from '../../../services/auth.service';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  
  currentDate: string = '';
  currentUser: any = null;
  isLoading = true;
  error: string = '';
  
  roomDetails: RoomDetails = {
    total: 0,
    occupied: 0,
    available: 0,
    maintenance: 0,
    reserved: 0,
    occupancyRate: 0
  };
  
  monthlyRevenue: number = 0;
  revenueData: MonthlyRevenue[] = [];
  maxRevenue: number = 0;

  constructor(
    private statisticsService: StatisticsService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.currentUser = this.authService.getUser();
    this.updateDate();
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.error = '';

    forkJoin({
      summary: this.statisticsService.getSummary(),
      roomDetails: this.statisticsService.getRoomDetails(),
      revenueChart: this.statisticsService.getRevenueChart(6)
    })
    .pipe(finalize(() => this.isLoading = false))
    .subscribe({
      next: (results) => {
        // Room details
        this.roomDetails = results.roomDetails;
        
        // Monthly revenue from summary
        this.monthlyRevenue = results.summary.monthlyRevenue || 0;
        
        // Revenue chart data
        this.revenueData = results.revenueChart.monthlyData || [];
        this.maxRevenue = Math.max(...this.revenueData.map(m => m.revenue), 1);
      },
      error: (err) => {
        console.error('Error loading dashboard data:', err);
        this.error = 'Không thể tải dữ liệu dashboard';
      }
    });
  }

  updateDate() {
    const now = new Date();
    const options: Intl.DateTimeFormatOptions = { 
      weekday: 'long',
      day: 'numeric', 
      month: 'long', 
      year: 'numeric' 
    };
    this.currentDate = now.toLocaleDateString('vi-VN', options);
  }

  refreshData() {
    this.loadDashboardData();
    this.updateDate();
  }

  // Format currency to VND
  formatCurrency(amount: number): string {
    if (amount >= 1000000000) {
      return (amount / 1000000000).toFixed(1) + ' tỷ';
    }
    if (amount >= 1000000) {
      return (amount / 1000000).toFixed(1) + ' tr';
    }
    return new Intl.NumberFormat('vi-VN').format(amount) + ' đ';
  }

  formatShortCurrency(amount: number): string {
    if (amount >= 1000000) {
      return (amount / 1000000).toFixed(1) + 'tr';
    }
    if (amount >= 1000) {
      return (amount / 1000).toFixed(0) + 'k';
    }
    return amount.toString();
  }

  // Calculate bar height percentage
  getBarHeight(revenue: number): number {
    if (this.maxRevenue === 0) return 10;
    return Math.max((revenue / this.maxRevenue) * 80, 10);
  }

  // Calculate pie chart slice dash array
  getSliceDashArray(count: number): string {
    if (this.roomDetails.total === 0) return '0 502.4';
    const percentage = (count / this.roomDetails.total) * 100;
    const circumference = 2 * Math.PI * 80; // 2πr where r=80
    const dashLength = (percentage / 100) * circumference;
    return `${dashLength} ${circumference - dashLength}`;
  }

  // Calculate pie chart slice offset
  getSliceOffset(previousCount: number): string {
    if (this.roomDetails.total === 0) return '0';
    const percentage = (previousCount / this.roomDetails.total) * 100;
    const circumference = 2 * Math.PI * 80;
    const offset = (percentage / 100) * circumference;
    return (-offset).toString();
  }
}
