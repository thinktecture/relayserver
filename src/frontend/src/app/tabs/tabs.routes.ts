import { Routes } from '@angular/router';
import { TabsPage } from './tabs.page';

export const routes: Routes = [
  {
    path: 'tabs',
    component: TabsPage,
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
