import {
  Component,
  EventEmitter,
  Output,
  ViewChild,
  inject,
} from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  IonButton,
  IonButtons,
  IonCheckbox,
  IonContent,
  IonDatetime,
  IonDatetimeButton,
  IonHeader,
  IonIcon,
  IonInput,
  IonItem,
  IonList,
  IonModal,
  IonTextarea,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addCircle, removeCircle } from 'ionicons/icons';
import { ApiService } from '../api/api.service';
import { lastValueFrom } from 'rxjs';
import { Tenant } from '../api/tenant.model';
import { Router } from '@angular/router';

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
    IonTextarea,
    IonCheckbox,
    IonIcon,
    IonDatetimeButton,
    IonModal,
    IonDatetime,
  ],
})
export class NewTenantComponent {
  private api = inject(ApiService);
  private router = inject(Router);

  @Output() dismiss = new EventEmitter<void>();

  @ViewChild('tenantName') tenantName: IonInput | null = null;

  form = new FormGroup({
    name: new FormControl('', Validators.required),
    displayName: new FormControl(''),
    description: new FormControl(''),
    requireAuthentication: new FormControl(false),
    maximumConcurrentConnectorRequests: new FormControl(0, Validators.min(0)),
    keepAliveInterval: new FormControl(0, Validators.min(0)),
    enableTracing: new FormControl(false),
    reconnectMinimumDelay: new FormControl(0, Validators.min(0)),
    reconnectMaximumDelay: new FormControl(0, Validators.min(0)),
    credentials: new FormArray<FormGroup<FormCredential>>([]),
  });

  constructor() {
    addIcons({ addCircle, removeCircle });
  }

  get credentials() {
    return this.form.controls.credentials;
  }

  focus() {
    this.tenantName?.setFocus();
  }

  async create() {
    const tenant = { ...this.form.getRawValue(), credentials: [] } as Tenant;
    await lastValueFrom(this.api.postTenant(tenant));
    this.dismiss.emit();
    this.router.navigate(['/tabs', 'tenants', tenant.name]);
  }

  addCredential() {
    this.credentials.push(
      new FormGroup({
        value: new FormControl('', { nonNullable: true }),
        isExpiring: new FormControl(false, { nonNullable: true }),
        expiration: new FormControl(new Date().toISOString(), {
          nonNullable: true,
        }),
      }),
    );
  }
}

interface FormCredential {
  value: FormControl<string>;
  isExpiring: FormControl<boolean>;
  expiration: FormControl<string>;
}
