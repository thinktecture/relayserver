import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import {
  IonButton,
  IonButtons,
  IonCheckbox,
  IonContent,
  IonHeader,
  IonIcon,
  IonInput,
  IonItem,
  IonLabel,
  IonList,
  IonTextarea,
  IonTitle,
  IonToolbar,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addCircle } from 'ionicons/icons';

@Component({
  selector: 'app-new-tenant',
  templateUrl: './new-tenant.component.html',
  styleUrls: ['./new-tenant.component.scss'],
  standalone: true,
  imports: [
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
    IonLabel,
  ],
})
export class NewTenantComponent {
  @Output() dismiss = new EventEmitter<void>();

  @ViewChild('tenantName') tenantName: IonInput | null = null;

  constructor() {
    addIcons({ addCircle });
  }

  focus() {
    this.tenantName?.setFocus();
  }
}
