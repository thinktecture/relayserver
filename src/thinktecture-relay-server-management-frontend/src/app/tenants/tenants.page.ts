import { Component, inject } from '@angular/core';
import { InfiniteScrollCustomEvent } from '@ionic/angular/standalone';
import { lastValueFrom } from 'rxjs';
import { ApiService, Tenant } from '../api/api.service';
import { RouterLink } from '@angular/router';
import { IonicModule } from '@ionic/angular';
import { addIcons } from 'ionicons';
import { add } from 'ionicons/icons';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [RouterLink, IonicModule],
})
export class TenantsPage {
  private api = inject(ApiService);

  tenants: Tenant[] = [];
  scrollDisabled = false;

  constructor() {
    this.loadTenants();
    addIcons({ add });
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
