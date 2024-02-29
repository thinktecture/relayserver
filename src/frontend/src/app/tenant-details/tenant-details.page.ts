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
  IonItem,
  IonList,
  IonNote,
  IonProgressBar,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import {
  combineLatestWith,
  exhaustMap,
  filter,
  map,
  pipe,
  switchMap,
  tap,
} from 'rxjs';
import { ApiService } from '../api/api.service';
import { Tenant } from '../api/tenant.model';
import { EditTenantComponent } from '../edit-tenant/edit-tenant.component';
import { EditTenantService } from '../edit-tenant/edit-tenant.service';
import { ViewTenantComponent } from '../view-tenant/view-tenant.component';
import { tapResponse } from '@ngrx/operators';

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
    IonProgressBar,
    IonList,
    IonItem,
    IonNote,
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
  loading = signal(false);
  error = signal(false);
  tenant$ = toObservable(this.name).pipe(
    combineLatestWith(toObservable(this.editing)),
    filter(([, editing]) => !editing),
    tap(() => this.loading.set(true)),
    switchMap(([name]) =>
      this.api.getSingleTenant(name).pipe(
        tapResponse({
          next: (tenant) => {
            this.updateForm(tenant);
            this.loading.set(false);
          },
          error: () => {
            // error toast is shown by API interceptor
            this.error.set(true);
            this.loading.set(false);
          },
        }),
      ),
    ),
  );
  form = this.editTenantService.generate();

  save = rxMethod<void>(
    pipe(
      filter(() => this.form.valid),
      tap(() => this.loading.set(true)),
      map(() => this.form.getRawValue()),
      map((formTenant) => ({
        ...formTenant,
        credentials: formTenant.credentials.map((credential) => ({
          ...credential,
          id: credential.id ?? undefined,
          created: credential.created ?? undefined,
        })),
      })),
      exhaustMap((tenant) =>
        this.api.putTenant(tenant).pipe(
          tapResponse({
            next: () => {
              this.loading.set(false);
              this.editing.set(false);
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

  get credentials(): FormArray<
    ReturnType<EditTenantService['generateCredential']>
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
      const control = this.editTenantService.generateCredential();

      // plainTextValue is required, disable it to make the form valid
      control.controls.plainTextValue.disable();

      this.credentials.push(control);
    });

    this.form.reset(tenant);
  }
}
