import { TenantCredential } from './tenant-credential.model';

/** Represents a tenant. */
export interface Tenant {
  /** Unique name of the tenant. */
  name: string;

  /** Display name of the tenant. */
  displayName: string | null;

  /** Optional, longer, textual description of this tenant. */
  description: string | null;

  /** Whether authentication is required for relaying a request to this tenant. */
  requireAuthentication: boolean | null;

  /** Maximum amount of concurrent requests a connector should receive. */
  maximumConcurrentConnectorRequests: number | null;

  /** Interval in seconds used to send keepalive pings between the server and a connector. */
  keepAliveInterval: number | null;

  /** Whether tracing is enabled for all requests of this particular tenant. */
  enableTracing: boolean | null;

  /** Minimum delay in seconds to wait for until a reconnect of a connector should be attempted again. */
  reconnectMinimumDelay: number | null;

  /** Maximum delay in seconds to wait for until a reconnect of a connector should be attempted again. */
  reconnectMaximumDelay: number | null;

  /** List of tenant credentials. */
  credentials: TenantCredential[];
}
