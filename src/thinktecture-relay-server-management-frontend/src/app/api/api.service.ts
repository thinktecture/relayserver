import { Observable } from 'rxjs';
import { Injectable, InjectionToken, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

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

/** Represents a tenant. */
export interface Tenant {
  /** Unique name of the tenant. */
  name: string;

  /** Display name of the tenant. */
  displayName?: string | undefined;

  /** Optional, longer, textual description of this tenant. */
  description?: string | undefined;

  /** Whether authentication is required for relaying a request to this tenant. */
  requireAuthentication?: boolean | undefined;

  /** Maximum amount of concurrent requests a connector should receive. */
  maximumConcurrentConnectorRequests?: number | undefined;

  /** Interval used to send keep alive pings between the server and a connector. */
  keepAliveInterval?: string;

  /** Whether tracing is enabled for all requests of this particular tenant. */
  enableTracing?: boolean | undefined;

  /** Minimum delay to wait for until a reconnect of a connector should be attempted again. */
  reconnectMinimumDelay?: string;

  /** Maximum delay to wait for until a reconnect of a connector should be attempted again. */
  reconnectMaximumDelay?: string;

  /** List of tenant credentials. */
  credentials: TenantCredential[];
}

/** Represents a stored credential for a tenant. */
export interface TenantCredential {
  /** Unique identifier for this credential. */
  id: string;

  /** SHA256 or SHA512 value representing the value of this credential, only used for creating a new credential. */
  value?: string | undefined;

  /** Plain text value, only used for creating a new credential. */
  plainTextValue?: string | undefined;

  /** Date and time this credential was created at. */
  created: Date;

  /** Date and time this credential expires at. */
  expiration?: Date | undefined;
}

/** Single page of a paginated result. */
export interface Page<T> {
  /** Results for this page. */
  results: T[];

  /** Total amount of data entries available. */
  totalCount: number;

  /** Starting index of the results within all available entries. */
  offset: number;

  /** Requested maximum size of the results. */
  pageSize: number;
}
