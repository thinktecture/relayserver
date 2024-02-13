import { Component, ViewChild, inject } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
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
import { ApiService } from '../api/api.service';
import { lastValueFrom } from 'rxjs';

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
  private api = inject(ApiService);

  @ViewChild('headerName') headerName?: IonInput;
  @ViewChild('form') form?: NgForm;

  model = { headerName: '', key: '' };

  constructor() {
    addIcons({ chevronForwardOutline });
  }

  ionViewDidEnter() {
    this.headerName?.setFocus();
  }

  async done() {
    if (!this.form?.valid) {
      return;
    }

    this.apiAuth.update(this.model.headerName, this.model.key);

    try {
      await lastValueFrom(this.api.getTenantsPaged());
    } catch {
      return;
    }

    this.router.navigate(['/']);
  }
}
