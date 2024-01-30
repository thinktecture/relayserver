import { Component, Input } from '@angular/core';
import { IonicModule } from '@ionic/angular';

@Component({
  selector: 'app-tenant-details',
  templateUrl: './tenant-details.page.html',
  styleUrls: ['./tenant-details.page.scss'],
  standalone: true,
  imports: [IonicModule],
})
export class TenantDetailsPage {
  @Input() name = '';
}
