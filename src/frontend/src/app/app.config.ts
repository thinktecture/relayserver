import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig } from '@angular/core';
import {
  RouteReuseStrategy,
  provideRouter,
  withComponentInputBinding,
} from '@angular/router';
import { IonicRouteStrategy } from '@ionic/angular';
import { provideIonicAngular } from '@ionic/angular/standalone';
import { apiInterceptor } from './api/api.interceptor';
import { routes } from './app.routes';
import { provideCharts } from 'ng2-charts';
import {
  CategoryScale,
  Colors,
  Legend,
  LineController,
  LineElement,
  LinearScale,
  PointElement,
} from 'chart.js';

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: RouteReuseStrategy, useClass: IonicRouteStrategy },
    provideIonicAngular({ mode: 'ios' }),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([apiInterceptor])),
    provideCharts({
      registerables: [
        LineController,
        LineElement,
        PointElement,
        CategoryScale,
        LinearScale,
        Legend,
        Colors,
      ],
    }),
  ],
};
