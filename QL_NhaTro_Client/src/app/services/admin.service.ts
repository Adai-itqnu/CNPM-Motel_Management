import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  constructor(private http: HttpService) {}

  // ========== BOOKING MANAGEMENT ==========
  getAllBookings(): Observable<any> {
    return this.http.get('admin/bookings');
  }

  updateBookingStatus(bookingId: string, status: string, adminNote?: string): Observable<any> {
    return this.http.put(`admin/bookings/${bookingId}/status`, { status, adminNote });
  }

  // ========== CONTRACT MANAGEMENT ==========
  getAllContracts(): Observable<any> {
    return this.http.get('admin/contracts');
  }

  terminateContract(contractId: string, reason: string): Observable<any> {
    return this.http.post(`admin/contracts/${contractId}/terminate`, { reason });
  }

  extendContract(contractId: string, extendMonths: number): Observable<any> {
    return this.http.post(`admin/contracts/${contractId}/extend`, { extendMonths });
  }

  // ========== BILL MANAGEMENT ==========
  getAllBills(): Observable<any> {
    return this.http.get('admin/bills');
  }

  getDepositPayments(): Observable<any> {
    return this.http.get('admin/payments/deposits');
  }

  createBill(billData: any): Observable<any> {
    return this.http.post('admin/bills', billData);
  }

  updateBillStatus(billId: string, status: string): Observable<any> {
    return this.http.put(`admin/bills/${billId}/status`, { status });
  }

  // ========== FIX DATA ==========
  fixDepositAmounts(): Observable<any> {
    return this.http.post('admin/fix-deposit-amounts', {});
  }
}
