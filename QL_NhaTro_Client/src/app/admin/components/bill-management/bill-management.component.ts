import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';

interface Bill {
  id: string;
  contractId: string;
  roomId: string;
  roomName: string;
  userId: string;
  userName: string;
  month: number;
  year: number;
  electricityOldIndex: number;
  electricityNewIndex: number;
  electricityPrice: number;
  electricityTotal: number;
  waterOldIndex: number;
  waterNewIndex: number;
  waterPrice: number;
  waterTotal: number;
  roomPrice: number;
  otherFees: number;
  totalAmount: number;
  status: string;
  dueDate: string;
  paymentDate: string | null;
  notes: string | null;
  createdAt: string;
}

interface DepositPayment {
  id: string;
  bookingId: string;
  roomName: string;
  userId: string;
  userName: string;
  amount: number;
  paymentMethod: string;
  status: string;
  provider: string;
  providerTxnId: string;
  paymentDate: string;
  createdAt: string;
}

@Component({
  selector: 'app-bill-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bill-management.component.html',
  styleUrls: ['./bill-management.component.css']
})
export class BillManagementComponent implements OnInit {
  activeTab: 'monthly' | 'deposit' = 'monthly';
  
  bills: Bill[] = [];
  deposits: DepositPayment[] = [];
  isLoading = true;
  error = '';

  // Filter
  filterStatus = 'all';
  filterMonth = new Date().getMonth() + 1;
  filterYear = new Date().getFullYear();

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    if (this.activeTab === 'monthly') {
      this.loadBills();
    } else {
      this.loadDeposits();
    }
  }

  loadBills(): void {
    this.adminService.getAllBills().subscribe({
      next: (data) => {
        this.bills = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách hóa đơn';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  loadDeposits(): void {
    this.adminService.getDepositPayments().subscribe({
      next: (data) => {
        this.deposits = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách tiền cọc';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  switchTab(tab: 'monthly' | 'deposit'): void {
    this.activeTab = tab;
    this.loadData();
  }

  get filteredBills(): Bill[] {
    return this.bills.filter(bill => {
      const statusMatch = this.filterStatus === 'all' || bill.status === this.filterStatus;
      const monthMatch = !this.filterMonth || bill.month === this.filterMonth;
      const yearMatch = !this.filterYear || bill.year === this.filterYear;
      return statusMatch && monthMatch && yearMatch;
    });
  }

  updateBillStatus(bill: Bill, newStatus: string): void {
    if (!confirm(`Xác nhận cập nhật trạng thái hóa đơn thành "${this.getStatusLabel(newStatus)}"?`)) {
      return;
    }

    this.adminService.updateBillStatus(bill.id, newStatus).subscribe({
      next: () => {
        bill.status = newStatus;
        if (newStatus === 'Paid') {
          bill.paymentDate = new Date().toISOString();
        }
        alert('Cập nhật trạng thái thành công!');
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Pending': 'Chưa thanh toán',
      'Paid': 'Đã thanh toán',
      'Overdue': 'Quá hạn',
      'Cancelled': 'Đã hủy'
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Pending': 'status-pending',
      'Paid': 'status-paid',
      'Overdue': 'status-overdue',
      'Cancelled': 'status-cancelled',
      'Success': 'status-paid',
      'Failed': 'status-cancelled'
    };
    return classes[status] || '';
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  formatCurrency(amount: number): string {
    return amount.toLocaleString('vi-VN') + ' VNĐ';
  }
}
