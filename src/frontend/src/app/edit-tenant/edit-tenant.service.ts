import { Injectable } from '@angular/core';
import { FormGroup, FormControl, Validators, FormArray } from '@angular/forms';
import { NewTenant } from '../api/new-tenant.model';
import { NewTenantCredential } from '../api/new-tenant-credential.model';

type GenericFormGroup<T> = FormGroup<{
  [k in keyof T]-?: T[k] extends (infer U)[]
    ? FormArray<GenericFormGroup<U>>
    : FormControl<UndefinedToNull<T[k]>>;
}>;

type UndefinedToNull<T> = undefined extends T
  ? Exclude<T, undefined> | null
  : T;

@Injectable({
  providedIn: 'root',
})
export class EditTenantService {
  generate(): GenericFormGroup<NewTenant> {
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
        ReturnType<EditTenantService['generateCredential']>
      >([]),
    });
  }

  generateCredential(): GenericFormGroup<NewTenantCredential> {
    return new FormGroup({
      id: new FormControl<string | null>(null),
      plainTextValue: new FormControl(
        this.generateRandomString(),
        Validators.required,
      ),
      created: new FormControl<string | null>(null),
      expiration: new FormControl<string | null>(null),
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
