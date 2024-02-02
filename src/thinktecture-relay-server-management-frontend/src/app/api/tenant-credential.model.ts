/** Represents a stored credential for a tenant. */
export interface TenantCredential {
  /** Unique identifier for this credential. */
  id: string;

  /** Plaintext value of this credential, only used for creating a new credential. */
  plainTextValue: string | null;

  /** Date and time this credential was created at, only used for existing credentials. */
  created: Date | null;

  /** Date and time this credential expires at. */
  expiration: Date | null;
}
