import { JsonPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  IonContent,
  IonHeader,
  IonInfiniteScroll,
  IonLabel,
  IonList,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [
    JsonPipe,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonList,
    IonInfiniteScroll,
    IonLabel,
  ],
})
export class TenantsPage {
  private api = inject(ApiService);

  tenants = toSignal(this.api.getTenantsPaged());
}
