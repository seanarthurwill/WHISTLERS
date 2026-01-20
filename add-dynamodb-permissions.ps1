# Add DynamoDB permissions to headofficial IAM user

$AWS_PROFILE = "deploy"
$IAM_USER = "headofficial"

Write-Host "Adding DynamoDB permissions to $IAM_USER..." -ForegroundColor Yellow

# Create inline policy for DynamoDB access
$policyDocument = @"
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "dynamodb:DescribeTable",
                "dynamodb:GetItem",
                "dynamodb:PutItem",
                "dynamodb:UpdateItem",
                "dynamodb:DeleteItem",
                "dynamodb:Query",
                "dynamodb:Scan",
                "dynamodb:BatchGetItem",
                "dynamodb:BatchWriteItem"
            ],
            "Resource": "arn:aws:dynamodb:us-east-2:636017849911:table/UserSessions"
        }
    ]
}
"@

# Add the policy to the user
aws iam put-user-policy `
    --user-name $IAM_USER `
    --policy-name "DynamoDBUserSessionsAccess" `
    --policy-document $policyDocument `
    --profile $AWS_PROFILE

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ DynamoDB permissions added successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Permissions granted for:" -ForegroundColor Cyan
    Write-Host "  - DescribeTable" -ForegroundColor White
    Write-Host "  - GetItem, PutItem, UpdateItem, DeleteItem" -ForegroundColor White
    Write-Host "  - Query, Scan" -ForegroundColor White
    Write-Host "  - BatchGetItem, BatchWriteItem" -ForegroundColor White
    Write-Host ""
    Write-Host "Verifying permissions..." -ForegroundColor Yellow
    
    # List all inline policies for the user
    aws iam list-user-policies `
        --user-name $IAM_USER `
        --profile $AWS_PROFILE `
        --output table
} else {
    Write-Host "✗ Failed to add DynamoDB permissions" -ForegroundColor Red
}
