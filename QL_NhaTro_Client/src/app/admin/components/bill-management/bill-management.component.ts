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
  daysInMonth: number;
  daysRented: number;
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
  isSent: boolean;
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

interface UpdateMetersDto {
  electricityNewIndex: number;
  waterNewIndex: number;
  otherFees: number;
  notes: string;
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
  filterSent = 'all';
  filterMonth = new Date().getMonth() + 1;
  filterYear = new Date().getFullYear();

  // Generate Modal
  showGenerateModal = false;
  generateMonth = new Date().getMonth() + 1;
  generateYear = new Date().getFullYear();
  isGenerating = false;

  // Update Meters Modal
  showUpdateModal = false;
  selectedBill: Bill | null = null;
  updateMeters: UpdateMetersDto = {
    electricityNewIndex: 0,
    waterNewIndex: 0,
    otherFees: 0,
    notes: ''
  };
  isUpdating = false;

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
      const monthMatch = this.filterMonth === 0 || bill.month === this.filterMonth;
      const yearMatch = !this.filterYear || bill.year === this.filterYear;
      const sentMatch = this.filterSent === 'all' || 
                        (this.filterSent === 'sent' && bill.isSent) ||
                        (this.filterSent === 'unsent' && !bill.isSent);
      return statusMatch && monthMatch && yearMatch && sentMatch;
    });
  }

  // Generate Bills
  openGenerateModal(): void {
    this.generateMonth = new Date().getMonth() + 1;
    this.generateYear = new Date().getFullYear();
    this.showGenerateModal = true;
  }

  closeGenerateModal(): void {
    this.showGenerateModal = false;
  }

  generateBills(): void {
    this.isGenerating = true;
    this.adminService.generateBills(this.generateMonth, this.generateYear).subscribe({
      next: (res: any) => {
        alert(res.message || `Đã tạo ${res.count} hóa đơn!`);
        this.closeGenerateModal();
        this.loadBills();
        this.isGenerating = false;
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi tạo hóa đơn');
        this.isGenerating = false;
      }
    });
  }

  // Update Meters
  openUpdateMetersModal(bill: Bill): void {
    this.selectedBill = bill;
    this.updateMeters = {
      electricityNewIndex: bill.electricityNewIndex || bill.electricityOldIndex,
      waterNewIndex: bill.waterNewIndex || bill.waterOldIndex,
      otherFees: bill.otherFees || 0,
      notes: bill.notes || ''
    };
    this.showUpdateModal = true;
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.selectedBill = null;
  }

  saveMeters(): void {
    if (!this.selectedBill) return;
    
    this.isUpdating = true;
    this.adminService.updateBillMeters(this.selectedBill.id, this.updateMeters).subscribe({
      next: (res: any) => {
        // Update local bill data
        if (this.selectedBill) {
          this.selectedBill.electricityNewIndex = this.updateMeters.electricityNewIndex;
          this.selectedBill.waterNewIndex = this.updateMeters.waterNewIndex;
          this.selectedBill.otherFees = this.updateMeters.otherFees;
          this.selectedBill.notes = this.updateMeters.notes;
          if (res.bill) {
            this.selectedBill.electricityTotal = res.bill.electricityTotal;
            this.selectedBill.waterTotal = res.bill.waterTotal;
            this.selectedBill.totalAmount = res.bill.totalAmount;
          }
        }
        alert('Đã cập nhật chỉ số điện nước!');
        this.closeUpdateModal();
        this.isUpdating = false;
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
        this.isUpdating = false;
      }
    });
  }

  // Send Bill
  sendBillToTenant(bill: Bill): void {
    if (!confirm(`Gửi hóa đơn tháng ${bill.month}/${bill.year} đến ${bill.userName}?`)) {
      return;
    }

    this.adminService.sendBillToTenant(bill.id).subscribe({
      next: (res: any) => {
        bill.isSent = true;
        alert(res.message || 'Đã gửi hóa đơn!');
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
      }
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
    return amount.toLocaleString('vi-VN') + ' đ';
  }
}
