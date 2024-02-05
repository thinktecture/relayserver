import { Component, Input, ViewChild, inject } from '@angular/core';
import {
  AbstractControl,
  ControlContainer,
  FormArray,
  FormControl,
  FormGroup,
  FormGroupDirective,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Clipboard } from '@capacitor/clipboard';
import {
  IonButton,
  IonCheckbox,
  IonDatetime,
  IonDatetimeButton,
  IonIcon,
  IonInput,
  IonItem,
  IonList,
  IonModal,
  IonTextarea,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addCircle, copyOutline, removeCircle } from 'ionicons/icons';

@Component({
  selector: 'app-edit-tenant',
  templateUrl: './edit-tenant.component.html',
  styleUrls: ['./edit-tenant.component.scss'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    IonList,
    IonItem,
    IonInput,
    IonTextarea,
    IonCheckbox,
    IonButton,
    IonIcon,
    IonDatetimeButton,
    IonModal,
    IonDatetime,
  ],
})
export class EditTenantComponent {
  private formGroupDirective = inject(FormGroupDirective);

  @ViewChild('tenantName') tenantName: IonInput | undefined;

  constructor() {
    addIcons({ addCircle, removeCircle, copyOutline });
  }

  static generateForm() {
    return new FormGroup({
      name: new FormControl('', {
        nonNullable: true,
        validators: Validators.required,
      }),
      displayName: new FormControl(''),
      description: new FormControl(''),
      requireAuthentication: new FormControl(false, { nonNullable: true }),
      maximumConcurrentConnectorRequests: new FormControl(0, {
        nonNullable: true,
        validators: Validators.min(0),
      }),
      keepAliveInterval: new FormControl(0, Validators.min(0)),
      enableTracing: new FormControl(false),
      reconnectMinimumDelay: new FormControl(0, Validators.min(0)),
      reconnectMaximumDelay: new FormControl(0, Validators.min(0)),
      credentials: new FormArray<
        ReturnType<typeof EditTenantComponent.generateCredentialForm>
      >([]),
    });
  }

  get form() {
    return this.formGroupDirective.form as ReturnType<
      typeof EditTenantComponent.generateForm
    >;
  }

  get credentials() {
    return this.form.controls.credentials;
  }

  focus() {
    this.tenantName?.setFocus();
  }

  addCredential() {
    this.credentials.push(EditTenantComponent.generateCredentialForm());
  }

  removeCredential(index: number) {
    this.credentials?.removeAt(index);
  }

  copyCredential(index: number) {
    const value = this.credentials?.at(index).value.plainTextValue;
    if (value !== undefined) {
      Clipboard.write({ string: value });
    }
  }

  private static generateCredentialForm() {
    return new FormGroup({
      plainTextValue: new FormControl(
        EditTenantComponent.generateRandomString(),
        {
          nonNullable: true,
          validators: Validators.required,
        },
      ),
      isExpiring: new FormControl(false, { nonNullable: true }),
      expiration: new FormControl(new Date().toISOString(), {
        nonNullable: true,
      }),
    });
  }

  private static generateRandomString(): string {
    const characters =
      '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
    return Array.from(crypto.getRandomValues(new Uint32Array(37)))
      .map((x) => characters[x % characters.length])
      .join('');
  }
}
