export interface Contract {
  id: string;
  roomId: string;
  userId: string;
  startDate: Date;
  endDate?: Date;
  depositAmount: number;
  monthlyPrice: number;
  electricityStartIndex: number;
  waterStartIndex: number;
  termsAndConditions?: string;
  status: 'active' | 'expired' | 'terminated';
  createdAt: Date;
  updatedAt: Date;
  
  // Populated fields
  room?: any; // Room model
  user?: any; // User model
}

export interface CreateContractDto {
  roomId: string;
  userId: string;
  startDate: Date;
  endDate?: Date;
  depositAmount: number;
  monthlyPrice: number;
  electricityStartIndex?: number;
  waterStartIndex?: number;
  termsAndConditions?: string;
}

export interface TerminateContractDto {
  contractId: string;
  reason?: string;
}
