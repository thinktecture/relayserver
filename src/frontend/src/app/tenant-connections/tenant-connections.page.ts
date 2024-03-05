import { AsyncPipe, DatePipe, JsonPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal,
} from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import {
  IonBackButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonItem,
  IonLabel,
  IonList,
  IonNote,
  IonProgressBar,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { tapResponse } from '@ngrx/operators';
import { switchMap, tap } from 'rxjs';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-tenant-connections',
  templateUrl: './tenant-connections.page.html',
  styleUrls: ['./tenant-connections.page.scss'],
  standalone: true,
  imports: [
    AsyncPipe,
    DatePipe,
    JsonPipe,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonBackButton,
    IonProgressBar,
    IonContent,
    IonList,
    IonItem,
    IonNote,
    IonLabel,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantConnectionsPage {
  private api = inject(ApiService);

  name = input.required<string>();

  loading = signal(false);
  error = signal(false);
  connections$ = toObservable(this.name).pipe(
    tap(() => this.loading.set(true)),
    switchMap((name) =>
      this.api.getTenantConnections(name).pipe(
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
