#!/bin/bash

# Diagnostic script to check keycloak-admin client configuration
# Usage: ./check-keycloak-admin.sh

set -e

# Configuration
KEYCLOAK_URL="${KEYCLOAK_URL:-http://localhost:5002}"
REALM="relayserver"
ADMIN_CLIENT_ID="keycloak-admin"
ADMIN_CLIENT_SECRET="${ADMIN_CLIENT_SECRET:-<Strong!Passw0rd>}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored messages
print_message() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

print_message "$BLUE" "========================================="
print_message "$BLUE" "Keycloak Admin Client Diagnostic"
print_message "$BLUE" "========================================="
echo ""

# Check Keycloak availability
print_message "$YELLOW" "1. Checking Keycloak availability..."
if curl -s -f -o /dev/null "${KEYCLOAK_URL}/realms/${REALM}/.well-known/openid-configuration"; then
    print_message "$GREEN" "   ✓ Keycloak is accessible at ${KEYCLOAK_URL}"
else
    print_message "$RED" "   ✗ Cannot reach Keycloak at ${KEYCLOAK_URL}"
    print_message "$RED" "   Please ensure Docker containers are running"
    exit 1
fi

# Try to get token
print_message "$YELLOW" "2. Testing authentication with keycloak-admin client..."
token_response=$(curl -s -X POST \
    "${KEYCLOAK_URL}/realms/${REALM}/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "grant_type=client_credentials" \
    -d "client_id=${ADMIN_CLIENT_ID}" \
    -d "client_secret=${ADMIN_CLIENT_SECRET}")

access_token=$(echo "$token_response" | grep -o '"access_token":"[^"]*' | sed 's/"access_token":"//')

if [ -n "$access_token" ]; then
    print_message "$GREEN" "   ✓ Successfully authenticated"
    
    # Decode token to check roles (if jq is available)
    if command -v jq >/dev/null 2>&1 && command -v base64 >/dev/null 2>&1; then
        print_message "$YELLOW" "3. Checking token permissions..."
        token_payload=$(echo "$access_token" | cut -d. -f2 | base64 -d 2>/dev/null | jq . 2>/dev/null || echo "{}")
        
        # Check for realm-management roles
        realm_roles=$(echo "$token_payload" | jq -r '.resource_access."realm-management".roles[]?' 2>/dev/null)
        
        if [ -n "$realm_roles" ]; then
            print_message "$GREEN" "   ✓ Found realm-management roles:"
            echo "$realm_roles" | while read -r role; do
                echo "     - $role"
            done
        else
            print_message "$YELLOW" "   ⚠ No realm-management roles found in token"
        fi
    fi
else
    print_message "$RED" "   ✗ Failed to authenticate"
    error_msg=$(echo "$token_response" | grep -o '"error_description":"[^"]*' | sed 's/"error_description":"//')
    if [ -n "$error_msg" ]; then
        print_message "$RED" "   Error: $error_msg"
    else
        print_message "$RED" "   Response: $token_response"
    fi
    echo ""
    print_message "$YELLOW" "   Possible causes:"
    echo "   - The keycloak-admin client may not exist"
    echo "   - The client secret may be incorrect"
    echo "   - Keycloak may still be initializing"
    exit 1
fi

# Test API access
print_message "$YELLOW" "4. Testing Admin API access..."

# Try to list clients
list_response=$(curl -s -w "\n%{http_code}" -X GET \
    "${KEYCLOAK_URL}/admin/realms/${REALM}/clients?max=1" \
    -H "Authorization: Bearer ${access_token}")

http_code=$(echo "$list_response" | tail -n1)

if [ "$http_code" = "200" ]; then
    print_message "$GREEN" "   ✓ Can list clients (HTTP 200)"
else
    print_message "$RED" "   ✗ Cannot list clients (HTTP ${http_code})"
    response_body=$(echo "$list_response" | sed '$d')
    if [ -n "$response_body" ]; then
        print_message "$RED" "   Response: $response_body"
    fi
fi

# Try to check create permission
print_message "$YELLOW" "5. Checking create client permission..."
# We'll do a dry-run by sending an invalid request that should fail with 400 (bad request) not 403 (forbidden)
create_test=$(curl -s -w "\n%{http_code}" -X POST \
    "${KEYCLOAK_URL}/admin/realms/${REALM}/clients" \
    -H "Authorization: Bearer ${access_token}" \
    -H "Content-Type: application/json" \
    -d '{}')

http_code=$(echo "$create_test" | tail -n1)

if [ "$http_code" = "400" ]; then
    print_message "$GREEN" "   ✓ Has permission to create clients (got expected 400 for invalid request)"
elif [ "$http_code" = "403" ]; then
    print_message "$RED" "   ✗ No permission to create clients (HTTP 403 Forbidden)"
    print_message "$RED" "   The keycloak-admin client needs realm-admin permissions"
else
    print_message "$YELLOW" "   ⚠ Unexpected response code: HTTP ${http_code}"
fi

echo ""
print_message "$BLUE" "========================================="
print_message "$BLUE" "Diagnostic Complete"
print_message "$BLUE" "========================================="

if [ "$http_code" = "400" ] || [ "$http_code" = "201" ]; then
    echo ""
    print_message "$GREEN" "✓ The keycloak-admin client appears to be configured correctly!"
    echo ""
    echo "You can now create tenants using:"
    echo "  ./create-tenant.sh <tenant_name> <display_name> [description] [secret]"
else
    echo ""
    print_message "$YELLOW" "⚠ There may be permission issues with the keycloak-admin client."
    echo ""
    echo "To fix this:"
    echo "1. Stop the Docker containers: docker-compose down"
    echo "2. Start them again: docker-compose up -d"
    echo "3. Wait about 30 seconds for Keycloak to initialize"
    echo "4. Run this diagnostic again"
    echo ""
    echo "If the problem persists, check the realm configuration in:"
    echo "  keycloak_data/relayserver-realm.json"
fi