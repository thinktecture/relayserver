import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormGroupDirective, ReactiveFormsModule } from '@angular/forms';
import { Clipboard } from '@capacitor/clipboard';
import {
  IonButton,
  IonCheckbox,
  IonDatetime,
  IonDatetimeButton,
  IonIcon,
  IonInput,
  IonItem,
  IonLabel,
  IonList,
  IonModal,
  IonTextarea,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addCircle, copyOutline, removeCircle } from 'ionicons/icons';
import { EditTenantService } from './edit-tenant.service';

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
    IonLabel,
    IonDatetimeButton,
    IonModal,
    IonDatetime,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditTenantComponent {
  private formGroupDirective = inject(FormGroupDirective);
  private editTenantService = inject(EditTenantService);

  constructor() {
    addIcons({ addCircle, removeCircle, copyOutline });
  }

  get form() {
    return this.formGroupDirective.form as ReturnType<
      EditTenantService['generateForm']
    >;
  }

  get credentials() {
    return this.form.controls.credentials;
  }

  addCredential() {
    this.credentials.push(this.editTenantService.generateCredentialForm());
  }

  removeCredential(index: number) {
    this.credentials?.removeAt(index);
  }

  copyCredential(index: number) {
    const value = this.credentials?.at(index).value.plainTextValue;
    if (value) {
      Clipboard.write({ string: value });
    }
  }
}
