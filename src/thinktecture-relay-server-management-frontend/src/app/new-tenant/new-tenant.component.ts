import { JsonPipe } from '@angular/common';
import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import {
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
    JsonPipe,
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
    IonLabel,
  ],
})
export class NewTenantComponent {
  @Output() dismiss = new EventEmitter<void>();

  @ViewChild('tenantName') tenantName: IonInput | null = null;

  form = new FormGroup({
    name: new FormControl('', Validators.required),
    displayName: new FormControl(''),
    description: new FormControl(''),
    requireAuthentication: new FormControl(false),
    maximumConcurrentConnectorRequests: new FormControl(0),
    keepAliveInterval: new FormControl(0),
    enableTracing: new FormControl(false),
    reconnectMinimumDelay: new FormControl(0),
    reconnectMaximumDelay: new FormControl(0),
  });

  constructor() {
    addIcons({ addCircle });
  }

  focus() {
    this.tenantName?.setFocus();
  }
}
