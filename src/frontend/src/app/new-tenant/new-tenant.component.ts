import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
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
} from '@ionic/angular/standalone';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { exhaustMap, filter, map, pipe, tap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';
import { EditTenantService } from '../edit-tenant/edit-tenant.service';
import { tapResponse } from '@ngrx/operators';

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

  @Output() dismiss = new EventEmitter<void>();

  tenantName = viewChild.required<IonInput>('tenantName');

  form = this.editTenantService.generateForm();
  loading = signal(false);

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
              this.loading.set(false);
              this.dismiss.emit();
              this.router.navigate(['/tabs', 'tenants', tenant.name]);
            },
            error: () => {
              // error toast is shown by API interceptor
              this.loading.set(false);
            },
          }),
        ),
      ),
    ),
  );

  focusTenantName(): void {
    this.tenantName().setFocus();
  }
}
