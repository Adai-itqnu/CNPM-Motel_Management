export interface User {
  id: string;
  username: string;
  email: string;
  passwordHash?: string; 
  fullName: string;
  phone?: string;
  idCard?: string;
  address?: string;
  role: 'admin' | 'user';
  avatarUrl?: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
  fullName: string;
  phone?: string;
  idCard?: string;
  role?: 'admin' | 'user';
}

export interface UpdateUserDto {
  fullName?: string;
  email?: string;
  phone?: string;
  idCard?: string;
  address?: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}
