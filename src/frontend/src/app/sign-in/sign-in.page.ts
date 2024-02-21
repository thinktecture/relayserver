import {
  ChangeDetectionStrategy,
  Component,
  inject,
  viewChild,
} from '@angular/core';
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
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignInPage {
  private apiAuth = inject(ApiAuthStore);
  private router = inject(Router);
  private api = inject(ApiService);

  headerName = viewChild.required<IonInput>('headerName');
  form = viewChild.required(NgForm);

  model = { headerName: '', key: '' };

  constructor() {
    addIcons({ chevronForwardOutline });
  }

  ionViewDidEnter(): void {
    this.headerName().setFocus();
  }

  async done(): Promise<void> {
    if (!this.form().valid) {
      return;
    }

    this.apiAuth.update(this.model.headerName, this.model.key);

    try {
      await lastValueFrom(this.api.getTenantsPaged());
      this.router.navigate(['/']);
    } catch {
      // error toast is shown by API interceptor
    }
  }
}
