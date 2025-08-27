#!/bin/bash

# Script to create a new tenant client in Keycloak for RelayServer
# Usage: ./create-tenant.sh <tenant_name> <display_name> [description] [client_secret]
# Example: ./create-tenant.sh TestTenant3 "Test Tenant 3" "Third test tenant" "<Strong!Passw0rd>"

set -e

# Enable debug mode if DEBUG=1
if [ "${DEBUG}" = "1" ]; then
    set -x
fi

# Configuration
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:5002}"
REALM="relayserver"
ADMIN_CLIENT_ID="keycloak-admin"
ADMIN_CLIENT_SECRET="${ADMIN_CLIENT_SECRET:-<Strong!Passw0rd>}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_message() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to get access token for the admin client
get_admin_token() {
    local token_response=$(curl -s -X POST \
        "${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token" \
        -H "Content-Type: application/x-www-form-urlencoded" \
        -d "grant_type=client_credentials" \
        -d "client_id=${ADMIN_CLIENT_ID}" \
        -d "client_secret=${ADMIN_CLIENT_SECRET}")
    
    # Extract access token using grep and sed (portable across systems)
    local access_token=$(echo "$token_response" | grep -o '"access_token":"[^"]*' | sed 's/"access_token":"//')
    
    if [ -z "$access_token" ]; then
        print_message "$RED" "Error: Failed to obtain access token"
        print_message "$RED" "Response: $token_response"
        exit 1
    fi
    
    # Debug: decode token if jq is available
    if [ "${DEBUG}" = "1" ] && command -v jq >/dev/null 2>&1; then
        echo "Token payload:" >&2
        echo "$access_token" | cut -d. -f2 | base64 -d 2>/dev/null | jq . >&2 || true
    fi
    
    echo "$access_token"
}

