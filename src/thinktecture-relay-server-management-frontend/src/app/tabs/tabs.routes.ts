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
        path: 'tab2',
        loadComponent: () =>
          import('../tab2/tab2.page').then((m) => m.Tab2Page),
      },
      {
        path: 'tab3',
        loadComponent: () =>
          import('../tab3/tab3.page').then((m) => m.Tab3Page),
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
