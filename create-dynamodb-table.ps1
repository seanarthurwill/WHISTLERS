# Create UserSessions DynamoDB table

# Refresh PATH from system environment
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"
$TABLE_NAME = "UserSessions"

Write-Host "Creating DynamoDB table: $TABLE_NAME..." -ForegroundColor Yellow

# Create the table
aws dynamodb create-table --table-name $TABLE_NAME --attribute-definitions AttributeName=TokenHash,AttributeType=S --key-schema AttributeName=TokenHash,KeyType=HASH --billing-mode PAY_PER_REQUEST --region $AWS_REGION --profile $AWS_PROFILE --tags Key=Service,Value=Users Key=Purpose,Value=SessionManagement

if ($LASTEXITCODE -eq 0) {
    Write-Host "Table creation initiated" -ForegroundColor Green
    Write-Host "Waiting for table to become active..." -ForegroundColor Yellow
    
    aws dynamodb wait table-exists --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE
    
    Write-Host "Table is now active" -ForegroundColor Green
    Write-Host "Enabling TTL for automatic session cleanup..." -ForegroundColor Yellow
    
    aws dynamodb update-time-to-live --table-name $TABLE_NAME --time-to-live-specification "Enabled=true, AttributeName=TTL" --region $AWS_REGION --profile $AWS_PROFILE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "TTL enabled successfully" -ForegroundColor Green
    }
    
    Write-Host "Table Details:" -ForegroundColor Cyan
    aws dynamodb describe-table --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE
} else {
    Write-Host "Failed to create table" -ForegroundColor Red
    Write-Host "The table may already exist. Checking..." -ForegroundColor Yellow
    aws dynamodb describe-table --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE
}
