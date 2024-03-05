import { AsyncPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import {
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonProgressBar,
  IonList,
  IonItem,
  IonNote,
} from '@ionic/angular/standalone';
import { ChartConfiguration } from 'chart.js';
import { ApiService } from '../api/api.service';
import { tapResponse } from '@ngrx/operators';
import { Observable, map } from 'rxjs';
import { ViewStatsComponent } from '../view-stats/view-stats.component';

@Component({
  selector: 'app-stats',
  templateUrl: 'stats.page.html',
  styleUrls: ['stats.page.scss'],
  standalone: true,
  imports: [
    AsyncPipe,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonProgressBar,
    IonContent,
    IonList,
    IonItem,
    IonNote,
    ViewStatsComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatsPage {
  private api = inject(ApiService);

  loading = signal(true);
  error = signal(false);

  data$: Observable<ChartConfiguration['data']> = this.api.getStatistics().pipe(
    tapResponse({
      next: () => {},
      // error toast is shown by API interceptor
      error: () => this.error.set(true),
      finalize: () => this.loading.set(false),
    }),
    map((statistics) => ({
      datasets: [
        {
          data: statistics.map((s) => s.totalRequestBodySize),
          label: 'Total request body size',
          yAxisID: 'yRight',
        },
        {
          data: statistics.map((s) => s.totalResponseBodySize),
          label: 'Total response body size',
          yAxisID: 'yRight',
        },
        {
          data: statistics.map((s) => s.requestCount),
          label: 'Total requests',
        },
        {
          data: statistics.map((s) => s.abortedRequestCount),
          label: 'Aborted requests',
        },
        {
          data: statistics.map((s) => s.failedRequestCount),
          label: 'Failed requests',
        },
        {
          data: statistics.map((s) => s.expiredRequestCount),
          label: 'Expired requests',
        },
        {
          data: statistics.map((s) => s.erroredRequestCount),
          label: 'Errored requests',
        },
      ],
      labels: statistics.map((s) => s.date),
    })),
  );
}
