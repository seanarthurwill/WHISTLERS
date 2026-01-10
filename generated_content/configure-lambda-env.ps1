# Configure Lambda Environment Variables
$ErrorActionPreference = "Stop"

# Refresh PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$region = "us-east-2"
$profile = "deploy"

# Database connection string (from appsettings.json)
$dbHost = "whistl3r-1-instance-1.cno80gy6gzh5.us-east-2.rds.amazonaws.com"
$dbPort = "5432"
$dbName = "postgres"
$dbUser = "headofficial"
$dbPassword = "0pt1m0sPr1m3."
$connectionString = "Host=$dbHost;Port=$dbPort;Database=$dbName;Username=$dbUser;Password=$dbPassword;SSL Mode=Require;Search Path=public"

# JWT configuration
$jwtSecret = "your-super-secret-key-that-is-at-least-32-characters-long-change-in-production"
$jwtIssuer = "Whistl3r"
$jwtAudience = "Whistl3rAPI"
$jwtExpirationMinutes = "60"

# API Gateway base URL for inter-service communication
$apiBaseUrl = "https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api"

$services = @(
    "users-lambda",
    "games-lambda",
    "organizations-lambda",
    "assignors-lambda",
    "communication-lambda",
    "reviews-lambda",
    "groups-lambda",
    "payscale-lambda"
)

foreach ($service in $services) {
    Write-Host "Configuring $service..." -ForegroundColor Cyan
    
    # Build environment variables JSON
    $envVars = @{
        ConnectionStrings__DefaultConnection = $connectionString
        Jwt__SecretKey = $jwtSecret
        Jwt__Issuer = $jwtIssuer
        Jwt__Audience = $jwtAudience
        Jwt__ExpirationMinutes = $jwtExpirationMinutes
        ASPNETCORE_ENVIRONMENT = "Production"
    }
    
    # Add service-specific URLs
    switch -Wildcard ($service) {
        "users-*" { 
            $envVars["Services__Communication"] = "$apiBaseUrl/communication"
        }
        "games-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
            $envVars["Services__Organizations"] = "$apiBaseUrl/organizations"
        }
        "organizations-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
        }
        "assignors-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
            $envVars["Services__Games"] = "$apiBaseUrl/games"
        }
        "communication-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
        }
        "reviews-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
            $envVars["Services__Games"] = "$apiBaseUrl/games"
        }
        "groups-*" { 
            $envVars["Services__Users"] = "$apiBaseUrl/users"
        }
        "payscale-*" { 
            $envVars["Services__Organizations"] = "$apiBaseUrl/organizations"
        }
    }
    
    # Save environment variables to temp file with correct AWS format
    $tempFile = "lambda-env-$service.json"
    $awsEnvFormat = @{ Variables = $envVars }
    $awsEnvFormat | ConvertTo-Json | Set-Content $tempFile
    
    try {
        aws lambda update-function-configuration `
            --function-name $service `
            --region $region `
            --profile $profile `
            --environment "file://$tempFile" `
            --no-cli-pager | Out-Null
        
        Remove-Item $tempFile -ErrorAction SilentlyContinue
        Write-Host "  Configured $service" -ForegroundColor Green
    }
    catch {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
        Write-Host "  Failed to configure $service : $_" -ForegroundColor Red
    }
}

Write-Host "`nWaiting 10 seconds for configurations to propagate..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "`nAll Lambda functions configured!" -ForegroundColor Green
Write-Host "Test with: Invoke-WebRequest -Uri 'https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/users' -Method GET"