# Function to create a new client
create_client() {
    local tenant_name=$1
    local display_name=$2
    local description=${3:-"Tenant client for RelayServer"}
    local client_secret=${4:-"<Strong!Passw0rd>"}
    local token=$5
    
    # Generate a unique client ID
    local client_uuid=$(uuidgen 2>/dev/null || cat /proc/sys/kernel/random/uuid 2>/dev/null || echo "$(date +%s)-$tenant_name")
    
    # Prepare the client configuration JSON
    local client_json=$(cat <<EOF
{
  "id": "${client_uuid}",
  "clientId": "${tenant_name}",
  "name": "${display_name}",
  "description": "${description}",
  "rootUrl": "",
  "adminUrl": "",
  "baseUrl": "",
  "surrogateAuthRequired": false,
  "enabled": true,
  "alwaysDisplayInConsole": false,
  "clientAuthenticatorType": "client-secret",
  "secret": "${client_secret}",
  "redirectUris": ["/*"],
  "webOrigins": ["/*"],
  "notBefore": 0,
  "bearerOnly": false,
  "consentRequired": false,
  "standardFlowEnabled": false,
  "implicitFlowEnabled": false,
  "directAccessGrantsEnabled": false,
  "serviceAccountsEnabled": true,
  "publicClient": false,
  "frontchannelLogout": true,
  "protocol": "openid-connect",
  "attributes": {
    "oauth2.device.authorization.grant.enabled": "false",
    "backchannel.logout.revoke.offline.tokens": "false",
    "use.refresh.tokens": "true",
    "oidc.ciba.grant.enabled": "false",
    "client.use.lightweight.access.token.enabled": "false",
    "backchannel.logout.session.required": "true",
    "client_credentials.use_refresh_token": "false",
    "tls.client.certificate.bound.access.tokens": "false",
    "require.pushed.authorization.requests": "false",
    "acr.loa.map": "{}",
    "display.on.consent.screen": "false",
    "token.response.type.bearer.lower-case": "false"
  },
  "authenticationFlowBindingOverrides": {},
  "fullScopeAllowed": true,
  "nodeReRegistrationTimeout": -1,
  "protocolMappers": [
    {
      "name": "Client IP Address",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-usersessionmodel-note-mapper",
      "consentRequired": false,
      "config": {
        "user.session.note": "clientAddress",
        "introspection.token.claim": "true",
        "id.token.claim": "true",
        "access.token.claim": "true",
        "claim.name": "clientIpAddress",
        "jsonType.label": "String"
      }
    },
    {
      "name": "Client ID",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-usersessionmodel-note-mapper",
      "consentRequired": false,
      "config": {
        "user.session.note": "client_id",
        "introspection.token.claim": "true",
        "id.token.claim": "true",
        "access.token.claim": "true",
        "claim.name": "client_id",
        "jsonType.label": "String"
      }
    },
    {
      "name": "Client Host",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-usersessionmodel-note-mapper",
      "consentRequired": false,
      "config": {
        "user.session.note": "clientHost",
        "introspection.token.claim": "true",
        "id.token.claim": "true",
        "access.token.claim": "true",
        "claim.name": "clientHost",
        "jsonType.label": "String"
      }
    }
  ],
  "defaultClientScopes": ["web-origins", "acr", "connector"],
  "optionalClientScopes": ["address", "phone", "offline_access", "roles", "profile", "microprofile-jwt", "email"]
}
EOF
)
    
    # Create the client
    local create_response=$(curl -s -w "\n%{http_code}" -X POST \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/clients" \
        -H "Authorization: Bearer ${token}" \
        -H "Content-Type: application/json" \
        -d "${client_json}")
    
    local http_code=$(echo "$create_response" | tail -n1)
    local response_body=$(echo "$create_response" | sed '$d')
    
    if [ "$http_code" = "201" ] || [ "$http_code" = "204" ]; then
        print_message "$GREEN" "✓ Client '${tenant_name}' created successfully"
        return 0
    else
        print_message "$RED" "Error: Failed to create client (HTTP ${http_code})"
        if [ -n "$response_body" ]; then
            print_message "$RED" "Response: $response_body"
        fi
        return 1
    fi
}

# Function to get the service account user ID for a client
get_service_account_user() {
    local tenant_name=$1
    local token=$2
    
    # Get service account user
    local user_response=$(curl -s -X GET \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/users?username=service-account-${tenant_name}" \
        -H "Authorization: Bearer ${token}")
    
    # Extract user ID
    local user_id=$(echo "$user_response" | grep -o '"id":"[^"]*' | sed -n '1p' | sed 's/"id":"//')
    
    if [ -z "$user_id" ]; then
        print_message "$YELLOW" "Warning: Could not find service account user for ${tenant_name}"
        return 1
    fi
    
    echo "$user_id"
}

# Function to assign Access-To-RelayServer role to the service account
assign_relay_access_role() {
    local user_id=$1
    local token=$2
    
    # First, get the relayserver client ID
    local clients_response=$(curl -s -X GET \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=relayserver" \
        -H "Authorization: Bearer ${token}")
    
    local relayserver_client_id=$(echo "$clients_response" | grep -o '"id":"[^"]*' | sed -n '1p' | sed 's/"id":"//')
    
    if [ -z "$relayserver_client_id" ]; then
        print_message "$YELLOW" "Warning: Could not find relayserver client"
        return 1
    fi
    
    # Get the Access-To-RelayServer role
    local roles_response=$(curl -s -X GET \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/clients/${relayserver_client_id}/roles?search=Access-To-RelayServer" \
        -H "Authorization: Bearer ${token}")
    
    local role_id=$(echo "$roles_response" | grep -o '"id":"[^"]*' | sed -n '1p' | sed 's/"id":"//')
    local role_name=$(echo "$roles_response" | grep -o '"name":"[^"]*' | sed -n '1p' | sed 's/"name":"//')
    
    if [ -z "$role_id" ]; then
        print_message "$YELLOW" "Warning: Could not find Access-To-RelayServer role"
        return 1
    fi
    
    # Assign the role to the service account
    local role_json="[{\"id\":\"${role_id}\",\"name\":\"${role_name}\"}]"
    
    local assign_response=$(curl -s -w "\n%{http_code}" -X POST \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/users/${user_id}/role-mappings/clients/${relayserver_client_id}" \
        -H "Authorization: Bearer ${token}" \
        -H "Content-Type: application/json" \
        -d "${role_json}")
    
    local http_code=$(echo "$assign_response" | tail -n1)
    
    if [ "$http_code" = "204" ] || [ "$http_code" = "200" ]; then
        print_message "$GREEN" "✓ Access-To-RelayServer role assigned successfully"
        return 0
    else
        print_message "$YELLOW" "Warning: Could not assign Access-To-RelayServer role (HTTP ${http_code})"
        return 1
    fi
}

# Function to test API access
test_api_access() {
    local token=$1
    
    print_message "$YELLOW" "Testing API access permissions..."
    
    # Try to list clients to verify permissions
    local test_response=$(curl -s -w "\n%{http_code}" -X GET \
        "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?clientId=relayserver&max=1" \
        -H "Authorization: Bearer ${token}")
    
    local http_code=$(echo "$test_response" | tail -n1)
    
    if [ "$http_code" = "200" ]; then
        print_message "$GREEN" "✓ API access verified"
        return 0
    else
        print_message "$RED" "Error: Cannot access Keycloak Admin API (HTTP ${http_code})"
        local response_body=$(echo "$test_response" | sed '$d')
        if [ -n "$response_body" ]; then
            print_message "$RED" "Response: $response_body"
        fi
        print_message "$RED" "Please ensure the keycloak-admin client has realm-admin permissions"
        return 1
    fi
}

# Main script
main() {
    # Check arguments
    if [ $# -lt 2 ]; then
        print_message "$RED" "Error: Missing required arguments"
        echo "Usage: $0 <tenant_name> <display_name> [description] [client_secret]"
        echo "Example: $0 TestTenant3 \"Test Tenant 3\" \"Third test tenant\" \"<Strong!Passw0rd>\""
        exit 1
    fi
    
    local tenant_name=$1
    local display_name=$2
    local description=${3:-"Tenant client for RelayServer"}
    local client_secret=${4:-"<Strong!Passw0rd>"}
    
    print_message "$YELLOW" "Creating tenant: ${tenant_name}"
    print_message "$YELLOW" "Display name: ${display_name}"
    print_message "$YELLOW" "Description: ${description}"
    echo ""
    
    # Get admin token
    print_message "$YELLOW" "Obtaining admin access token..."
    local token=$(get_admin_token)
    print_message "$GREEN" "✓ Access token obtained"
    
    # Test API access
    if ! test_api_access "$token"; then
        exit 1
    fi
    
    # Create the client
    print_message "$YELLOW" "Creating client..."
    if create_client "$tenant_name" "$display_name" "$description" "$client_secret" "$token"; then
        
        # Wait a moment for the service account to be created
        sleep 2
        
        # Get the service account user ID
        print_message "$YELLOW" "Configuring service account..."
        local user_id=$(get_service_account_user "$tenant_name" "$token")
        
        if [ -n "$user_id" ]; then
            print_message "$GREEN" "✓ Service account found: ${user_id}"
            
            # Assign the Access-To-RelayServer role
            print_message "$YELLOW" "Assigning RelayServer access role..."
            assign_relay_access_role "$user_id" "$token"
        fi
        
        echo ""
        print_message "$GREEN" "========================================="
        print_message "$GREEN" "Tenant '${tenant_name}' created successfully!"
        print_message "$GREEN" "========================================="
        echo ""
        echo "Client ID: ${tenant_name}"
        echo "Client Secret: ${client_secret}"
        echo ""
        echo "You can now use this tenant to connect to the RelayServer."
        echo ""
        echo "To use with a connector, set these environment variables:"
        echo "  RelayConnector__TenantName=${tenant_name}"
        echo "  RelayConnector__RelayServerBaseUri=http://relay-server-a:5000  # or relay-server-b"
        echo ""
    else
        print_message "$RED" "Failed to create tenant"
        exit 1
    fi
}

# Run the main function
main "$@"