# Update all services with JSON cycle fix and redeploy
$ErrorActionPreference = "Stop"

# Refresh PATH
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
$env:DOCKER_BUILDKIT = "0"

$accountId = "636017849911"
$region = "us-east-2"

# JSON fix to add
$jsonFix = @'
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
'@

$services = @(
    @{Name="Games"; Dockerfile="Dockerfile.games"; Repo="games-lambda"},
    @{Name="Organizations"; Dockerfile="Dockerfile.organizations"; Repo="organizations-lambda"},
    @{Name="Assignors"; Dockerfile="Dockerfile.assignors"; Repo="assignors-lambda"},
    @{Name="Communication"; Dockerfile="Dockerfile.communication"; Repo="communication-lambda"},
    @{Name="Reviews"; Dockerfile="Dockerfile.reviews"; Repo="reviews-lambda"},
    @{Name="Groups"; Dockerfile="Dockerfile.groups"; Repo="groups-lambda"},
    @{Name="PayScale"; Dockerfile="Dockerfile.payscale"; Repo="payscale-lambda"}
)

# Login to ECR once
Write-Host "Logging into ECR..." -ForegroundColor Yellow
aws ecr get-login-password --region $region --profile deploy | docker login --username AWS --password-stdin "$accountId.dkr.ecr.$region.amazonaws.com" 2>$null | Out-Null

foreach ($service in $services) {
    $serviceName = $service.Name
    $dockerfile = $service.Dockerfile
    $repo = $service.Repo
    
    Write-Host "`nProcessing $serviceName..." -ForegroundColor Cyan
    
    # Update Program.cs
    $programFile = "services/$serviceName/Program.cs"
    if (Test-Path $programFile) {
        $content = Get-Content $programFile -Raw
        if ($content -notmatch "ReferenceHandler.IgnoreCycles") {
            Write-Host "  Updating Program.cs..." -ForegroundColor Yellow
            $content = $content -replace 'builder\.Services\.AddControllers\(\);', $jsonFix
            $content | Set-Content $programFile -NoNewline
        }
    }
    
    # Build Docker image
    Write-Host "  Building Docker image..." -ForegroundColor Yellow
    docker build --platform linux/amd64 -t $repo -f $dockerfile . --quiet | Out-Null
    
    # Tag and push
    Write-Host "  Pushing to ECR..." -ForegroundColor Yellow
    docker tag "${repo}:latest" "$accountId.dkr.ecr.$region.amazonaws.com/${repo}:latest"
    docker push "$accountId.dkr.ecr.$region.amazonaws.com/${repo}:latest" --quiet | Out-Null
    
    # Update Lambda
    Write-Host "  Updating Lambda function..." -ForegroundColor Yellow
    aws lambda update-function-code `
        --function-name $repo `
        --image-uri "$accountId.dkr.ecr.$region.amazonaws.com/${repo}:latest" `
        --region $region `
        --profile deploy `
        --no-cli-pager | Out-Null
    
    Write-Host "  $serviceName deployed!" -ForegroundColor Green
}

Write-Host "`nAll services updated and deployed!" -ForegroundColor Green
Write-Host "Waiting 30 seconds for all deployments to complete..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "`nAPI Gateway Endpoint: https://32avbpfsw6.execute-api.us-east-2.amazonaws.com" -ForegroundColor Cyan
Write-Host "`nTest endpoints:"
Write-Host "  https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/users"
Write-Host "  https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/games"
Write-Host "  https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api/organizations"
