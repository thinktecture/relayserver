# Helper script that seeds tenants

function Create-Tenant {
    $resultCode
    $result = Invoke-RestMethod -Method Post -Uri https://localhost:5005/api/tenant -Body (@{ "name" = $tenantName } | ConvertTo-Json) -Headers @{ 'Content-Type' = 'application/json' } -SkipHttpErrorCheck -StatusCodeVariable "resultCode"

    if ($resultCode -eq 201) {
        Write-Output "Tenant $tenantName was created $result"
    } elseif ($resultCode -eq 409) {
        Write-Output "Tenant $tenantName already exists $result"
    } else {
        Write-Output "Unexpected result while creating tenant: StatusCode: $resultCode Content: $result"
    }    
}

function Create-Secret {
    $resultCode
    $result = Invoke-RestMethod -Method Get -Uri https://localhost:5005/api/tenant/$tenantName -contenttype 'application/json'

    $result = Invoke-RestMethod -Method Post -Uri "https://localhost:5005/api/tenant/$($result.id)/secret" -Form @{ secret = "$tenantSecret" } -contenttype 'application/json'
    Write-Host "Added secret $($result.secret) to tenant $tenantName"
}

$tenantSecret = "<Strong!Passw0rd>";

$tenantName = "TestTenant1";
Create-Tenant
Create-Secret

$tenantName = "TestTenant2";
Create-Tenant
Create-Secret
