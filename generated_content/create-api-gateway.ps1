# Create API Gateway HTTP API for Lambda functions

$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"
$API_NAME = "whistl3r-api"

# Refresh PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "Creating API Gateway HTTP API..." -ForegroundColor Cyan

# Create HTTP API with CORS
$apiOutput = aws apigatewayv2 create-api `
    --name $API_NAME `
    --protocol-type HTTP `
    --cors-configuration "AllowOrigins=*,AllowMethods=*,AllowHeaders=*" `
    --region $AWS_REGION `
    --profile $AWS_PROFILE `
    --output json | ConvertFrom-Json

$apiId = $apiOutput.ApiId
$apiEndpoint = $apiOutput.ApiEndpoint

Write-Host "Created API: $apiId" -ForegroundColor Green
Write-Host "Endpoint: $apiEndpoint" -ForegroundColor White
Write-Host ""

# Services to integrate
$services = @(
    @{name="users"; path="/api/users"},
    @{name="games"; path="/api/games"},
    @{name="organizations"; path="/api/organizations"},
    @{name="assignors"; path="/api/assignors"},
    @{name="communication"; path="/api/communication"},
    @{name="reviews"; path="/api/reviews"},
    @{name="groups"; path="/api/groups"},
    @{name="payscale"; path="/api/payscale"}
)

foreach ($service in $services) {
    $functionName = "$($service.name)-lambda"
    $basePath = $service.path
    
    Write-Host "Configuring $functionName..." -ForegroundColor Yellow
    
    # Get Lambda ARN
    $lambdaArn = aws lambda get-function `
        --function-name $functionName `
        --region $AWS_REGION `
        --profile $AWS_PROFILE `
        --query 'Configuration.FunctionArn' `
        --output text
    
    # Create integration
    $integrationOutput = aws apigatewayv2 create-integration `
        --api-id $apiId `
        --integration-type AWS_PROXY `
        --integration-uri $lambdaArn `
        --payload-format-version 2.0 `
        --region $AWS_REGION `
        --profile $AWS_PROFILE `
        --output json | ConvertFrom-Json
    
    $integrationId = $integrationOutput.IntegrationId
    
    # Create routes for common HTTP methods
    $routes = @("GET", "POST", "PUT", "DELETE", "PATCH")
    
    foreach ($method in $routes) {
        # Route for exact path
        aws apigatewayv2 create-route `
            --api-id $apiId `
            --route-key "$method $basePath" `
            --target "integrations/$integrationId" `
            --region $AWS_REGION `
            --profile $AWS_PROFILE | Out-Null
        
        # Route for path with parameters
        aws apigatewayv2 create-route `
            --api-id $apiId `
            --route-key "$method $basePath/{proxy+}" `
            --target "integrations/$integrationId" `
            --region $AWS_REGION `
            --profile $AWS_PROFILE | Out-Null
    }
    
    # Add Lambda permission for API Gateway
    aws lambda add-permission `
        --function-name $functionName `
        --statement-id "apigateway-$apiId" `
        --action lambda:InvokeFunction `
        --principal apigateway.amazonaws.com `
        --source-arn "arn:aws:execute-api:${AWS_REGION}:*:${apiId}/*" `
        --region $AWS_REGION `
        --profile $AWS_PROFILE 2>$null | Out-Null
    
    Write-Host "Configured: $functionName" -ForegroundColor Green
}

# Create default stage
aws apigatewayv2 create-stage `
    --api-id $apiId `
    --stage-name '$default' `
    --auto-deploy `
    --region $AWS_REGION `
    --profile $AWS_PROFILE | Out-Null

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "API Gateway Deployed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "API Endpoint: $apiEndpoint" -ForegroundColor Cyan
Write-Host ""
Write-Host "Test your APIs:" -ForegroundColor Yellow
Write-Host "  Users:    $apiEndpoint/api/users" -ForegroundColor White
Write-Host "  Games:    $apiEndpoint/api/games" -ForegroundColor White
Write-Host "  Org:      $apiEndpoint/api/organizations" -ForegroundColor White
Write-Host ""
Write-Host "Example:" -ForegroundColor Yellow
Write-Host "  Invoke-WebRequest -Uri '$apiEndpoint/api/users' -Method GET" -ForegroundColor Gray
Write-Host ""

# Save endpoint to file
$apiEndpoint | Out-File -FilePath "api-endpoint.txt" -Encoding utf8
Write-Host "API endpoint saved to api-endpoint.txt" -ForegroundColor Green
