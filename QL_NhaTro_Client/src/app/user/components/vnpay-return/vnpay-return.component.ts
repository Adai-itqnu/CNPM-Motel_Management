import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-vnpay-return',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vnpay-return.component.html',
  styleUrls: ['./vnpay-return.component.css']
})
export class VnpayReturnComponent implements OnInit {
  isSuccess = false;
  isLoading = true;
  message = '';
  transactionId = '';
  amount = 0;

  private apiUrl = 'http://localhost:5001/api';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    // Get VNPAY response from query params
    this.route.queryParams.subscribe(params => {
      const responseCode = params['vnp_ResponseCode'];
      this.transactionId = params['vnp_TransactionNo'] || '';
      const amountStr = params['vnp_Amount'] || '0';
      this.amount = parseInt(amountStr) / 100; // VNPAY sends amount * 100

      // Show result immediately based on VNPAY response code
      this.isLoading = false;
      
      if (responseCode === '00') {
        this.isSuccess = true;
        this.message = 'Thanh toán thành công!';
        
        // Call backend to process in background - don't wait for response
        this.processPaymentInBackground(params);
      } else {
        this.isSuccess = false;
        this.message = this.getErrorMessage(responseCode);
      }
    });
  }

  processPaymentInBackground(params: any): void {
    // Build query string from all params
    let httpParams = new HttpParams();
    for (const key in params) {
      if (params.hasOwnProperty(key)) {
        httpParams = httpParams.set(key, params[key]);
      }
    }

    // Call backend in background - no need to wait or handle response
    this.http.get(`${this.apiUrl}/payment/vnpay-callback`, { params: httpParams })
      .subscribe({
        next: (response: any) => {
          console.log('Backend processed payment:', response);
        },
        error: (err) => {
          console.log('Backend processing (may already be done):', err?.message);
        }
      });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(amount);
  }

  getErrorMessage(code: string): string {
    const errorMessages: { [key: string]: string } = {
      '07': 'Giao dịch bị nghi ngờ gian lận',
      '09': 'Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking',
      '10': 'Xác thực không đúng quá 3 lần',
      '11': 'Đã hết hạn chờ thanh toán',
      '12': 'Thẻ/Tài khoản bị khóa',
      '13': 'Sai mật khẩu xác thực giao dịch (OTP)',
      '24': 'Khách hàng hủy giao dịch',
      '51': 'Tài khoản không đủ số dư',
      '65': 'Vượt quá hạn mức giao dịch trong ngày',
      '75': 'Ngân hàng thanh toán đang bảo trì',
      '79': 'Nhập sai mật khẩu quá số lần quy định',
      '99': 'Lỗi không xác định'
    };
    return errorMessages[code] || `Thanh toán thất bại (Mã lỗi: ${code})`;
  }

  goToMyRooms(): void {
    this.router.navigate(['/user/my-rooms']);
  }

  goToDashboard(): void {
    this.router.navigate(['/user/dashboard']);
  }
}
