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
  Subject,
  lastValueFrom,
  map,
  mergeWith,
  of,
  switchMap,
  switchScan,
  tap,
} from 'rxjs';
import { ApiService } from '../api/api.service';
import { Tenant } from '../api/tenant.model';
import { NewTenantComponent } from '../new-tenant/new-tenant.component';
import { AsyncPipe } from '@angular/common';
import { toObservable } from '@angular/core/rxjs-interop';

const PAGE_SIZE = 50;

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

  private scrollEv$ = new BehaviorSubject<
    InfiniteScrollCustomEvent | undefined
  >(undefined);
  private deleteEv$ = new Subject<string>();

  presentingElement = inject(IonRouterOutlet).nativeEl;

  filter = signal('');
  tenants$ = toObservable(this.filter).pipe(
    tap(() => this.content?.scrollToTop()),
    switchMap((filter) =>
      this.scrollEv$.pipe(
        map((scrollEv) => ({ scrollEv, filter })),
        mergeWith(this.deleteEv$.pipe(map((deleteEv) => ({ deleteEv })))),
        switchScan(
          (accumulated, value) => this.accumulateTenants(accumulated, value),
          { results: [], moreAvailable: true },
        ),
      ),
    ),
  );

  @ViewChild(NewTenantComponent) newTenant?: NewTenantComponent;
  @ViewChild(IonContent) content?: IonContent;

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
            this.deleteEv$.next(tenant.name);
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

  private accumulateTenants(
    { results, moreAvailable }: { results: Tenant[]; moreAvailable: boolean },
    {
      scrollEv,
      filter,
      deleteEv,
    }: {
      scrollEv?: InfiniteScrollCustomEvent;
      filter?: string;
      deleteEv?: string;
    },
  ) {
    if (deleteEv !== undefined) {
      return of({
        results: results.filter((result) => result.name !== deleteEv),
        moreAvailable,
      });
    }

    if (!moreAvailable) {
      return of({ results, moreAvailable });
    }

    return this.api.getTenantsPaged(results.length, PAGE_SIZE, filter).pipe(
      map((page) => ({
        results: [...results, ...page.results],
        moreAvailable: page.results.length >= page.pageSize,
      })),
      tap(() => setTimeout(() => scrollEv?.target.complete())),
    );
  }
}
