import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
} from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { DateStatistic } from '../api/date-statistic.model';

@Component({
  selector: 'app-view-stats',
  templateUrl: './view-stats.component.html',
  styleUrl: './view-stats.component.scss',
  standalone: true,
  imports: [BaseChartDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewStatsComponent {
  statistics = input.required<DateStatistic[]>();

  data = computed<ChartConfiguration['data']>(() => ({
    datasets: [
      {
        data: this.statistics().map((s) => s.totalRequestBodySize),
        label: 'Total request body size',
        yAxisID: 'yBytes',
      },
      {
        data: this.statistics().map((s) => s.totalResponseBodySize),
        label: 'Total response body size',
        yAxisID: 'yBytes',
      },
      {
        data: this.statistics().map((s) => s.requestCount),
        label: 'Total requests',
      },
      {
        data: this.statistics().map((s) => s.abortedRequestCount),
        label: 'Aborted requests',
      },
      {
        data: this.statistics().map((s) => s.failedRequestCount),
        label: 'Failed requests',
      },
      {
        data: this.statistics().map((s) => s.expiredRequestCount),
        label: 'Expired requests',
      },
      {
        data: this.statistics().map((s) => s.erroredRequestCount),
        label: 'Errored requests',
      },
    ],
    labels: this.statistics().map((s) => s.date),
  }));

  options: ChartConfiguration['options'] = {
    elements: {
      line: {
        tension: 0.5,
      },
    },
    scales: {
      y: {
        title: { display: true, text: 'Count' },
        min: 0,
      },
      yBytes: {
        position: 'right',
        title: { display: true, text: 'Size' },
        ticks: {
          callback: function (value) {
            const units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB'];

            const bytes = Number(value);
            if (bytes === 0) {
              return '0';
            }
            if (bytes < 1) {
              return undefined;
            }

            const k = 1000;
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return `${bytes / k ** i} ${units[i]}`;
          },
        },
        min: 0,
      },
    },
  };
}
