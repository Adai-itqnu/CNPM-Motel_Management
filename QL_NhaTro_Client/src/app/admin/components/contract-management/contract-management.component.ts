import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../services/admin.service';

interface Contract {
  id: string;
  roomId: string;
  roomName: string;
  userId: string;
  userName: string;
  userEmail: string;
  userPhone: string;
  startDate: string;
  endDate: string;
  monthlyPrice: number;
  depositAmount: number;
  status: string;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-contract-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contract-management.component.html',
  styleUrls: ['./contract-management.component.css']
})
export class ContractManagementComponent implements OnInit {
  contracts: Contract[] = [];
  isLoading = true;
  error = '';
  filterStatus = 'all';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadContracts();
  }

  loadContracts(): void {
    this.isLoading = true;
    this.adminService.getAllContracts().subscribe({
      next: (data) => {
        this.contracts = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Không thể tải danh sách hợp đồng';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  get filteredContracts(): Contract[] {
    if (this.filterStatus === 'all') {
      return this.contracts;
    }
    return this.contracts.filter(c => c.status === this.filterStatus);
  }

  terminateContract(contract: Contract): void {
    const reason = prompt('Lý do chấm dứt hợp đồng:');
    if (reason === null) return;

    if (!confirm(`Xác nhận chấm dứt hợp đồng phòng ${contract.roomName}?`)) {
      return;
    }

    this.adminService.terminateContract(contract.id, reason).subscribe({
      next: () => {
        contract.status = 'Terminated';
        alert('Đã chấm dứt hợp đồng thành công');
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
      }
    });
  }

  extendContract(contract: Contract): void {
    const monthsStr = prompt('Nhập số tháng muốn gia hạn:', '12');
    if (monthsStr === null) return;

    const months = parseInt(monthsStr);
    if (isNaN(months) || months <= 0) {
      alert('Vui lòng nhập số tháng hợp lệ');
      return;
    }

    if (!confirm(`Xác nhận gia hạn hợp đồng phòng ${contract.roomName} thêm ${months} tháng?`)) {
      return;
    }

    this.adminService.extendContract(contract.id, months).subscribe({
      next: (response) => {
        alert(response.message);
        this.loadContracts(); // Reload to get updated endDate
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra');
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      'Draft': 'Chờ nhận phòng',
      'Active': 'Đang hoạt động',
      'Expired': 'Đã hết hạn',
      'Terminated': 'Đã chấm dứt',
      'Cancelled': 'Đã hủy'
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: { [key: string]: string } = {
      'Draft': 'status-draft',
      'Active': 'status-active',
      'Expired': 'status-expired',
      'Terminated': 'status-terminated',
      'Cancelled': 'status-cancelled'
    };
    return classes[status] || '';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN');
  }

  formatCurrency(amount: number): string {
    return amount.toLocaleString('vi-VN') + ' VNĐ';
  }

  getRemainingDays(endDate: string): number {
    const end = new Date(endDate);
    const today = new Date();
    const diff = end.getTime() - today.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  getDraftCount(): number {
    return this.contracts.filter(c => c.status === 'Draft').length;
  }

  getActiveCount(): number {
    return this.contracts.filter(c => c.status === 'Active').length;
  }

  getExpiredCount(): number {
    return this.contracts.filter(c => c.status === 'Expired').length;
  }
}
