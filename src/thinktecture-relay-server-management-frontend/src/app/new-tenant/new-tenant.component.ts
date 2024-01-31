import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
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
  ],
})
export class NewTenantComponent {
  @Output() dismiss = new EventEmitter<void>();

  @ViewChild('tenantName') tenantName: IonInput | null = null;

  focus() {
    this.tenantName?.setFocus();
  }
}
