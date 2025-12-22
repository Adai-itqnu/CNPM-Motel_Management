export interface Contract {
  id: string;
  roomId: string;
  tenantId: string;
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
  tenant?: any; // User model
}

export interface CreateContractDto {
  roomId: string;
  tenantId: string;
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
