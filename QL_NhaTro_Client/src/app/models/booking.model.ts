export interface Booking {
  id: string;
  roomId: string;
  userId: string;
  checkInDate: Date;
  message?: string;
  depositAmount: number;
  depositStatus: 'pending' | 'paid' | 'failed' | 'refunded';
  depositPaidAt?: Date;
  paymentMethod: string;
  status: 'pending' | 'approved' | 'cancelled' | 'rejected';
  adminNote?: string;
  createdAt: Date;
  updatedAt: Date;
  
  // Populated fields
  room?: any; // Room model
  user?: any; // User model
}

export interface CreateBookingDto {
  roomId: string;
  checkInDate: Date;
  message?: string;
}

export interface ApproveBookingDto {
  bookingId: string;
  adminNote?: string;
}

export interface RejectBookingDto {
  bookingId: string;
  adminNote: string;
}
