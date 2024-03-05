import { AsyncPipe } from '@angular/common';
import { Component, inject, input, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import {
  IonBackButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonItem,
  IonList,
  IonNote,
  IonProgressBar,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { tapResponse } from '@ngrx/operators';
import { switchMap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { ViewStatsComponent } from '../view-stats/view-stats.component';

@Component({
  selector: 'app-tenant-statistics',
  templateUrl: './tenant-statistics.page.html',
  styleUrls: ['./tenant-statistics.page.scss'],
  standalone: true,
  imports: [
    AsyncPipe,
    IonHeader,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonTitle,
    IonProgressBar,
    IonContent,
    IonList,
    IonItem,
    IonNote,
    ViewStatsComponent,
  ],
})
export class TenantStatisticsPage {
  private api = inject(ApiService);

  name = input.required<string>();

  loading = signal(false);
  error = signal(false);
  statistics$ = toObservable(this.name).pipe(
    switchMap((name) =>
      this.api.getTenantStatistics(name).pipe(
        tapResponse({
          next: () => {},
          // error toast is shown by API interceptor
          error: () => this.error.set(true),
          finalize: () => this.loading.set(false),
        }),
      ),
    ),
  );
}
