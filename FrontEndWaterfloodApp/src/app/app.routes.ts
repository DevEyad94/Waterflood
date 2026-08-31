import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { Roles } from './core/enum/roles.enum';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/admin/login/login.component').then((m) => m.LoginComponent),
    title: 'Waterflood Performance Management System',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./components/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    canActivate: [AuthGuard],
    title: 'Dashboard',
  },
  {
    path: 'map',
    loadComponent: () =>
      import('./components/map/map.component').then((m) => m.MapComponent),
    canActivate: [AuthGuard],
    title: 'Well Map',
  },
  {
    path: 'wells',
    loadComponent: () =>
      import('./components/wells/wells.component').then((m) => m.WellsComponent),
    canActivate: [AuthGuard],
    title: 'Well Data Management',
  },
  {
    path: 'monitoring',
    loadComponent: () =>
      import('./components/monitoring/monitoring.component').then((m) => m.MonitoringComponent),
    canActivate: [AuthGuard],
    title: 'Performance Monitoring',
  },
  {
    path: 'relationships',
    loadComponent: () =>
      import('./components/relationships/relationships.component').then(
        (m) => m.RelationshipsComponent
      ),
    canActivate: [AuthGuard, AdminGuard],
    data: { roles: [Roles.ADMIN, Roles.PETROLEUM_ENGINEER] },
    title: 'Injector-Producer Relationships',
  },
  {
    path: 'login',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./components/users/users.component').then((m) => m.UsersComponent),
    canActivate: [AuthGuard, AdminGuard],
    data: { roles: [Roles.ADMIN] },
    title: 'User Management',
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./components/unauthorized/unauthorized.component').then(
        (m) => m.UnauthorizedComponent
      ),
    title: 'Unauthorized',
  },
  { path: '**', redirectTo: 'dashboard', pathMatch: 'full' },
];
