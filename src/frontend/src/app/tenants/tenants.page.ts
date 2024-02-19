import { AsyncPipe, I18nPluralPipe } from '@angular/common';
import { Component, ViewChild, inject, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
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
  IonProgressBar,
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
  Observable,
  Subject,
  catchError,
  combineLatest,
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
import { ApiAuthStore } from '../api/api-auth.store';

interface AccumulatedTenants {
  results: Tenant[];
  moreAvailable: boolean;
  error: boolean;
}

interface AccumulateTenantsInput {
  scrollEv?: InfiniteScrollCustomEvent;
  filter?: string;
  deleteEv?: string;
}

const PAGE_SIZE = 50;

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [
    RouterLink,
    AsyncPipe,
    I18nPluralPipe,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonSearchbar,
    IonProgressBar,
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
  private apiAuth = inject(ApiAuthStore);

  private scrollEv$ = new BehaviorSubject<
    InfiniteScrollCustomEvent | undefined
  >(undefined);
  private deleteEv$ = new Subject<string>();

  presentingElement = inject(IonRouterOutlet).nativeEl;

  tenantsMapping = {
    '=1': '1 tenant',
    other: '# tenants',
  };

  loading = signal(false);
  filter = signal('');
  tenants$ = combineLatest({
    filter: toObservable(this.filter),
    headerName: toObservable(this.apiAuth.headerName),
    key: toObservable(this.apiAuth.key),
  }).pipe(
    tap(() => this.content?.scrollToTop()),
    tap(() => this.loading.set(true)),
    switchMap(({ filter }) =>
      this.scrollEv$.pipe(
        map((scrollEv) => ({ scrollEv, filter })),
        mergeWith(this.deleteEv$.pipe(map((deleteEv) => ({ deleteEv })))),
        switchScan(
          (accumulated, value) => this.accumulateTenants(accumulated, value),
          { results: [], moreAvailable: true, error: false },
        ),
      ),
    ),
    tap(() => this.loading.set(false)),
  );

  @ViewChild(NewTenantComponent) newTenant?: NewTenantComponent;
  @ViewChild(IonContent) content?: IonContent;

  constructor() {
    addIcons({ add, trashOutline });
  }

  search(ev: SearchbarCustomEvent): void {
    this.filter.set(ev.target.value ?? '');
  }

  async deleteTenant(event: MouseEvent, tenant: Tenant): Promise<void> {
    event.stopPropagation();

    const alert = await this.alertController.create({
      message: `Are you sure you want to delete the tenant "${tenant.displayName ?? tenant.name}"`,
      buttons: [
        {
          text: 'Delete tenant',
          role: 'destructive',
          handler: async () => {
            try {
              await lastValueFrom(this.api.deleteTenant(tenant.name));
              this.deleteEv$.next(tenant.name);
            } catch (error) {
              // error toast is shown by API interceptor
            }
          },
        },
        { text: 'Cancel', role: 'cancel' },
      ],
    });

    await alert.present();
  }

  onInfinite(ev: Event): void {
    this.scrollEv$.next(ev as InfiniteScrollCustomEvent);
  }

  private accumulateTenants(
    { results, moreAvailable }: AccumulatedTenants,
    { scrollEv, filter, deleteEv }: AccumulateTenantsInput,
  ): Observable<AccumulatedTenants> {
    if (deleteEv !== undefined) {
      return of({
        results: results.filter((result) => result.name !== deleteEv),
        moreAvailable,
        error: false,
      });
    }

    if (!moreAvailable) {
      return of({ results, moreAvailable, error: false });
    }

    return this.api.getTenantsPaged(results.length, PAGE_SIZE, filter).pipe(
      map((page) => ({
        results: [...results, ...page.results],
        moreAvailable: results.length + page.results.length < page.totalCount,
        error: false,
      })),
      catchError(() => {
        // error toast is shown by API interceptor
        return of({
          results,
          moreAvailable: false,
          error: true,
        });
      }),
      tap(() => setTimeout(() => scrollEv?.target.complete())),
    );
  }
}
