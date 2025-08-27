#!/bin/bash

# Test script to demonstrate tenant creation
# This script should be run after the Docker containers are up and running

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}=========================================${NC}"
echo -e "${YELLOW}RelayServer Tenant Creation Test${NC}"
echo -e "${YELLOW}=========================================${NC}"
echo ""
echo "This script will demonstrate creating a new tenant in Keycloak."
echo ""
echo "Prerequisites:"
echo "1. Docker containers must be running (docker-compose up -d)"
echo "2. Wait for Keycloak to be fully initialized (about 30 seconds)"
echo ""
echo -e "${YELLOW}Press Enter to continue or Ctrl+C to cancel...${NC}"
read

# Check if Keycloak is accessible
echo -e "${YELLOW}Checking Keycloak availability...${NC}"
if curl -s -f -o /dev/null "http://localhost:5002/realms/relayserver/.well-known/openid-configuration"; then
    echo -e "${GREEN}✓ Keycloak is running and accessible${NC}"
else
    echo "Error: Keycloak is not accessible at http://localhost:5002"
    echo "Please ensure the containers are running: docker-compose up -d"
    exit 1
fi

echo ""
echo -e "${YELLOW}Creating test tenant: TestTenant3${NC}"
echo ""

# Create the test tenant
./create-tenant.sh TestTenant3 "Test Tenant 3" "Third test tenant for demonstration"

echo ""
echo -e "${GREEN}=========================================${NC}"
echo -e "${GREEN}Test completed successfully!${NC}"
echo -e "${GREEN}=========================================${NC}"
echo ""
echo "You can verify the tenant was created by:"
echo "1. Logging into Keycloak Admin Console at http://localhost:5002"
echo "   Username: admin"
echo "   Password: admin"
echo "2. Navigate to the 'relayserver' realm → Clients"
echo "3. You should see 'TestTenant3' in the list"
echo ""
echo "To create additional tenants, run:"
echo "  ./create-tenant.sh <tenant_name> <display_name> [description] [secret]"
echo ""