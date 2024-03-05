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
          import('../tenant-connections/tenant-connections.page').then(
            (m) => m.TenantConnectionsPage,
          ),
      },
      {
        path: 'tenants/:name/statistics',
        loadComponent: () =>
          import('../tenant-statistics/tenant-statistics.page').then(
            (m) => m.TenantStatisticsPage,
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
