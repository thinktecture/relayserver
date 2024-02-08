import {
  Component,
  EventEmitter,
  Output,
  ViewChild,
  inject,
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
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { lastValueFrom } from 'rxjs';
import { ApiService } from '../api/api.service';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';

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
    EditTenantComponent,
  ],
})
export class NewTenantComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  @Output() dismiss = new EventEmitter<void>();

  @ViewChild(EditTenantComponent) editTenant: EditTenantComponent | undefined;

  form = EditTenantComponent.generateForm();

  focus() {
    this.editTenant?.focus();
  }

  async create() {
    const formTenant = this.form.getRawValue();
    const tenant = {
      ...formTenant,
      credentials: formTenant.credentials.map((credential) => ({
        id: credential.id ?? undefined,
        plainTextValue: credential.plainTextValue,
        expiration: credential.isExpiring
          ? new Date(
              `${credential.expiration.substring(0, 10)}T23:59:59`,
            ).toISOString()
          : null,
      })),
    };
    await lastValueFrom(this.api.postTenant(tenant));
    this.dismiss.emit();
    this.router.navigate(['/tabs', 'tenants', tenant.name]);
  }
}
