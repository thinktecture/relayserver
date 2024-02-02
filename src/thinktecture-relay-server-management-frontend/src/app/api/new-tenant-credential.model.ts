/** Represents a new credential for a tenant. */
export interface NewTenantCredential {
  /** Plaintext value of this credential. */
  plainTextValue: string;

  /** Date and time this credential expires at. */
  expiration: string | null;
}
