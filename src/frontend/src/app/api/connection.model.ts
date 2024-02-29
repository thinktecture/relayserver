/** Represents a single connection of tenants' on-premises installation to a relay server. */
export interface Connection {
  /** The transport-specific connection ID. */
  id: string;

  /** The unique name of the tenant. */
  tenantName: string;

  /** The unique ID of the relay server instance this connection is held to. */
  originId: string;

  /** The time when this connection was opened. */
  connectTime: string;

  /** The time when this connection was closed. */
  disconnectTime: string | null;

  /** The last time when the last message through this connection was recorded. */
  lastSeenTime: string | null;

  /** The remote IP address of the connector. */
  remoteIpAddress: string | null;
}
