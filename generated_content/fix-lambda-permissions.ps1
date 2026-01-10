# Fix Lambda function URL permissions

$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"

# Refresh PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$services = @("users", "games", "organizations", "assignors", "communication", "reviews", "groups", "payscale")

foreach ($service in $services) {
    $functionName = "$service-lambda"
    Write-Host "Fixing permissions for $functionName..." -ForegroundColor Yellow
    
    # Remove old permission
    aws lambda remove-permission `
        --function-name $functionName `
        --statement-id FunctionURLAllowPublicAccess `
        --region $AWS_REGION `
        --profile $AWS_PROFILE 2>$null
    
    # Add new permission
    aws lambda add-permission `
        --function-name $functionName `
        --statement-id FunctionURLAllowPublicAccess `
        --action lambda:InvokeFunctionUrl `
        --principal "*" `
        --function-url-auth-type NONE `
        --region $AWS_REGION `
        --profile $AWS_PROFILE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Fixed: $functionName" -ForegroundColor Green
    } else {
        Write-Host "Failed: $functionName" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "All permissions updated!" -ForegroundColor Green
