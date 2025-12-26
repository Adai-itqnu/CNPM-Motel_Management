import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface Bill {
  id: string;
  roomName: string;
  month: number;
  year: number;
  daysInMonth: number;
  daysRented: number;
  electricityOldIndex: number;
  electricityNewIndex: number;
  electricityTotal: number;
  waterOldIndex: number;
  waterNewIndex: number;
  waterTotal: number;
  roomPrice: number;
  otherFees: number;
  totalAmount: number;
  status: string;
  isSent: boolean;
  dueDate: string;
  paymentDate: string | null;
  notes: string | null;
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
  isPayingBillId: string | null = null;

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
        // Backend đã filter chỉ trả về bills đã gửi (isSent = true)
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

  payBill(bill: Bill): void {
    this.isPayingBillId = bill.id;
    
    this.http.post<{ paymentUrl: string }>(`${this.apiUrl}/payment/bill/${bill.id}/vnpay`, {}).subscribe({
      next: (res) => {
        if (res.paymentUrl) {
          window.location.href = res.paymentUrl;
        }
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi tạo thanh toán');
        this.isPayingBillId = null;
      }
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' đ';
  }

  formatShortCurrency(amount: number): string {
    if (amount >= 1000000) {
      return (amount / 1000000).toFixed(1) + ' tr';
    }
    return new Intl.NumberFormat('vi-VN').format(amount) + ' đ';
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Pending': 'Chưa thanh toán',
      'Paid': 'Đã thanh toán',
      'Overdue': 'Quá hạn'
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Pending': 'status-pending',
      'Paid': 'status-paid',
      'Overdue': 'status-overdue'
    };
    return classes[status] || '';
  }

  getMonthYear(month: number, year: number): string {
    return `Tháng ${month}/${year}`;
  }

  setFilter(status: string): void {
    this.filterStatus = status;
  }
}
