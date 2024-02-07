import { AsyncPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import {
  IonBackButton,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { firstValueFrom, lastValueFrom, shareReplay, switchMap } from 'rxjs';
import { ApiService } from '../api/api.service';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';
import { ViewTenantComponent } from '../view-tenant/view-tenant.component';

@Component({
  selector: 'app-tenant-details',
  templateUrl: './tenant-details.page.html',
  styleUrls: ['./tenant-details.page.scss'],
  standalone: true,
  imports: [
    AsyncPipe,
    ReactiveFormsModule,
    IonHeader,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonButton,
    IonTitle,
    IonContent,
    EditTenantComponent,
    ViewTenantComponent,
  ],
})
export class TenantDetailsPage {
  private api = inject(ApiService);

  name = input.required<string>();

  tenant$ = toObservable(this.name).pipe(
    switchMap((name) => this.api.getSingleTenant(name)),
    shareReplay(),
  );
  editing = false;
  form = EditTenantComponent.generateForm();

  get credentials() {
    return this.form.controls.credentials;
  }

  async edit() {
    const tenant = await firstValueFrom(this.tenant$);

    this.credentials.clear();

    tenant.credentials.forEach(() => {
      const control = EditTenantComponent.generateCredentialForm();

      // plainTextValue is required, disable it to make the form valid
      control.controls.plainTextValue.disable();

      this.credentials.push(control);
    });

    this.form.reset({
      ...tenant,
      credentials: tenant.credentials.map((credential) => ({
        id: credential.id,
        plainTextValue: null,
        created: credential.created,
        isExpiring: credential.expiration !== null,
        expiration: credential.expiration ?? '',
      })),
    });

    this.editing = true;
  }

  async save() {
    const formTenant = this.form.getRawValue();
    const tenant = {
      ...formTenant,
      credentials: formTenant.credentials.map((credential) => ({
        id: credential.id,
        plainTextValue: credential.plainTextValue,
        expiration: credential.isExpiring ? credential.expiration : null,
      })),
    };
    await lastValueFrom(this.api.putTenant(tenant));

    this.editing = false;
  }

  cancel() {
    this.editing = false;
  }
}
