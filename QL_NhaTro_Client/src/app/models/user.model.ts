export interface User {
  id: string;
  username: string;
  email: string;
  passwordHash?: string; // Không nên gửi về frontend
  fullName: string;
  phone?: string;
  idCard?: string;
  role: 'admin' | 'tenant';
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
  role?: 'admin' | 'tenant';
}

export interface UpdateUserDto {
  fullName?: string;
  phone?: string;
  idCard?: string;
  avatarUrl?: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}
