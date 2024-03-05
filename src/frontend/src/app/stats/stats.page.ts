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
import { ApiService } from '../api/api.service';
import { tapResponse } from '@ngrx/operators';
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

  statistics$ = this.api.getStatistics().pipe(
    tapResponse({
      next: () => {},
      // error toast is shown by API interceptor
      error: () => this.error.set(true),
      finalize: () => this.loading.set(false),
    }),
  );
}
