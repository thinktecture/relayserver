import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Connection } from './connection.model';
import { NewTenant } from './new-tenant.model';
import { Page } from './page.model';
import { Tenant } from './tenant.model';
import { DateStatistic } from './date-statistic';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = '/management';

  deleteTenant(tenantName: string): Observable<void> {
    const url = `${this.baseUrl}/tenants/${tenantName}`;

    return this.http.delete<void>(url);
  }

  getSingleTenant(tenantName: string): Observable<Tenant> {
    const url = `${this.baseUrl}/tenants/${tenantName}`;

    return this.http.get<Tenant>(url);
  }

  putTenant(tenant: NewTenant): Observable<void> {
    const url = `${this.baseUrl}/tenants/${tenant.name}`;

    return this.http.put<void>(url, tenant);
  }

  getTenantsPaged(
    skip?: number,
    take?: number,
    filter?: string,
  ): Observable<Page<Tenant>> {
    const url = `${this.baseUrl}/tenants`;

    let params = new HttpParams();
    if (skip !== undefined) {
      params = params.set('skip', skip);
    }
    if (take !== undefined) {
      params = params.set('take', take);
    }
    if (filter !== undefined && filter !== '') {
      params = params.set('filter', filter);
    }

    return this.http.get<Page<Tenant>>(url, { params });
  }

  postTenant(tenant: NewTenant): Observable<void> {
    const url = `${this.baseUrl}/tenants`;

    return this.http.post<void>(url, tenant);
  }

  getTenantConnections(tenantName: string): Observable<Connection[]> {
    const url = `${this.baseUrl}/tenants/${tenantName}/connections`;

    return this.http.get<Connection[]>(url);
  }

  getStatistics(): Observable<DateStatistic[]> {
    const url = `${this.baseUrl}/statistics`;

    return this.http.get<DateStatistic[]>(url);
  }
}
