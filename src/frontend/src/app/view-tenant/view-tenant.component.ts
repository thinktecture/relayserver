import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { IonIcon, IonItem, IonLabel, IonList } from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { keyOutline } from 'ionicons/icons';
import { Tenant } from '../api/tenant.model';

@Component({
  selector: 'app-view-tenant',
  templateUrl: './view-tenant.component.html',
  styleUrls: ['./view-tenant.component.scss'],
  standalone: true,
  imports: [DatePipe, IonList, IonItem, IonLabel, IonIcon],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewTenantComponent {
  tenant = input.required<Tenant>();

  constructor() {
    addIcons({ keyOutline });
  }
}
