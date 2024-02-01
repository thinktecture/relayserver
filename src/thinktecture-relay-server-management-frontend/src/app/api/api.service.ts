import { Observable } from 'rxjs';
import { Injectable, InjectionToken, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Tenant } from './tenant.model';
import { Page } from './page.model';

export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

@Injectable()
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = inject(API_BASE_URL);

  deleteTenant(tenantName: string): Observable<void> {
    const url = `${this.baseUrl}/management/tenants/${tenantName}`;

    return this.http.delete<void>(url);
  }

  getSingleTenant(tenantName: string): Observable<Tenant> {
    const url = `${this.baseUrl}/management/tenants/${tenantName}`;

    return this.http.get<Tenant>(url);
  }

  putTenant(body: Tenant): Observable<void> {
    const url = `${this.baseUrl}/management/tenants/${body.name}`;

    return this.http.put<void>(url, { body });
  }

  getTenantsPaged(skip?: number, take?: number): Observable<Page<Tenant>> {
    const url = `${this.baseUrl}/management/tenants`;

    let params = new HttpParams();
    if (skip !== undefined) {
      params = params.set('skip', skip);
    }
    if (take !== undefined) {
      params = params.set('take', take);
    }

    return this.http.get<Page<Tenant>>(url, { params });
  }

  postTenant(body: Tenant): Observable<void> {
    const url = `${this.baseUrl}/management/tenants`;

    return this.http.post<void>(url, { body });
  }
}
