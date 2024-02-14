import { Injectable } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray } from '@angular/forms';

@Injectable({
  providedIn: 'root',
})
export class EditTenantService {
  // TODO: return type
  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
  generateForm() {
    return new FormGroup({
      name: new FormControl('', {
        nonNullable: true,
        validators: Validators.required,
      }),
      displayName: new FormControl<string | null>(null),
      description: new FormControl<string | null>(null),
      requireAuthentication: new FormControl(false, { nonNullable: true }),
      maximumConcurrentConnectorRequests: new FormControl(0, {
        nonNullable: true,
        validators: Validators.min(0),
      }),
      keepAliveInterval: new FormControl<string | null>(null),
      enableTracing: new FormControl<boolean | null>(null),
      reconnectMinimumDelay: new FormControl<string | null>(null),
      reconnectMaximumDelay: new FormControl<string | null>(null),
      credentials: new FormArray<
        ReturnType<EditTenantService['generateCredentialForm']>
      >([]),
    });
  }

  // TODO: return type
  // eslint-disable-next-line @typescript-eslint/explicit-function-return-type
  generateCredentialForm() {
    return new FormGroup({
      id: new FormControl<string | null>(null),
      plainTextValue: new FormControl(
        this.generateRandomString(),
        Validators.required,
      ),
      created: new FormControl('', { nonNullable: true }),
      isExpiring: new FormControl(false, { nonNullable: true }),
      expiration: new FormControl(new Date().toISOString(), {
        nonNullable: true,
      }),
    });
  }

  private generateRandomString(): string {
    const characters =
      '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz';
    return Array.from(crypto.getRandomValues(new Uint32Array(37)))
      .map((x) => characters[x % characters.length])
      .join('');
  }
}
