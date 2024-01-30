import { JsonPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  InfiniteScrollCustomEvent,
  IonContent,
  IonHeader,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonItem,
  IonLabel,
  IonList,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { lastValueFrom } from 'rxjs';
import { ApiService, Tenant } from '../api/api.service';

const PAGE_SIZE = 20;

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
    IonLabel,
    IonItem,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
  ],
})
export class TenantsPage {
  private api = inject(ApiService);

  tenants: Tenant[] = [];
  scrollDisabled = false;

  constructor() {
    this.loadTenants();
  }

  async onInfinite(ev: Event) {
    console.log('onInfinite');
    const cev = ev as InfiniteScrollCustomEvent;
    await this.loadTenants();
    cev.target.complete();
  }

  private async loadTenants() {
    const page = await lastValueFrom(
      this.api.getTenantsPaged(this.tenants.length, PAGE_SIZE),
    );
    for (const tenant of page.results ?? []) {
      this.tenants.push(tenant);
    }
    console.log(`${this.tenants.length} / ${page.totalCount}`);

    this.scrollDisabled = (page.results?.length ?? 0) < PAGE_SIZE;
  }
}
