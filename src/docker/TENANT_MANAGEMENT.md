# Tenant Management for RelayServer

## Overview

This document describes how to manage tenants in the Keycloak identity provider for RelayServer.

## Prerequisites

1. Keycloak container must be running (`relay-identityprovider`)
2. The `keycloak-admin` client must be configured in the realm (already included in the updated `relayserver-realm.json`)

## Creating New Tenants

### Using the Shell Script

The `create-tenant.sh` script allows you to create new tenant clients via the Keycloak API.

#### Usage

```bash
./create-tenant.sh <tenant_name> <display_name> [description] [client_secret]
```

#### Parameters

- `tenant_name` (required): The unique identifier for the tenant (e.g., `TestTenant3`)
- `display_name` (required): Human-readable name for the tenant (e.g., `"Test Tenant 3"`)
- `description` (optional): Description of the tenant (default: "Tenant client for RelayServer")
- `client_secret` (optional): The client secret to use (default: `<Strong!Passw0rd>`)

#### Examples

Create a tenant with default settings:
```bash
./create-tenant.sh TestTenant3 "Test Tenant 3"
```

Create a tenant with custom description and secret:
```bash
./create-tenant.sh MyTenant "My Custom Tenant" "Production tenant for ACME Corp" "MySecurePassword123!"
```

#### Environment Variables

- `KEYCLOAK_URL`: The Keycloak server URL (default: `http://localhost:5002`)
- `ADMIN_CLIENT_SECRET`: The keycloak-admin client secret (default: `<Strong!Passw0rd>`)

Example with custom Keycloak URL:
```bash
KEYCLOAK_URL=http://keycloak.example.com:8080 ./create-tenant.sh TestTenant4 "Test Tenant 4"
```

## What the Script Does

1. **Authenticates**: Obtains an access token using the `keycloak-admin` service account
2. **Creates Client**: Creates a new OAuth2/OIDC client with the specified configuration
3. **Configures Permissions**: Assigns the `Access-To-RelayServer` role to the client's service account
4. **Sets up Protocol Mappers**: Configures client IP, client ID, and client host mappers

The created client will have:
- Service accounts enabled
- Client credentials grant type
- The same configuration as TestTenant1 and TestTenant2
- Access to the RelayServer via the assigned role

## Using the New Tenant

After creating a tenant, you can use it with a connector by setting these environment variables:

```yaml
environment:
  RelayConnector__TenantName: YourTenantName
  RelayConnector__RelayServerBaseUri: http://relay-server-a:5000
```

Or in a docker-compose service:

```yaml
my-connector:
  image: relay-connector
  environment:
    RelayConnector__TenantName: TestTenant3
    RelayConnector__RelayServerBaseUri: http://relay-server-a:5000
    # ... other configuration
```

## Manual Tenant Creation via Keycloak Admin Console

If you prefer to create tenants manually:

1. Access Keycloak Admin Console at `http://localhost:5002`
2. Login with admin/admin
3. Select the `relayserver` realm
4. Go to Clients → Create client
5. Configure the client with:
   - Client type: OpenID Connect
   - Client ID: Your tenant name
   - Client authentication: ON
   - Service accounts roles: ON
   - Standard flow: OFF
   - Direct access grants: OFF
6. Save and configure the client secret
7. Go to the Service accounts roles tab
8. Assign the `Access-To-RelayServer` role from the `relayserver` client

## Keycloak Admin Client Details

The `keycloak-admin` client is configured with the following permissions:
- `manage-clients`: Manage all clients in the realm
- `create-client`: Create new clients
- `view-clients`: View client configurations
- `query-clients`: Query for clients
- `view-realm`: View realm configuration
- `manage-users`: Manage users (needed for service account configuration)
- `view-users`: View users
- `query-users`: Query for users

This client uses service account authentication with the client credentials grant type.

## Security Notes

1. The default password `<Strong!Passw0rd>` should only be used in demo/development environments
2. In production, use strong, unique passwords for each tenant
3. The `keycloak-admin` client has elevated privileges - protect its credentials carefully
4. Consider implementing additional security measures like:
   - IP restrictions
   - Short token lifetimes
   - Audit logging
   - Regular credential rotation

## Troubleshooting

### Script fails to get access token
- Check that Keycloak is running and accessible
- Verify the KEYCLOAK_URL is correct
- Ensure the keycloak-admin client exists and has the correct secret

### Client creation succeeds but role assignment fails
- The service account may take a moment to be created - the script waits 2 seconds
- Check that the `relayserver` client exists in the realm
- Verify the `Access-To-RelayServer` role exists

### Connector can't authenticate with new tenant
- Verify the tenant was created successfully in Keycloak
- Check that the service account has the `Access-To-RelayServer` role
- Ensure the client secret matches what's configured in the connector