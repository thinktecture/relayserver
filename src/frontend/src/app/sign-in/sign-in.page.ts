import { Component, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
import { addIcons } from 'ionicons';
import { chevronForwardOutline } from 'ionicons/icons';
import { Router } from '@angular/router';
import { ApiAuthStore } from '../api/api-auth.store';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.page.html',
  styleUrls: ['./sign-in.page.scss'],
  standalone: true,
  imports: [
    FormsModule,
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
export class SignInPage {
  private apiAuth = inject(ApiAuthStore);
  private router = inject(Router);

  @ViewChild('headerName') headerName?: IonInput;

  model = { headerName: '', key: '' };

  constructor() {
    addIcons({ chevronForwardOutline });
  }

  ionViewDidEnter() {
    this.headerName?.setFocus();
  }

  done() {
    this.apiAuth.update(this.model.headerName, this.model.key);
    this.router.navigate(['/tabs', 'tenants']);
    // TODO: check access
  }
}
