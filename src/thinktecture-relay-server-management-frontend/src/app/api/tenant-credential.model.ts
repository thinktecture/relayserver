import { NewTenantCredential } from './new-tenant-credential.model';

/** Represents a stored credential for a tenant. */
export interface TenantCredential
  extends Omit<NewTenantCredential, 'plainTextValue'> {
  /** Unique identifier for this credential. */
  id: string;

  /** Date and time this credential was created at. */
  created: string;

  /** Date and time this credential expires at. */
  expiration: string | null;
}
