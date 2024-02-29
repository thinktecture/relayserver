import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Tenant } from '../api/tenant.model';
import { IonList, IonItem, IonLabel } from '@ionic/angular/standalone';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-view-tenant',
  templateUrl: './view-tenant.component.html',
  styleUrls: ['./view-tenant.component.scss'],
  standalone: true,
  imports: [DatePipe, IonList, IonItem, IonLabel],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewTenantComponent {
  tenant = input.required<Tenant>();
}
