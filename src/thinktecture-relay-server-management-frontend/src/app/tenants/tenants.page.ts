import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AlertController,
  InfiniteScrollCustomEvent,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonIcon,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonItem,
  IonLabel,
  IonList,
  IonModal,
  IonNote,
  IonRouterOutlet,
  IonSearchbar,
  IonTitle,
  IonToolbar,
  SearchbarCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { add, trashOutline } from 'ionicons/icons';
import { lastValueFrom } from 'rxjs';
import { ApiService } from '../api/api.service';
import { Tenant } from '../api/tenant.model';
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
    IonButtons,
    IonButton,
    IonIcon,
    IonSearchbar,
    IonContent,
    IonList,
    IonItem,
    IonLabel,
    IonNote,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonModal,
    NewTenantComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantsPage {
  private api = inject(ApiService);
  private alertController = inject(AlertController);

  tenants = signal<Tenant[]>([], {});
  scrollDisabled = signal(false);

  presentingElement = inject(IonRouterOutlet).nativeEl;
  filter = '';

  @ViewChild('newTenant') newTenant: NewTenantComponent | null = null;

  constructor() {
    this.loadTenants();
    addIcons({ add, trashOutline });
  }

  search(ev: SearchbarCustomEvent) {
    this.filter = ev.target.value ?? '';
    this.loadTenants(true);
  }

  async deleteTenant(event: MouseEvent, index: number) {
    event.stopPropagation();

    const tenant = this.tenants()[index];

    const alert = await this.alertController.create({
      message: `Are you sure you want to delete the tenant "${tenant.displayName ?? tenant.name}"`,
      buttons: [
        {
          text: 'Delete tenant',
          role: 'destructive',
          handler: async () => {
            await lastValueFrom(this.api.deleteTenant(tenant.name));
            this.loadTenants(true);
          },
        },
        { text: 'Cancel', role: 'cancel' },
      ],
    });
    await alert.present();
  }

  async onInfinite(ev: Event) {
    const customEv = ev as InfiniteScrollCustomEvent;
    await this.loadTenants();
    customEv.target.complete();
  }

  private async loadTenants(reset = false) {
    const page = await lastValueFrom(
      this.api.getTenantsPaged(
        reset ? 0 : this.tenants().length,
        PAGE_SIZE,
        this.filter,
      ),
    );
    if (reset) {
      this.tenants.set(page.results);
    } else {
      this.tenants.update((tenants) => [...tenants, ...page.results]);
    }

    this.scrollDisabled.set(page.results.length < page.pageSize);
  }
}
