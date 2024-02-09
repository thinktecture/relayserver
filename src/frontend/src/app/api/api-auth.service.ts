import { Injectable } from '@angular/core';

const API_HEADER_NAME = 'api-header-name';
const API_KEY = 'api-key';

@Injectable({
  providedIn: 'root',
})
export class ApiAuthService {
  private backingHeaderName?: string;
  private backingKey?: string;

  constructor() {
    this.backingHeaderName = localStorage.getItem(API_HEADER_NAME) ?? undefined;
    this.backingKey = localStorage.getItem(API_KEY) ?? undefined;
  }

  get headerName(): string | undefined {
    return this.backingHeaderName;
  }

  set headerName(headerName: string) {
    this.backingHeaderName = headerName;
    localStorage.setItem(API_HEADER_NAME, headerName);
  }

  get key(): string | undefined {
    return this.backingKey;
  }

  set key(key: string) {
    this.backingKey = key;
    localStorage.setItem(API_KEY, key);
  }
}
