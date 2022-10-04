# Helper script that seeds tenants

Function Create-Tenant {
    Param($id, $name, $secret)
    $resultCode
    $result = Invoke-RestMethod -Method Post -Uri http://localhost:5004/api/management/tenants -Body (@{ id = $id; name = $name; credentials = @(@{ plainTextValue = $secret }) } | ConvertTo-Json) -Headers @{ 'Content-Type' = 'application/json'; 'tt-apikey' = 'write-key' } -SkipHttpErrorCheck -StatusCodeVariable "resultCode"

    if ($resultCode -eq 201) {
        Write-Output "Tenant $tenantName was created $result"
    } elseif ($resultCode -eq 409) {
        Write-Output "Tenant $tenantName already exists $result"
    } else {
        Write-Output "Unexpected result while creating tenant: StatusCode: $resultCode Content: $result"
    }
}

$tenantSecret = "<Strong!Passw0rd>";

Create-Tenant -id "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" -name "TestTenant1" -secret $tenantSecret
Create-Tenant -id "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" -name "TestTenant2" -secret $tenantSecret
