import {
  ChangeDetectionStrategy,
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

  focusTenantName(): void {
    this.tenantName?.setFocus();
  }

  async create(): Promise<void> {
    if (!this.form.valid) {
      return;
    }

    const formTenant = this.form.getRawValue();
    const tenant = {
      ...formTenant,
      credentials: formTenant.credentials.map((credential) => ({
        plainTextValue: credential.plainTextValue,
        expiration: credential.expiration,
      })),
    };

    await lastValueFrom(this.api.postTenant(tenant));

    this.dismiss.emit();
    this.router.navigate(['/tabs', 'tenants', tenant.name]);
  }
}
