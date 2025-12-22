export interface Bill {
  id: string;
  contractId: string;
  roomId: string;
  tenantId: string;
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
  
  status: 'pending' | 'paid' | 'partially_paid' | 'overdue';
  paymentDate?: Date;
  dueDate?: Date;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
  
  // Populated fields
  contract?: any; // Contract model
  room?: any; // Room model
  tenant?: any; // User model
}

export interface CreateBillDto {
  contractId: string;
  month: number;
  year: number;
  electricityNewIndex: number;
  waterNewIndex: number;
  otherFees?: number;
  dueDate?: Date;
  notes?: string;
}

export interface UpdateBillDto {
  electricityNewIndex?: number;
  waterNewIndex?: number;
  otherFees?: number;
  dueDate?: Date;
  notes?: string;
}
