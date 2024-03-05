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
      },
      yRight: {
        position: 'right',
        title: { display: true, text: 'Size in bytes' },
      },
    },
  };
}
