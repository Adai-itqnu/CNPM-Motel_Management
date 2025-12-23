import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatisticsService } from '../../../services/statistics.service';
import { ActivityService } from '../../../services/activity.service';
import { forkJoin } from 'rxjs';
import { finalize } from 'rxjs/operators';

interface StatsCard {
  icon: string;
  title: string;
  value: string;
  label: string;
  trend?: string;
  trendPositive?: boolean;
}

interface RoomStatus {
  occupied: number;
  available: number;
  maintenance: number;
  total: number;
}

interface RecentActivity {
  type: string;
  userName: string;
  description: string;
  time: string;
  amount?: string;
  avatarUrl?: string;
  badge?: { text: string; color: string };
}

interface DashboardStats {
  totalRooms: number;
  occupiedRooms: number;
  availableRooms: number;
  monthlyRevenue: number;
  pendingBookings: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  
  currentDate: string = '';
  isLoading = true; // Skeleton loading
  error: string = '';
  
  stats: StatsCard[] = [];
  roomStatus: RoomStatus = { occupied: 0, available: 0, maintenance: 0, total: 0 };
  recentActivities: RecentActivity[] = [];

  constructor(
    private statisticsService: StatisticsService,
    private activityService: ActivityService
  ) {}

  ngOnInit() {
    this.updateDate();
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.error = '';

    // Load all 3 APIs in PARALLEL - data shows immediately when ready! 🚀
    forkJoin({
      summary: this.statisticsService.getSummary(),
      roomStatus: this.statisticsService.getRoomStatus(),
      activities: this.activityService.getRecent(3)
    })
    .pipe(finalize(() => this.isLoading = false))
    .subscribe({
      next: (results) => {
        // Summary stats
        const data: DashboardStats = results.summary;
        this.stats = [
          {
            icon: 'bedroom_parent',
            title: 'Tổng',
            value: data.totalRooms.toString(),
            label: 'Tổng số phòng'
          },
          {
            icon: 'vpn_key',
            title: 'Đã cho thuê',
            value: data.occupiedRooms.toString(),
            label: 'Đã cho thuê',
            trend: '5%',
            trendPositive: true
          },
          {
            icon: 'payments',
            title: 'Doanh thu',
            value: `${(data.monthlyRevenue / 1000000).toFixed(1)} tr`,
            label: 'Doanh thu tháng',
            trend: '12%',
            trendPositive: true
          }
        ];

        // Room status
        this.roomStatus = results.roomStatus;

        // Recent activities
        this.recentActivities = results.activities;
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
      day: 'numeric', 
      month: 'long', 
      year: 'numeric' 
    };
    this.currentDate = `Hôm nay, ${now.toLocaleDateString('vi-VN', options)}`;
  }

  refreshData() {
    this.loadDashboardData();
    this.updateDate();
  }
}
