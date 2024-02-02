/** Represents a stored credential for a tenant. */
export interface TenantCredential {
  /** Unique identifier for this credential. */
  id: string;

  /** SHA256 or SHA512 value representing the value of this credential, only used for creating a new credential. */
  value: string | null;

  /** Date and time this credential was created at, only used for existing credentials. */
  created: Date | null;

  /** Date and time this credential expires at. */
  expiration: Date | null;
}
