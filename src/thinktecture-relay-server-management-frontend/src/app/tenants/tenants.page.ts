import { Component, OnInit, ViewChild, inject } from '@angular/core';
import {
  InfiniteScrollCustomEvent,
  IonButton,
  IonButtons,
  IonContent,
  IonFab,
  IonFabButton,
  IonHeader,
  IonIcon,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonInput,
  IonItem,
  IonLabel,
  IonList,
  IonModal,
  IonNote,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { lastValueFrom } from 'rxjs';
import { ApiService, Tenant } from '../api/api.service';
import { RouterLink } from '@angular/router';
import { addIcons } from 'ionicons';
import { add } from 'ionicons/icons';
import { NewTenantComponent } from '../new-tenant/new-tenant.component';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [
    RouterLink,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonFab,
    IonFabButton,
    IonIcon,
    IonList,
    IonItem,
    IonLabel,
    IonNote,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonModal,
    NewTenantComponent,
  ],
})
export class TenantsPage implements OnInit {
  private api = inject(ApiService);

  tenants: Tenant[] = [];
  scrollDisabled = false;
  presentingElement: HTMLIonRouterOutletElement | null = null;

  @ViewChild('newTenant') newTenant: NewTenantComponent | null = null;

  constructor() {
    this.loadTenants();
    addIcons({ add });
  }

  ngOnInit() {
    this.presentingElement = document.querySelector('ion-router-outlet');
  }

  async onInfinite(ev: Event) {
    const cev = ev as InfiniteScrollCustomEvent;
    await this.loadTenants();
    cev.target.complete();
  }

  private async loadTenants() {
    const page = await lastValueFrom(
      this.api.getTenantsPaged(this.tenants.length, PAGE_SIZE),
    );
    this.tenants.push(...(page.results ?? []));
    this.scrollDisabled = (page.results?.length ?? 0) < PAGE_SIZE;
  }
}
