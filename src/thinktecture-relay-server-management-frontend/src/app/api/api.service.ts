import { Observable, map } from 'rxjs';
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

    return this.http.get<TenantDto>(url).pipe(map(ApiService.tenantFromDto));
  }

  putTenant(tenant: Tenant): Observable<void> {
    const url = `${this.baseUrl}/management/tenants/${tenant.name}`;

    return this.http.put<void>(url, { body: ApiService.tenantToDto(tenant) });
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

    return this.http.get<Page<TenantDto>>(url, { params }).pipe(
      map((page) => ({
        ...page,
        results: page.results.map(ApiService.tenantFromDto),
      })),
    );
  }

  postTenant(tenant: Tenant): Observable<void> {
    const url = `${this.baseUrl}/management/tenants`;

    return this.http.post<void>(url, { body: ApiService.tenantToDto(tenant) });
  }

  private static tenantToDto(tenant: Tenant): TenantDto {
    return {
      ...tenant,
      keepAliveInterval: ApiService.intervalToDto(tenant.keepAliveInterval),
      reconnectMinimumDelay: ApiService.intervalToDto(
        tenant.reconnectMinimumDelay,
      ),
      reconnectMaximumDelay: ApiService.intervalToDto(
        tenant.reconnectMaximumDelay,
      ),
    };
  }

  private static intervalToDto(seconds: number | null): string | null {
    if (seconds === null) {
      return null;
    }

    const minutes = Math.trunc(seconds / 60);
    const hours = Math.trunc(minutes / 60);
    const days = Math.trunc(hours / 24);
    return `${days}.${hours % 24}:${minutes % 60}:${seconds % 60}`;
  }

  private static tenantFromDto(tenantDto: TenantDto): Tenant {
    return {
      ...tenantDto,
      keepAliveInterval: ApiService.intervalFromDto(
        tenantDto.keepAliveInterval,
      ),
      reconnectMinimumDelay: ApiService.intervalFromDto(
        tenantDto.reconnectMinimumDelay,
      ),
      reconnectMaximumDelay: ApiService.intervalFromDto(
        tenantDto.reconnectMaximumDelay,
      ),
    };
  }

  private static intervalFromDto(interval: string | null): number | null {
    if (interval === null) {
      return null;
    }

    const [daysAndHours, minutes, seconds] = interval.split(':');
    let days = '0';
    let hours = daysAndHours;
    if (hours.includes('.')) {
      [days, hours] = hours.split('.');
    }

    return (
      ((Number(days) * 24 + Number(hours)) * 60 + Number(minutes)) * 60 +
      Number(seconds)
    );
  }
}

/** Represents a tenant. */
interface TenantDto
  extends Omit<
    Tenant,
    'keepAliveInterval' | 'reconnectMinimumDelay' | 'reconnectMaximumDelay'
  > {
  /** Interval (hh:mm:ss) used to send keep alive pings between the server and a connector. */
  keepAliveInterval: string | null;

  /** Minimum delay (hh:mm:ss) to wait for until a reconnect of a connector should be attempted again. */
  reconnectMinimumDelay: string | null;

  /** Maximum delay (hh:mm:ss) to wait for until a reconnect of a connector should be attempted again. */
  reconnectMaximumDelay: string | null;
}
