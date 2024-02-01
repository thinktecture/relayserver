import { TenantCredential } from './tenant-credential.model';

/** Represents a tenant. */
export interface Tenant {
  /** Unique name of the tenant. */
  name: string;

  /** Display name of the tenant. */
  displayName?: string | undefined;

  /** Optional, longer, textual description of this tenant. */
  description?: string | undefined;

  /** Whether authentication is required for relaying a request to this tenant. */
  requireAuthentication?: boolean | undefined;

  /** Maximum amount of concurrent requests a connector should receive. */
  maximumConcurrentConnectorRequests?: number | undefined;

  /** Interval used to send keep alive pings between the server and a connector. */
  keepAliveInterval?: string;

  /** Whether tracing is enabled for all requests of this particular tenant. */
  enableTracing?: boolean | undefined;

  /** Minimum delay to wait for until a reconnect of a connector should be attempted again. */
  reconnectMinimumDelay?: string;

  /** Maximum delay to wait for until a reconnect of a connector should be attempted again. */
  reconnectMaximumDelay?: string;

  /** List of tenant credentials. */
  credentials: TenantCredential[];
}
