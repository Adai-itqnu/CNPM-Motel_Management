import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../services/user.service';


@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  // Quản lý người dùng
  // Xem danh sách người dùng
  // Khóa/mở tài khoản người dùng
  // Dữ liệu
  users: any[] = [];
  
  // Trạng thái
  loading = false;
  message = '';
  
  // Tìm kiếm
  searchText = '';
  
  // Phân trang
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  total = 0;

  constructor(private userService: UserService) {}

  ngOnInit() {
    this.loadUsers();
  }

  // Load danh sách người dùng
  loadUsers() {
    this.loading = true;
    this.message = '';
    
    this.userService.getUsers({
      search: this.searchText || undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    }).subscribe({
      next: (response) => {
        this.users = response.data;
        this.total = response.total;
        this.totalPages = response.totalPages;
        this.loading = false;
      },
      error: (err) => {
        this.message = 'Lỗi khi tải danh sách người dùng';
        this.loading = false;
        console.error(err);
      }
    });
  }

  // Tìm kiếm
  onSearch() {
    this.currentPage = 1;
    this.loadUsers();
  }

  // Chuyển trang
  changePage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadUsers();
  }

  // Khóa/mở tài khoản
  toggleStatus(user: any) {
    const action = user.isActive ? 'khóa' : 'mở';
    
    if (!confirm(`Bạn có chắc muốn ${action} tài khoản "${user.username}"?`)) {
      return;
    }
    
    this.loading = true;
    this.userService.toggleUserStatus(user.id).subscribe({
      next: (response) => {
        this.message = `Đã ${action} tài khoản thành công`;
        this.loadUsers();
      },
      error: (err) => {
        this.message = `Lỗi khi ${action} tài khoản`;
        this.loading = false;
      }
    });
  }

  // Helper: Lấy class badge cho role
  getRoleBadge(role: string): string {
    return role === 'Admin' ? 'badge-admin' : 'badge-tenant';
  }

  // Helper: Lấy text hiển thị cho role
  getRoleText(role: string): string {
    return role === 'Admin' ? 'Quản trị' : 'Người thuê';
  }

  // Helper: Lấy class badge cho status
  getStatusBadge(isActive: boolean): string {
    return isActive ? 'badge-active' : 'badge-inactive';
  }

  // Helper: Lấy text hiển thị cho status
  getStatusText(isActive: boolean): string {
    return isActive ? 'Hoạt động' : 'Đã khóa';
  }

  // Helper: Format ngày
  formatDate(date: any): string {
    if (!date) return '';
    const d = new Date(date);
    return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
  }

}
