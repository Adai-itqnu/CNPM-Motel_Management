import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface Payment {
  id: string;
  amount: number;
  paymentType: string;
  paymentMethod: string;
  status: string;
  provider: string;
  providerTxnId: string;
  paymentDate: string;
  roomName: string;
  billMonth?: number;
  billYear?: number;
  createdAt: string;
}

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-history.component.html',
  styleUrls: ['./payment-history.component.css']
})
export class PaymentHistoryComponent implements OnInit {
  payments: Payment[] = [];
  isLoading = true;
  error = '';

  private apiUrl = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.isLoading = true;
    this.error = '';
    
    this.http.get<Payment[]>(`${this.apiUrl}/user/my-payments`).subscribe({
      next: (data) => {
        this.payments = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading payments:', err);
        this.error = 'Không thể tải lịch sử thanh toán';
        this.isLoading = false;
      }
    });
  }

  get totalAmount(): number {
    return this.payments.reduce((sum, p) => sum + p.amount, 0);
  }

  get depositPayments(): Payment[] {
    return this.payments.filter(p => p.paymentType === 'Deposit');
  }

  get billPayments(): Payment[] {
    return this.payments.filter(p => p.paymentType === 'MonthlyBill');
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' VNĐ';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getPaymentTypeLabel(type: string): string {
    return type === 'Deposit' ? 'Tiền cọc' : 'Hóa đơn tháng';
  }

  getPaymentTypeClass(type: string): string {
    return type === 'Deposit' ? 'type-deposit' : 'type-bill';
  }

  getStatusLabel(status: string): string {
    return status === 'Success' ? 'Thành công' : status === 'Pending' ? 'Đang xử lý' : 'Thất bại';
  }

  getStatusClass(status: string): string {
    return status === 'Success' ? 'status-success' : status === 'Pending' ? 'status-pending' : 'status-failed';
  }
}
