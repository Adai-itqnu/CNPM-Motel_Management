import { Routes } from '@angular/router';
import { LoginComponent } from './Auth/login/login.component';
import { RegisterComponent } from './Auth/register/register.component';
import { AdminLayoutComponent } from './admin/admin-layout/admin-layout';
import { AdminDashboardComponent } from './admin/components/admin-dashboard/admin-dashboard.component';
import { UserLayoutComponent } from './user/user-layout/user-layout';
import { UserDashboardComponent } from './user/components/user-dashboard/user-dashboard.component';
import { UserProfileComponent } from './user/components/user-profile/user-profile.component';
import { VnpayReturnComponent } from './user/components/vnpay-return/vnpay-return.component';
import { MyRoomsComponent } from './user/components/my-rooms/my-rooms.component';
import { MyBillsComponent } from './user/components/my-bills/my-bills.component';
import { PaymentHistoryComponent } from './user/components/payment-history/payment-history.component';
import { adminGuard, userGuard } from './guards/auth.guard';
import { RoomManagementComponent } from './admin/components/room-management/room-management.component';
import { UserManagementComponent } from './admin/components/user-management/user-management.component';
import { BillManagementComponent } from './admin/components/bill-management/bill-management.component';
import { BookingApprovalComponent } from './admin/components/booking-approval/booking-approval.component';
import { ContractManagementComponent } from './admin/components/contract-management/contract-management.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  // Payment callback route (standalone, no layout)
  { path: 'payment/vnpay-return', component: VnpayReturnComponent },
  
  // Admin routes with shared layout
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'rooms', component: RoomManagementComponent },
      { path: 'users', component: UserManagementComponent },
      { path: 'bills', component: BillManagementComponent },
      { path: 'bookings', component: BookingApprovalComponent },
      { path: 'contracts', component: ContractManagementComponent },
    ]
  },
  
  // User routes with shared layout
  {
    path: 'user',
    component: UserLayoutComponent,
    canActivate: [userGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: UserDashboardComponent },
      { path: 'profile', component: UserProfileComponent },
      { path: 'my-rooms', component: MyRoomsComponent },
      { path: 'bills', component: MyBillsComponent },
      { path: 'payments', component: PaymentHistoryComponent },
    ]
  },
  
  { path: '**', redirectTo: '/login' }
];

