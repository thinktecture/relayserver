import { AsyncPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal,
} from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import {
  IonBackButton,
  IonButton,
  IonButtons,
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  combineLatestWith,
  exhaustMap,
  filter,
  lastValueFrom,
  pipe,
  switchMap,
  tap,
} from 'rxjs';
import { ApiService } from '../api/api.service';
import { Tenant } from '../api/tenant.model';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';
import { EditTenantService } from '../edit-tenant/edit-tenant.service';
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
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantDetailsPage {
  private api = inject(ApiService);
  private editTenantService = inject(EditTenantService);

  name = input.required<string>();

  editing = signal(false);
  tenant$ = toObservable(this.name).pipe(
    combineLatestWith(toObservable(this.editing)),
    filter(([, editing]) => !editing),
    switchMap(([name]) => this.api.getSingleTenant(name)),
    tap((tenant) => this.updateForm(tenant)),
  );
  form = this.editTenantService.generateForm();

  save = rxMethod<void>(
    pipe(
      filter(() => this.form.valid),
      exhaustMap(() => {
        const formTenant = this.form.getRawValue();
        const tenant = {
          ...formTenant,
          credentials: formTenant.credentials.map((credential) => ({
            ...credential,
            id: credential.id ?? undefined,
          })),
        };

        return this.api.putTenant(tenant);
      }),
      tap(() => this.editing.set(false)),
    ),
  );

  get credentials(): FormArray<
    ReturnType<EditTenantService['generateCredentialForm']>
  > {
    return this.form.controls.credentials;
  }

  edit(): void {
    this.editing.set(true);
  }

  cancel(): void {
    this.editing.set(false);
  }

  private updateForm(tenant: Tenant): void {
    this.credentials.clear();

    tenant.credentials.forEach(() => {
      const control = this.editTenantService.generateCredentialForm();

      // plainTextValue is required, disable it to make the form valid
      control.controls.plainTextValue.disable();

      this.credentials.push(control);
    });

    this.form.reset(tenant);
  }
}
