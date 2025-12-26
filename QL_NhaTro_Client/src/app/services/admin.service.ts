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

  // Generate monthly bills
  generateBills(month: number, year: number): Observable<any> {
    return this.http.post('admin/bills/generate', { month, year });
  }

  // Update electricity/water meters
  updateBillMeters(billId: string, data: { electricityNewIndex: number; waterNewIndex: number; otherFees?: number; notes?: string }): Observable<any> {
    return this.http.put(`admin/bills/${billId}/update-meters`, data);
  }

  // Send bill to tenant
  sendBillToTenant(billId: string): Observable<any> {
    return this.http.post(`admin/bills/${billId}/send`, {});
  }

  // ========== FIX DATA ==========
  fixDepositAmounts(): Observable<any> {
    return this.http.post('admin/fix-deposit-amounts', {});
  }
}

