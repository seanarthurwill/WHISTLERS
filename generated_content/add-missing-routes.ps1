# Add missing routes to API Gateway
$ErrorActionPreference = "Stop"

$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$apiId = "32avbpfsw6"
$region = "us-east-2"
$profile = "deploy"

# Get integration IDs
Write-Host "Getting Lambda integration IDs..." -ForegroundColor Cyan
$usersIntegration = (aws apigatewayv2 get-integrations --api-id $apiId --profile $profile --region $region --query "Items[?contains(IntegrationUri, 'users-lambda')].IntegrationId" --output text)
$gamesIntegration = (aws apigatewayv2 get-integrations --api-id $apiId --profile $profile --region $region --query "Items[?contains(IntegrationUri, 'games-lambda')].IntegrationId" --output text)

Write-Host "Users Integration: $usersIntegration" -ForegroundColor Yellow
Write-Host "Games Integration: $gamesIntegration" -ForegroundColor Yellow

# Routes to add
$routes = @(
    @{Path="GET /api/roles"; Integration=$usersIntegration; Description="Roles (Users service)"},
    @{Path="POST /api/roles"; Integration=$usersIntegration; Description="Roles (Users service)"},
    @{Path="GET /api/roles/{proxy+}"; Integration=$usersIntegration; Description="Roles with params"},
    @{Path="GET /api/sports"; Integration=$gamesIntegration; Description="Sports (Games service)"},
    @{Path="POST /api/sports"; Integration=$gamesIntegration; Description="Sports (Games service)"},
    @{Path="GET /api/sports/{proxy+}"; Integration=$gamesIntegration; Description="Sports with params"},
    @{Path="GET /api/leagues"; Integration=$gamesIntegration; Description="Leagues (Games service)"},
    @{Path="POST /api/leagues"; Integration=$gamesIntegration; Description="Leagues (Games service)"},
    @{Path="GET /api/leagues/{proxy+}"; Integration=$gamesIntegration; Description="Leagues with params"}
)

foreach ($route in $routes) {
    Write-Host "`nCreating route: $($route.Path)..." -ForegroundColor Cyan
    try {
        aws apigatewayv2 create-route `
            --api-id $apiId `
            --route-key $route.Path `
            --target "integrations/$($route.Integration)" `
            --region $region `
            --profile $profile `
            --no-cli-pager | Out-Null
        Write-Host "  Created: $($route.Description)" -ForegroundColor Green
    }
    catch {
        Write-Host "  Already exists or error: $_" -ForegroundColor Yellow
    }
}

Write-Host "`nDone! Testing endpoints..." -ForegroundColor Green
Start-Sleep -Seconds 3

@("roles", "sports", "leagues") | ForEach-Object {
    Write-Host "`nTesting /api/$_..." -ForegroundColor Yellow
    try {
        $response = Invoke-WebRequest -Uri "https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/$_" -Method GET -UseBasicParsing -TimeoutSec 5
        Write-Host "  Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "  Error: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
