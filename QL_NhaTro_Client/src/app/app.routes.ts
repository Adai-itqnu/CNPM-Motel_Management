import { Routes } from '@angular/router';
import { LoginComponent } from './Auth/login/login.component';
import { RegisterComponent } from './Auth/register/register.component';
import { AdminLayoutComponent } from './admin/admin-layout/admin-layout';
import { AdminDashboardComponent } from './admin/components/admin-dashboard/admin-dashboard.component';
import { UserLayoutComponent } from './user/user-layout/user-layout';
import { UserDashboardComponent } from './user/components/user-dashboard/user-dashboard.component';
import { UserProfileComponent } from './user/components/user-profile/user-profile.component';
import { adminGuard, userGuard } from './guards/auth.guard';
import { RoomManagementComponent } from './admin/components/room-management/room-management.component';
import { UserManagementComponent } from './admin/components/user-management/user-management.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
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
      // Add more admin routes here as needed
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
      // TODO: Add these components later
      // { path: 'profile', component: UserProfileComponent },
      // { path: 'bills', component: UserBillsComponent },
      // { path: 'payments', component: UserPaymentsComponent },
    ]
  },
  
  { path: '**', redirectTo: '/login' }
];
