export interface Payment {
  id: string;
  billId?: string;
  bookingId?: string;
  tenantId: string;
  amount: number;
  paymentType: 'deposit' | 'monthly_bill';
  paymentMethod: string;
  status: 'pending' | 'success' | 'failed';
  provider: string;
  providerTxnId?: string;
  paymentDate?: Date;
  createdAt: Date;
  updatedAt: Date;
  
  // Populated fields
  bill?: any; // Bill model
  booking?: any; // Booking model
  tenant?: any; // User model
}

export interface CreatePaymentDto {
  billId?: string;
  bookingId?: string;
  amount: number;
  paymentType: 'deposit' | 'monthly_bill';
  paymentMethod: string;
}

export interface VNPayCallbackDto {
  vnp_TxnRef: string;
  vnp_Amount: string;
  vnp_OrderInfo: string;
  vnp_ResponseCode: string;
  vnp_TransactionNo: string;
  vnp_BankCode: string;
  vnp_PayDate: string;
  vnp_SecureHash: string;
}
