import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface Bill {
  id: string;
  roomName: string;
  month: number;
  year: number;
  electricityTotal: number;
  waterTotal: number;
  roomPrice: number;
  otherFees: number;
  totalAmount: number;
  status: string;
  dueDate: string;
  paymentDate: string | null;
  createdAt: string;
}

@Component({
  selector: 'app-my-bills',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-bills.component.html',
  styleUrls: ['./my-bills.component.css']
})
export class MyBillsComponent implements OnInit {
  bills: Bill[] = [];
  isLoading = true;
  error = '';
  filterStatus = 'all';

  private apiUrl = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadBills();
  }

  loadBills(): void {
    this.isLoading = true;
    this.error = '';
    
    this.http.get<Bill[]>(`${this.apiUrl}/user/my-bills`).subscribe({
      next: (data) => {
        this.bills = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading bills:', err);
        this.error = 'Không thể tải hóa đơn';
        this.isLoading = false;
      }
    });
  }

  get filteredBills(): Bill[] {
    if (this.filterStatus === 'all') return this.bills;
    return this.bills.filter(b => b.status === this.filterStatus);
  }

  get pendingCount(): number {
    return this.bills.filter(b => b.status === 'Pending').length;
  }

  get paidCount(): number {
    return this.bills.filter(b => b.status === 'Paid').length;
  }

  get totalPending(): number {
    return this.bills.filter(b => b.status === 'Pending').reduce((sum, b) => sum + b.totalAmount, 0);
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  getStatusLabel(status: string): string {
    return status === 'Paid' ? 'Đã thanh toán' : 'Chưa thanh toán';
  }

  getStatusClass(status: string): string {
    return status === 'Paid' ? 'status-paid' : 'status-pending';
  }

  getMonthYear(month: number, year: number): string {
    return `Tháng ${month}/${year}`;
  }

  setFilter(status: string): void {
    this.filterStatus = status;
  }
}
