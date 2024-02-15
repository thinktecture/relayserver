/** Represents a new credential for a tenant. */
export interface NewTenantCredential {
  /** Unique identifier for this credential. */
  id?: string;

  /** Plaintext value of this credential. */
  plainTextValue: string | null;

  /** Date and time this credential was created at. */
  created?: string;

  /** Date and time this credential expires at. */
  expiration: string | null;
}
