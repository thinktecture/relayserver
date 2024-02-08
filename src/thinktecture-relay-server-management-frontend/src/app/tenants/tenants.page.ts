import { Component, ViewChild, inject, signal } from '@angular/core';
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
import {
  BehaviorSubject,
  combineLatestWith,
  lastValueFrom,
  map,
  switchMap,
  switchScan,
  tap,
} from 'rxjs';
import { ApiService } from '../api/api.service';
import { Tenant } from '../api/tenant.model';
import { NewTenantComponent } from '../new-tenant/new-tenant.component';
import { AsyncPipe } from '@angular/common';
import { toObservable } from '@angular/core/rxjs-interop';
import { Page } from '../api/page.model';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [
    RouterLink,
    AsyncPipe,
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
})
export class TenantsPage {
  private api = inject(ApiService);
  private alertController = inject(AlertController);

  private scrollEv$ = new BehaviorSubject<InfiniteScrollCustomEvent | null>(
    null,
  );
  private deleteEv$ = new BehaviorSubject<null>(null);

  presentingElement = inject(IonRouterOutlet).nativeEl;

  filter = signal('');
  tenantsPage$ = toObservable(this.filter).pipe(
    combineLatestWith(this.deleteEv$),
    switchMap(([filter]) =>
      this.scrollEv$.pipe(
        map((scrollEv) => ({ scrollEv, filter })),
        switchScan((accumulated, value) => this.loadPage(accumulated, value), {
          results: [],
          offset: 0,
          pageSize: 0,
          totalCount: 0,
        }),
      ),
    ),
  );

  @ViewChild('newTenant') newTenant: NewTenantComponent | null = null;

  constructor() {
    addIcons({ add, trashOutline });
  }

  search(ev: SearchbarCustomEvent) {
    this.filter.set(ev.target.value ?? '');
  }

  async deleteTenant(event: MouseEvent, tenant: Tenant) {
    event.stopPropagation();

    const alert = await this.alertController.create({
      message: `Are you sure you want to delete the tenant "${tenant.displayName ?? tenant.name}"`,
      buttons: [
        {
          text: 'Delete tenant',
          role: 'destructive',
          handler: async () => {
            await lastValueFrom(this.api.deleteTenant(tenant.name));
            this.deleteEv$.next(null);
          },
        },
        { text: 'Cancel', role: 'cancel' },
      ],
    });

    await alert.present();
  }

  async onInfinite(ev: Event) {
    this.scrollEv$.next(ev as InfiniteScrollCustomEvent);
  }

  private loadPage(
    accumulated: Page<Tenant>,
    {
      scrollEv,
      filter,
    }: { scrollEv: InfiniteScrollCustomEvent | null; filter: string },
  ) {
    return this.api
      .getTenantsPaged(accumulated.results.length, PAGE_SIZE, filter)
      .pipe(
        map((page) => ({
          ...page,
          results: [...accumulated.results, ...page.results],
        })),
        tap(() => scrollEv?.target.complete()),
      );
  }
}
