import { AsyncPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import {
  IonBackButton,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { switchMap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { ViewTenantComponent } from '../view-tenant/view-tenant.component';

@Component({
  selector: 'app-tenant-details',
  templateUrl: './tenant-details.page.html',
  styleUrls: ['./tenant-details.page.scss'],
  standalone: true,
  imports: [
    AsyncPipe,
    IonHeader,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonButton,
    IonTitle,
    IonContent,
    ViewTenantComponent,
  ],
})
export class TenantDetailsPage {
  private api = inject(ApiService);

  name = input.required<string>();

  details$ = toObservable(this.name).pipe(
    switchMap((name) => this.api.getSingleTenant(name)),
  );
}
