import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Payment {
  id: string;
  roomName: string;
  userName: string;
  amount: number;
  paymentType: string;
  paymentMethod: string;
  status: string;
  providerTxnId: string | null;
  paymentDate: string;
  createdAt: string;
}

@Component({
  selector: 'app-payments-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payments-management.component.html',
  styleUrls: ['./payments-management.component.css']
})
export class PaymentsManagementComponent implements OnInit {
  payments: Payment[] = [];
  filteredPayments: Payment[] = [];
  isLoading = true;
  
  searchText = '';
  filterType = 'all';
  filterStatus = 'all';
  filterMonth = 0;
  months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

  private apiUrl = 'http://localhost:5001/api';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.isLoading = true;
    this.http.get<Payment[]>(`${this.apiUrl}/admin/payments`).subscribe({
      next: (data) => {
        this.payments = data;
        this.filteredPayments = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading payments:', err);
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.applyFilters();
  }

  applyFilters(): void {
    this.filteredPayments = this.payments.filter(p => {
      // Search
      const searchMatch = !this.searchText || 
        p.roomName?.toLowerCase().includes(this.searchText.toLowerCase()) ||
        p.userName?.toLowerCase().includes(this.searchText.toLowerCase()) ||
        p.providerTxnId?.toLowerCase().includes(this.searchText.toLowerCase());
      
      // Type
      const typeMatch = this.filterType === 'all' || p.paymentType === this.filterType;
      
      // Status
      const statusMatch = this.filterStatus === 'all' || p.status === this.filterStatus;
      
      // Month
      const monthMatch = this.filterMonth === 0 || 
        (p.paymentDate && new Date(p.paymentDate).getMonth() + 1 === this.filterMonth);
      
      return searchMatch && typeMatch && statusMatch && monthMatch;
    });
  }

  getCountByStatus(status: string): number {
    return this.payments.filter(p => p.status === status).length;
  }

  getTotalSuccess(): number {
    return this.payments.filter(p => p.status === 'Success').reduce((sum, p) => sum + p.amount, 0);
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Success': 'Thành công',
      'Pending': 'Chờ xử lý',
      'Failed': 'Thất bại'
    };
    return labels[status] || status;
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
}
