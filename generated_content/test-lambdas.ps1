# Test Lambda Functions

$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"

$services = @(
    "users",
    "games",
    "organizations",
    "assignors",
    "communication",
    "reviews",
    "groups",
    "payscale"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Lambda Function URLs" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

foreach ($service in $services) {
    $functionName = "$service-lambda"
    
    # Get function URL
    $functionUrl = aws lambda get-function-url-config `
        --function-name $functionName `
        --region $AWS_REGION `
        --profile $AWS_PROFILE `
        --query 'FunctionUrl' `
        --output text 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "$functionName - Creating function URL..." -ForegroundColor Yellow
        
        # Create function URL
        aws lambda create-function-url-config `
            --function-name $functionName `
            --auth-type NONE `
            --region $AWS_REGION `
            --profile $AWS_PROFILE | Out-Null
        
        # Add permission for public access
        aws lambda add-permission `
            --function-name $functionName `
            --statement-id FunctionURLAllowPublicAccess `
            --action lambda:InvokeFunctionUrl `
            --principal "*" `
            --function-url-auth-type NONE `
            --region $AWS_REGION `
            --profile $AWS_PROFILE 2>$null | Out-Null
        
        # Get the newly created URL
        $functionUrl = aws lambda get-function-url-config `
            --function-name $functionName `
            --region $AWS_REGION `
            --profile $AWS_PROFILE `
            --query 'FunctionUrl' `
            --output text
    }
    
    Write-Host "$functionName`: " -NoNewline -ForegroundColor Green
    Write-Host "$functionUrl" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing Functions" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Test a simple endpoint (health check)
$testService = "users"
$functionName = "$testService-lambda"
$functionUrl = aws lambda get-function-url-config `
    --function-name $functionName `
    --region $AWS_REGION `
    --profile $AWS_PROFILE `
    --query 'FunctionUrl' `
    --output text

Write-Host "Testing $functionName..." -ForegroundColor Yellow
Write-Host "URL: $functionUrl" -ForegroundColor Gray

try {
    $response = Invoke-WebRequest -Uri $functionUrl -Method GET -TimeoutSec 30
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Gray
    Write-Host $response.Content
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Testing Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "You can test specific endpoints like:" -ForegroundColor Yellow
Write-Host "  Invoke-WebRequest -Uri '<function-url>/api/users' -Method GET" -ForegroundColor Gray
Write-Host "  Invoke-WebRequest -Uri '<function-url>/api/games' -Method GET" -ForegroundColor Gray
