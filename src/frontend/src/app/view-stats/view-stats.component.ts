import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

@Component({
  selector: 'app-view-stats',
  templateUrl: './view-stats.component.html',
  styleUrl: './view-stats.component.scss',
  standalone: true,
  imports: [BaseChartDirective],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewStatsComponent {
  data = input.required<ChartConfiguration['data']>();

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
