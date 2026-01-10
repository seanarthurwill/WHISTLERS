# Add auth routes to API Gateway
$ErrorActionPreference = "Stop"

$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$apiId = "32avbpfsw6"
$region = "us-east-2"
$profile = "deploy"

# Get users integration ID
Write-Host "Getting Users Lambda integration ID..." -ForegroundColor Cyan
$usersIntegration = (aws apigatewayv2 get-integrations --api-id $apiId --profile $profile --region $region --query "Items[?contains(IntegrationUri, 'users-lambda')].IntegrationId" --output text)

Write-Host "Users Integration: $usersIntegration" -ForegroundColor Yellow

# Auth routes to add
$routes = @(
    "GET /api/auth/encryption-key",
    "POST /api/auth/login",
    "POST /api/auth/register",
    "POST /api/auth/refresh",
    "GET /api/auth/{proxy+}",
    "POST /api/auth/{proxy+}"
)

foreach ($route in $routes) {
    Write-Host "`nCreating route: $route..." -ForegroundColor Cyan
    $result = aws apigatewayv2 create-route --api-id $apiId --route-key $route --target "integrations/$usersIntegration" --region $region --profile $profile --no-cli-pager 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Created successfully" -ForegroundColor Green
    } else {
        Write-Host "  Already exists or skipped" -ForegroundColor Yellow
    }
}

Write-Host "`nDone! Testing encryption-key endpoint..." -ForegroundColor Green
Start-Sleep -Seconds 3

$response = Invoke-WebRequest -Uri "https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/auth/encryption-key" -Method GET -UseBasicParsing -ErrorAction SilentlyContinue
if ($response -and $response.StatusCode -eq 200) {
    Write-Host "Status: $($response.StatusCode) - Success!" -ForegroundColor Green
} else {
    Write-Host "Failed to get response" -ForegroundColor Red
}
