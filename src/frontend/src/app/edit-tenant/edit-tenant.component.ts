import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  FormArray,
  FormGroupDirective,
  ReactiveFormsModule,
} from '@angular/forms';
import { Clipboard } from '@capacitor/clipboard';
import {
  IonButton,
  IonCheckbox,
  IonIcon,
  IonInput,
  IonItem,
  IonLabel,
  IonList,
  IonTextarea,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addCircle, copyOutline, removeCircle } from 'ionicons/icons';
import { EditTenantService } from './edit-tenant.service';
import { IntervalInputComponent } from '../interval-input/interval-input.component';
import { DateInputComponent } from '../date-input/date-input.component';

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
    IntervalInputComponent,
    DateInputComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditTenantComponent {
  private formGroupDirective = inject(FormGroupDirective);
  private editTenantService = inject(EditTenantService);

  constructor() {
    addIcons({ addCircle, removeCircle, copyOutline });
  }

  get form(): ReturnType<EditTenantService['generate']> {
    return this.formGroupDirective.form;
  }

  get credentials(): FormArray<
    ReturnType<EditTenantService['generateCredential']>
  > {
    return this.form.controls.credentials;
  }

  addCredential(): void {
    this.credentials.push(this.editTenantService.generateCredential());
  }

  removeCredential(index: number): void {
    this.credentials?.removeAt(index);
  }

  copyCredential(index: number): void {
    const value = this.credentials?.at(index).value.plainTextValue;
    if (value) {
      Clipboard.write({ string: value });
    }
  }
}
