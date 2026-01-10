# Update Lambda functions with new ECR images

$AWS_ACCOUNT_ID = "636017849911"
$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"

# Refresh PATH from system environment
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

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

Write-Host "Updating Lambda functions with new images..." -ForegroundColor Cyan
Write-Host ""

foreach ($service in $services) {
    $functionName = "$service-lambda"
    $imageUri = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$service-lambda:latest"
    
    Write-Host "Updating $functionName..." -ForegroundColor Yellow
    
    aws lambda update-function-code `
        --function-name $functionName `
        --image-uri $imageUri `
        --region $AWS_REGION `
        --profile $AWS_PROFILE `
        --output json | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Updated: $functionName" -ForegroundColor Green
    } else {
        Write-Host "Failed: $functionName" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Waiting for functions to become active..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All Lambda functions updated!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

