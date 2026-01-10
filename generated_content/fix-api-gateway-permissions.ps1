# Add API Gateway permissions to Lambda functions
$ErrorActionPreference = "Stop"

# Refresh PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$region = "us-east-2"
$profile = "deploy"
$apiId = "32avbpfsw6"

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
    Write-Host "Adding API Gateway permission to $service..." -ForegroundColor Cyan
    
    try {
        # Remove existing permission if it exists
        aws lambda remove-permission `
            --function-name $service `
            --statement-id "apigateway-$apiId" `
            --region $region `
            --profile $profile 2>$null
    }
    catch {
        # Ignore if doesn't exist
    }
    
    # Add new permission
    try {
        aws lambda add-permission `
            --function-name $service `
            --statement-id "apigateway-$apiId" `
            --action lambda:InvokeFunction `
            --principal apigateway.amazonaws.com `
            --source-arn "arn:aws:execute-api:${region}:636017849911:${apiId}/*" `
            --region $region `
            --profile $profile | Out-Null
        
        Write-Host "  Added permission to $service" -ForegroundColor Green
    }
    catch {
        Write-Host "  Failed to add permission to $service : $_" -ForegroundColor Red
    }
}

Write-Host "`nAll permissions configured!" -ForegroundColor Green
Write-Host "Test with: Invoke-WebRequest -Uri 'https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/users' -Method GET"
