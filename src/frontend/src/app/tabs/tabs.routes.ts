import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'tabs',
    loadComponent: () => import('../tabs/tabs.page').then((m) => m.TabsPage),
    children: [
      {
        path: 'tenants',
        loadComponent: () =>
          import('../tenants/tenants.page').then((m) => m.TenantsPage),
      },
      {
        path: 'tenants/:name',
        loadComponent: () =>
          import('../tenant-details/tenant-details.page').then(
            (m) => m.TenantDetailsPage,
          ),
      },
      {
        path: 'tenants/:name/connections',
        loadComponent: () =>
          import('../connections/connections.page').then(
            (m) => m.ConnectionsPage,
          ),
      },
      {
        path: 'stats',
        loadComponent: () =>
          import('../stats/stats.page').then((m) => m.StatsPage),
      },
      {
        path: '',
        redirectTo: '/tabs/tenants',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: '',
    redirectTo: '/tabs/tenants',
    pathMatch: 'full',
  },
];
