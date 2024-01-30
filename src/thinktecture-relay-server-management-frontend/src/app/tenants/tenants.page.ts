import { Component, inject } from '@angular/core'
import { IonHeader, IonToolbar, IonTitle, IonContent } from '@ionic/angular/standalone'
import { HttpClient } from '@angular/common/http'
import { JsonPipe } from '@angular/common'
import { toSignal } from '@angular/core/rxjs-interop'

@Component({
  selector: 'app-tenants',
  templateUrl: 'tenants.page.html',
  styleUrls: ['tenants.page.scss'],
  standalone: true,
  imports: [IonHeader, IonToolbar, IonTitle, IonContent, JsonPipe],
})
export class TenantsPage {
  private httpClient = inject(HttpClient)

  tenants = toSignal(
    this.httpClient.get('/api/management/tenants', {
      headers: {
        'TT-Api-Key': 'readwrite-key',
      },
    }),
  )
}
