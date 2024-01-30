import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { IonicModule } from '@ionic/angular';
import { map } from 'rxjs';

@Component({
  selector: 'app-tenant-details',
  templateUrl: './tenant-details.page.html',
  styleUrls: ['./tenant-details.page.scss'],
  standalone: true,
  imports: [IonicModule],
})
export class TenantDetailsPage {
  name = toSignal(inject(ActivatedRoute).params.pipe(map((p) => p['name'])));
}
