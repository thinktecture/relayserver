/** Represents a stored credential for a tenant. */
export interface TenantCredential {
  /** Unique identifier for this credential. */
  id: string;

  /** SHA256 or SHA512 value representing the value of this credential, only used for creating a new credential. */
  value: string | null;

  /** Plain text value, only used for creating a new credential. */
  plainTextValue: string | null;

  /** Date and time this credential was created at. */
  created: Date;

  /** Date and time this credential expires at. */
  expiration: Date | null;
}
