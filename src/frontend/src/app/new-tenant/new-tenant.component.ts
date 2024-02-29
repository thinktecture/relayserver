import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonInput,
  IonItem,
  IonList,
  IonProgressBar,
  IonTitle,
  IonToolbar,
  ModalController,
} from '@ionic/angular/standalone';
import { tapResponse } from '@ngrx/operators';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { exhaustMap, filter, map, pipe, tap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';
import { EditTenantService } from '../edit-tenant/edit-tenant.service';

@Component({
  selector: 'app-new-tenant',
  templateUrl: './new-tenant.component.html',
  styleUrls: ['./new-tenant.component.scss'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonContent,
    IonList,
    IonItem,
    IonInput,
    IonProgressBar,
    EditTenantComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NewTenantComponent {
  private api = inject(ApiService);
  private router = inject(Router);
  private editTenantService = inject(EditTenantService);
  private modalController = inject(ModalController);

  tenantName = viewChild.required<IonInput>('tenantName');

  form = this.editTenantService.generate();
  loading = signal(false);

  async dismiss(): Promise<void> {
    await this.modalController.dismiss();
  }

  create = rxMethod<void>(
    pipe(
      filter(() => this.form.valid),
      tap(() => this.loading.set(true)),
      map(() => this.form.getRawValue()),
      map((formTenant) => ({
        ...formTenant,
        credentials: formTenant.credentials.map((credential) => ({
          ...credential,
          id: undefined,
          created: undefined,
        })),
      })),
      exhaustMap((tenant) =>
        this.api.postTenant(tenant).pipe(
          tapResponse({
            next: () => {
              this.dismiss();
              this.router.navigate(['/tabs', 'tenants', tenant.name]);
            },
            // error toast is shown by API interceptor
            error: () => {},
            finalize: () => this.loading.set(false),
          }),
        ),
      ),
    ),
  );

  focusTenantName(): void {
    this.tenantName().setFocus();
  }
}
