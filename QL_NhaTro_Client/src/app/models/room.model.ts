export interface Room {
  id: string;
  name: string;
  roomType?: string;
  price: number;
  depositAmount: number;
  electricityPrice: number;
  waterPrice: number;
  description?: string;
  area?: number;
  floor: number;
  status: 'available' | 'occupied' | 'maintenance' | 'reserved';
  currentUserId?: string;
  currentContractId?: string;
  amenities?: RoomAmenity[];
  images?: RoomImage[];
  createdAt: Date;
  updatedAt: Date;
}

export interface RoomAmenity {
  id: number;
  roomId: string;
  amenityName: string;
}

export interface RoomImage {
  id: number;
  roomId: string;
  imageUrl: string;
  filename: string;
  contentType: string;
  isPrimary: boolean;
}

export interface CreateRoomDto {
  name: string;
  roomType?: string;
  price: number;
  depositAmount?: number;
  electricityPrice?: number;
  waterPrice?: number;
  description?: string;
  area?: number;
  floor?: number;
  amenities?: string[];
}

export interface UpdateRoomDto {
  name?: string;
  roomType?: string;
  price?: number;
  depositAmount?: number;
  electricityPrice?: number;
  waterPrice?: number;
  description?: string;
  area?: number;
  floor?: number;
  status?: 'available' | 'occupied' | 'maintenance' | 'reserved';
}
