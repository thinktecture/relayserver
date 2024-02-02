import { NewTenant } from './new-tenant.model';
import { TenantCredential } from './tenant-credential.model';

/** Represents a stored tenant. */
export interface Tenant extends Omit<NewTenant, 'credentials'> {
  /** List of tenant credentials. */
  credentials: TenantCredential[];
}
