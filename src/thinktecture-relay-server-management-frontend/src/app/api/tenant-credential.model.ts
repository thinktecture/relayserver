import { NewTenantCredential } from './new-tenant-credential.model';

/** Represents a stored credential for a tenant. */
export interface TenantCredential
  extends Omit<NewTenantCredential, 'plainTextValue'> {
  /** Date and time this credential was created at. */
  created: string;
}
