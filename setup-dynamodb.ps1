# Setup DynamoDB table and permissions for Users service

# Refresh PATH from system environment
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"
$TABLE_NAME = "UserSessions"
$DEPLOY_USER = "Deploy"
$HEADOFFICIAL_USER = "headofficial"
$POLICY_FILE = "c:\dev\dynamodb-policy.json"

# Step 1: Add DynamoDB permissions to Deploy user (to create table)
Write-Host "Step 1: Adding DynamoDB permissions to $DEPLOY_USER user..." -ForegroundColor Cyan

aws iam put-user-policy --user-name $DEPLOY_USER --policy-name "DynamoDBUserSessionsAccess" --policy-document file://$POLICY_FILE --profile $AWS_PROFILE

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deploy user permissions added successfully" -ForegroundColor Green
} else {
    Write-Host "Failed to add permissions to Deploy user" -ForegroundColor Red
    exit 1
}

# Step 2: Add DynamoDB permissions to headofficial user (runtime access)
Write-Host ""
Write-Host "Step 2: Adding DynamoDB permissions to $HEADOFFICIAL_USER user..." -ForegroundColor Cyan

aws iam put-user-policy --user-name $HEADOFFICIAL_USER --policy-name "DynamoDBUserSessionsAccess" --policy-document file://$POLICY_FILE --profile $AWS_PROFILE

if ($LASTEXITCODE -eq 0) {
    Write-Host "headofficial user permissions added successfully" -ForegroundColor Green
} else {
    Write-Host "Failed to add permissions to headofficial user" -ForegroundColor Red
    exit 1
}

# Step 3: Create DynamoDB table
Write-Host ""
Write-Host "Step 3: Creating DynamoDB table: $TABLE_NAME..." -ForegroundColor Cyan

aws dynamodb create-table --table-name $TABLE_NAME --attribute-definitions AttributeName=TokenHash,AttributeType=S --key-schema AttributeName=TokenHash,KeyType=HASH --billing-mode PAY_PER_REQUEST --region $AWS_REGION --profile $AWS_PROFILE --tags Key=Service,Value=Users Key=Purpose,Value=SessionManagement

if ($LASTEXITCODE -eq 0) {
    Write-Host "Table creation initiated" -ForegroundColor Green
    Write-Host "Waiting for table to become active..." -ForegroundColor Yellow
    
    aws dynamodb wait table-exists --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE
    
    Write-Host "Table is now active" -ForegroundColor Green
    
    # Step 4: Enable TTL
    Write-Host ""
    Write-Host "Step 4: Enabling TTL for automatic session cleanup..." -ForegroundColor Cyan
    
    aws dynamodb update-time-to-live --table-name $TABLE_NAME --time-to-live-specification "Enabled=true, AttributeName=TTL" --region $AWS_REGION --profile $AWS_PROFILE
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "TTL enabled successfully" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Setup Complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Table Details:" -ForegroundColor Cyan
    aws dynamodb describe-table --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE --query "Table.{Name:TableName,Status:TableStatus,CreationDateTime:CreationDateTime}" --output table
} else {
    # Table might already exist
    Write-Host "Failed to create table (may already exist)" -ForegroundColor Yellow
    Write-Host "Checking existing table..." -ForegroundColor Yellow
    aws dynamodb describe-table --table-name $TABLE_NAME --region $AWS_REGION --profile $AWS_PROFILE --query "Table.{Name:TableName,Status:TableStatus}" --output table
}

Write-Host ""
Write-Host "DynamoDB setup complete! You can now redeploy your Users service." -ForegroundColor Green
