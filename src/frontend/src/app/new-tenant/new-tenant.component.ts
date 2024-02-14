import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
  ViewChild,
  inject,
  signal,
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

  @ViewChild('tenantName') tenantName?: IonInput;

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
          plainTextValue: credential.plainTextValue,
          expiration: credential.expiration,
        })),
      })),
      exhaustMap((tenant) =>
        this.api.postTenant(tenant).pipe(
          tap(() => this.loading.set(false)),
          tap(() => this.dismiss.emit()),
          tap(() => this.router.navigate(['/tabs', 'tenants', tenant.name])),
        ),
      ),
    ),
  );

  focusTenantName(): void {
    this.tenantName?.setFocus();
  }
}
