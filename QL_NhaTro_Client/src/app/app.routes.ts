import { Routes } from '@angular/router';
import { LoginComponent } from './Auth/login/login.component';
import { RegisterComponent } from './Auth/register/register.component';
import { AdminDashboardComponent } from './admin/components/admin-dashboard/admin-dashboard.component';
import { UserDashboardComponent } from './user/components/user-dashboard/user-dashboard.component';
import { adminGuard, tenantGuard } from './guards/auth.guard';
import { RoomManagementComponent } from './admin/components/room-management/room-management.component';
import { UserManagementComponent } from './admin/components/user-management/user-management.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
  { path: 'admin/rooms', component: RoomManagementComponent, canActivate: [adminGuard] },
  { path: 'admin/users', component: UserManagementComponent, canActivate: [adminGuard] },
  { path: 'user/dashboard', component: UserDashboardComponent, canActivate: [tenantGuard] },
  { path: '**', redirectTo: '/login' }
];
