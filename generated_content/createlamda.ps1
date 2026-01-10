# Create IAM role for Lambda execution

$AWS_ACCOUNT_ID = "636017849911"
$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"
$ROLE_NAME = "lambda-execution-role"

# Refresh PATH from system environment (in case AWS was just installed)
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

Write-Host "Creating IAM role for Lambda functions..." -ForegroundColor Cyan

# Create trust policy document
$trustPolicy = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
"@

# Save trust policy to temp file with UTF8 no BOM encoding
[System.IO.File]::WriteAllText("$PWD\trust-policy.json", $trustPolicy)

# Create the IAM role
Write-Host "Creating role: $ROLE_NAME" -ForegroundColor Yellow
$createRoleOutput = aws iam create-role `
    --role-name $ROLE_NAME `
    --assume-role-policy-document file://trust-policy.json `
    --profile $AWS_PROFILE 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "Role created successfully" -ForegroundColor Green
} elseif ($createRoleOutput -match "EntityAlreadyExists") {
    Write-Host "Role already exists, continuing..." -ForegroundColor Yellow
} else {
    Write-Host "ERROR: Failed to create role" -ForegroundColor Red
    Write-Host $createRoleOutput -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure your Deploy IAM user has these permissions:" -ForegroundColor Yellow
    Write-Host "  - iam:CreateRole" -ForegroundColor Yellow
    Write-Host "  - iam:AttachRolePolicy" -ForegroundColor Yellow
    Write-Host "  - iam:GetRole" -ForegroundColor Yellow
    Write-Host "  - iam:PassRole" -ForegroundColor Yellow
    Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue
    exit 1
}

# Verify role exists before attaching policies
Write-Host "Verifying role exists..." -ForegroundColor Yellow
$roleCheck = aws iam get-role --role-name $ROLE_NAME --profile $AWS_PROFILE 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Role does not exist. Cannot continue." -ForegroundColor Red
    Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue
    exit 1
}
Write-Host "Role verified" -ForegroundColor Green

# Attach basic Lambda execution policy
Write-Host "Attaching AWSLambdaBasicExecutionRole policy..." -ForegroundColor Yellow
aws iam attach-role-policy `
    --role-name $ROLE_NAME `
    --policy-arn "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole" `
    --profile $AWS_PROFILE

# Attach VPC execution policy (if your Lambda needs VPC access)
Write-Host "Attaching AWSLambdaVPCAccessExecutionRole policy..." -ForegroundColor Yellow
aws iam attach-role-policy `
    --role-name $ROLE_NAME `
    --policy-arn "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole" `
    --profile $AWS_PROFILE

# Clean up temp file
Remove-Item "trust-policy.json" -ErrorAction SilentlyContinue

Write-Host "Waiting 10 seconds for role to propagate..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "========================================" -ForegroundColor Green
Write-Host "IAM role created: $ROLE_NAME" -ForegroundColor Green
Write-Host "Role ARN: arn:aws:iam::${AWS_ACCOUNT_ID}:role/$ROLE_NAME" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
# Create Lambda functions from ECR images

$AWS_ACCOUNT_ID = "636017849911"
$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"
$ROLE_ARN = "arn:aws:iam::${AWS_ACCOUNT_ID}:role/lambda-execution-role"

# Array of services
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

Write-Host "Creating Lambda functions..." -ForegroundColor Cyan

foreach ($service in $services) {
    $functionName = "$service-lambda"
    $imageUri = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$service-lambda:latest"
    
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host "Creating: $functionName" -ForegroundColor Magenta
    Write-Host "========================================" -ForegroundColor Magenta
    
    # Create Lambda function
    aws lambda create-function `
        --function-name $functionName `
        --package-type Image `
        --code ImageUri=$imageUri `
        --role $ROLE_ARN `
        --timeout 30 `
        --memory-size 512 `
        --region $AWS_REGION `
        --profile $AWS_PROFILE `
        --environment "Variables={ConnectionStrings__DefaultConnection=Host=whistl3r-1-instance-1.cno80gy6gzh5.us-east-2.rds.amazonaws.com;Database=postgres;Username=headofficial;Password=0pt1m0sPr1m3.;Search Path=public,ASPNETCORE_ENVIRONMENT=Production}"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Lambda function created: $functionName" -ForegroundColor Green
        
        # Create function URL (so you can access it via HTTP)
        Write-Host "Creating function URL for $functionName..." -ForegroundColor Yellow
        aws lambda create-function-url-config `
            --function-name $functionName `
            --auth-type NONE `
            --region $AWS_REGION `
            --profile $AWS_PROFILE
        
        # Add permission for public access
        aws lambda add-permission `
            --function-name $functionName `
            --statement-id FunctionURLAllowPublicAccess `
            --action lambda:InvokeFunctionUrl `
            --principal "*" `
            --function-url-auth-type NONE `
            --region $AWS_REGION `
            --profile $AWS_PROFILE
        
        # Get the function URL
        $functionUrl = aws lambda get-function-url-config `
            --function-name $functionName `
            --region $AWS_REGION `
            --profile $AWS_PROFILE `
            --query 'FunctionUrl' `
            --output text
        
        Write-Host "Function URL: $functionUrl" -ForegroundColor Cyan
    } else {
        Write-Host "Failed to create Lambda function: $functionName" -ForegroundColor Red
    }
    
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "All Lambda functions created!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green