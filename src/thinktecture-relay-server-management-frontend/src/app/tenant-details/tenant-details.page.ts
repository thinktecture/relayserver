import { AsyncPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import {
  IonBackButton,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { firstValueFrom, shareReplay, switchMap } from 'rxjs';
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

  async edit() {
    const tenant = await firstValueFrom(this.tenant$);

    this.form.reset({
      ...tenant,
      credentials: [],
    });

    this.form.controls.credentials.clear();
    for (const credential of tenant.credentials) {
      this.form.controls.credentials.push(
        new FormGroup({
          plainTextValue: new FormControl('', { nonNullable: true }),
          isExpiring: new FormControl(credential.expiration !== null, {
            nonNullable: true,
          }),
          expiration: new FormControl(
            new Date(credential.expiration ?? Date.now()).toISOString(),
            { nonNullable: true },
          ),
        }),
      );
    }

    this.editing = true;
  }

  async save() {}

  cancel() {
    this.editing = false;
  }
}
