import { AsyncPipe, JsonPipe } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { IonicModule } from '@ionic/angular';
import { switchMap } from 'rxjs';
import { ApiService } from '../api/api.service';

@Component({
  selector: 'app-tenant-details',
  templateUrl: './tenant-details.page.html',
  styleUrls: ['./tenant-details.page.scss'],
  standalone: true,
  imports: [AsyncPipe, JsonPipe, IonicModule],
})
export class TenantDetailsPage {
  private api = inject(ApiService);

  name = input.required<string>();

  details = toObservable(this.name).pipe(
    switchMap((name) => this.api.getSingleTenant(name)),
  );
}
