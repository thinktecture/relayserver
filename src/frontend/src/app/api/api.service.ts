import { Observable, map } from 'rxjs';
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Tenant } from './tenant.model';
import { Page } from './page.model';
import { NewTenant } from './new-tenant.model';

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

    return this.http
      .get<TenantDto>(url)
      .pipe(map((tenantDto) => this.tenantFromDto(tenantDto)));
  }

  putTenant(tenant: NewTenant): Observable<void> {
    const url = `${this.baseUrl}/tenants/${tenant.name}`;

    return this.http.put<void>(url, this.tenantToDto(tenant));
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

    return this.http.get<Page<TenantDto>>(url, { params }).pipe(
      map((page) => ({
        ...page,
        results: page.results.map((tenantDto) => this.tenantFromDto(tenantDto)),
      })),
    );
  }

  postTenant(tenant: NewTenant): Observable<void> {
    const url = `${this.baseUrl}/tenants`;

    return this.http.post<void>(url, this.tenantToDto(tenant));
  }

  private tenantToDto(tenant: NewTenant): NewTenantDto {
    return {
      ...tenant,
      keepAliveInterval: this.intervalToDto(tenant.keepAliveInterval),
      reconnectMinimumDelay: this.intervalToDto(tenant.reconnectMinimumDelay),
      reconnectMaximumDelay: this.intervalToDto(tenant.reconnectMaximumDelay),
    };
  }

  private intervalToDto(totalSeconds: number | null): string | null {
    if (totalSeconds === null) {
      return null;
    }

    const totalMinutes = Math.trunc(totalSeconds / 60);
    const totalHours = Math.trunc(totalMinutes / 60);
    const totalDays = Math.trunc(totalHours / 24);

    const format = Intl.NumberFormat(undefined, { minimumIntegerDigits: 2 });
    const seconds = format.format(totalSeconds % 60);
    const minutes = format.format(totalMinutes % 60);
    const hours = format.format(totalHours % 24);
    const days = totalDays.toString();

    const interval = `${hours}:${minutes}:${seconds}`;
    if (totalDays === 0) {
      return interval;
    }

    return `${days}.${interval}`;
  }

  private tenantFromDto(tenantDto: TenantDto): Tenant {
    return {
      ...tenantDto,
      keepAliveInterval: this.intervalFromDto(tenantDto.keepAliveInterval),
      reconnectMinimumDelay: this.intervalFromDto(
        tenantDto.reconnectMinimumDelay,
      ),
      reconnectMaximumDelay: this.intervalFromDto(
        tenantDto.reconnectMaximumDelay,
      ),
    };
  }

  private intervalFromDto(interval: string | null): number | null {
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

/** Overrides for the tenant data transfer objects. */
interface TenantDtoOverrides {
  /** Interval (hh:mm:ss) used to send keep alive pings between the server and a connector. */
  keepAliveInterval: string | null;

  /** Minimum delay (hh:mm:ss) to wait for until a reconnect of a connector should be attempted again. */
  reconnectMinimumDelay: string | null;

  /** Maximum delay (hh:mm:ss) to wait for until a reconnect of a connector should be attempted again. */
  reconnectMaximumDelay: string | null;
}

/** Data transfer object for a new tenant. */
interface NewTenantDto
  extends Omit<NewTenant, keyof TenantDtoOverrides>,
    TenantDtoOverrides {}

/** Data transfer object for a stored tenant. */
interface TenantDto
  extends Omit<Tenant, keyof TenantDtoOverrides>,
    TenantDtoOverrides {}
