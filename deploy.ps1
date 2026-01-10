# Deploy all Lambda services to AWS ECR

# Configuration
$AWS_ACCOUNT_ID = "636017849911"
$AWS_REGION = "us-east-2"
$AWS_PROFILE = "deploy"

# Refresh PATH from system environment (in case AWS was just installed)
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Check if AWS CLI is installed
Write-Host "Checking AWS CLI installation..." -ForegroundColor Cyan
$awsCommand = Get-Command aws -ErrorAction SilentlyContinue
if (-not $awsCommand) {
    Write-Host "ERROR: AWS CLI is not installed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install AWS CLI:" -ForegroundColor Yellow
    Write-Host "1. Download from: https://aws.amazon.com/cli/" -ForegroundColor Yellow
    Write-Host "2. Or use: winget install Amazon.AWSCLI" -ForegroundColor Yellow
    Write-Host "3. After installation, restart PowerShell and run 'aws configure'" -ForegroundColor Yellow
    exit 1
}
$awsVersion = & aws --version 2>&1
Write-Host "AWS CLI found: $awsVersion" -ForegroundColor Green

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Cyan
try {
    docker version | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Docker is not running!" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    exit 1
}

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

# Create ECR repositories (one-time setup)
Write-Host "Creating ECR repositories..." -ForegroundColor Cyan
foreach ($service in $services) {
    Write-Host "Creating repository: $service-lambda" -ForegroundColor Yellow
    aws ecr create-repository --repository-name "$service-lambda" --region $AWS_REGION --profile $AWS_PROFILE 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Repository created: $service-lambda" -ForegroundColor Green
    } else {
        Write-Host "Repository already exists: $service-lambda" -ForegroundColor Gray
    }
}

Write-Host "Logging in to ECR..." -ForegroundColor Cyan
aws ecr get-login-password --region $AWS_REGION --profile $AWS_PROFILE | docker login --username AWS --password-stdin "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

# Build, tag, and push each service
Write-Host "Building and pushing services..." -ForegroundColor Cyan
foreach ($service in $services) {
    $dockerfileName = "Dockerfile.$service"
    $imageName = "$service-lambda"
    $ecrUri = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$imageName`:latest"
    
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host "Processing: $service" -ForegroundColor Magenta
    Write-Host "========================================" -ForegroundColor Magenta
    
    # Build
    Write-Host "Building $service for Linux/AMD64..." -ForegroundColor Yellow
    $env:DOCKER_BUILDKIT = "0"
    docker build --platform linux/amd64 -f $dockerfileName -t $imageName .
    $env:DOCKER_BUILDKIT = "1"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed for $service" -ForegroundColor Red
        continue
    }
    Write-Host "Build complete: $service" -ForegroundColor Green
    
    # Tag
    Write-Host "Tagging $service..." -ForegroundColor Yellow
    docker tag "$imageName`:latest" $ecrUri
    Write-Host "Tagged: $service" -ForegroundColor Green
    
    # Push
    Write-Host "Pushing $service to ECR..." -ForegroundColor Yellow
    docker push $ecrUri
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Push failed for $service" -ForegroundColor Red
        continue
    }
    Write-Host "Pushed: $service" -ForegroundColor Green
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "All services deployed to ECR!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan