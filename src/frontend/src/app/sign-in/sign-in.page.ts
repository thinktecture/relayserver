import { JsonPipe } from '@angular/common';
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
import { ApiAuthService } from '../api/api-auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.page.html',
  styleUrls: ['./sign-in.page.scss'],
  standalone: true,
  imports: [
    FormsModule,
    JsonPipe,
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
  private apiAuth = inject(ApiAuthService);
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
    this.apiAuth.headerName = this.model.headerName;
    this.apiAuth.key = this.model.key;
    this.router.navigate(['/']);
    // TODO: check access
  }
}
