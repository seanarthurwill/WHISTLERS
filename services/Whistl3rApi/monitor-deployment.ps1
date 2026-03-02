# Monitor ECS Deployment in Real-Time

$AlbUrl = "http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com"

Write-Host "`n╔════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   🔍 ECS Deployment Monitor               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "Monitoring your deployment...`n" -ForegroundColor Yellow

# Monitor deployment progress
$maxAttempts = 30
$attempt = 0

while ($attempt -lt $maxAttempts) {
    $attempt++
    
    Write-Host "[$attempt/$maxAttempts] Checking deployment status..." -ForegroundColor Gray
    
    # Check ECS service
    try {
        $serviceJson = aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2 2>&1
        $service = $serviceJson | ConvertFrom-Json
        
        if ($service.services -and $service.services.Count -gt 0) {
            $svc = $service.services[0]
            $status = $svc.status
            $desired = $svc.desiredCount
            $running = $svc.runningCount
            $pending = $svc.pendingCount
            
            Write-Host "  Service Status: $status" -ForegroundColor $(if ($status -eq "ACTIVE") { "Green" } else { "Yellow" })
            Write-Host "  Tasks: $running running, $pending pending (desired: $desired)" -ForegroundColor Gray
            
            # Check if deployment is complete
            if ($status -eq "ACTIVE" -and $running -eq $desired -and $pending -eq 0) {
                Write-Host "`n✅ Deployment complete!" -ForegroundColor Green
                break
            }
            
            # Check for recent deployments
            if ($svc.deployments -and $svc.deployments.Count -gt 0) {
                $primaryDeployment = $svc.deployments | Where-Object { $_.status -eq "PRIMARY" } | Select-Object -First 1
                if ($primaryDeployment) {
                    Write-Host "  Deployment: $($primaryDeployment.rolloutState)" -ForegroundColor Cyan
                }
            }
        }
    }
    catch {
        Write-Host "  ⚠️ Could not fetch service status" -ForegroundColor Yellow
    }
    
    # Check health endpoint every 5 attempts
    if ($attempt % 5 -eq 0) {
        Write-Host "  Testing health endpoint..." -ForegroundColor Gray
        try {
            $health = Invoke-RestMethod -Uri "$AlbUrl/health" -TimeoutSec 5 -ErrorAction Stop
            Write-Host "  ✅ Health check passed!" -ForegroundColor Green
            Write-Host "     Status: $($health.status)" -ForegroundColor Gray
            Write-Host "     Environment: $($health.environment)" -ForegroundColor Gray
        }
        catch {
            Write-Host "  ⏳ Health check not ready yet" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Start-Sleep -Seconds 10
}

if ($attempt -ge $maxAttempts) {
    Write-Host "⏰ Monitoring timeout. Deployment may still be in progress." -ForegroundColor Yellow
    Write-Host "   Check GitHub Actions: https://github.com/seanarthurwill/WHISTLERS/actions" -ForegroundColor Cyan
}

Write-Host "`n════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan

# Final status check
try {
    $serviceJson = aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2 2>&1
    $service = $serviceJson | ConvertFrom-Json
    
    if ($service.services -and $service.services.Count -gt 0) {
        $svc = $service.services[0]
        
        Write-Host "Service: $($svc.serviceName)" -ForegroundColor White
        Write-Host "Status: $($svc.status)" -ForegroundColor $(if ($svc.status -eq "ACTIVE") { "Green" } else { "Yellow" })
        Write-Host "Running Tasks: $($svc.runningCount) / $($svc.desiredCount)" -ForegroundColor White
        
        # Get task ARNs
        $tasksJson = aws ecs list-tasks --cluster whistl3r-cluster --service-name whistl3r-api-service --region us-east-2 2>&1
        $tasks = $tasksJson | ConvertFrom-Json
        
        if ($tasks.taskArns -and $tasks.taskArns.Count -gt 0) {
            Write-Host "`nRunning Tasks:" -ForegroundColor Yellow
            foreach ($taskArn in $tasks.taskArns) {
                $taskId = $taskArn.Split('/')[-1]
                Write-Host "  - $taskId" -ForegroundColor Gray
            }
        }
    }
}
catch {
    Write-Host "Could not retrieve final status" -ForegroundColor Red
}

Write-Host "`n════════════════════════════════════════════`n" -ForegroundColor Cyan

# Test the API
Write-Host "Testing API Endpoints..." -ForegroundColor Yellow

Write-Host "`n1. Health Check:" -ForegroundColor White
try {
    $health = Invoke-RestMethod -Uri "$AlbUrl/health" -TimeoutSec 10
    Write-Host "   ✅ Status: $($health.status)" -ForegroundColor Green
    Write-Host "   Environment: $($health.environment)" -ForegroundColor Gray
    Write-Host "   Timestamp: $($health.timestamp)" -ForegroundColor Gray
}
catch {
    Write-Host "   ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2. Swagger UI:" -ForegroundColor White
Write-Host "   URL: $AlbUrl/swagger" -ForegroundColor Cyan
Write-Host "   Opening in browser..." -ForegroundColor Gray
Start-Process "$AlbUrl/swagger"

Write-Host "`n════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "View logs:" -ForegroundColor White
Write-Host "  aws logs tail /ecs/whistl3r-api --follow --region us-east-2`n" -ForegroundColor Gray

Write-Host "Check service:" -ForegroundColor White
Write-Host "  aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2`n" -ForegroundColor Gray

Write-Host "Force redeploy:" -ForegroundColor White
Write-Host "  aws ecs update-service --cluster whistl3r-cluster --service whistl3r-api-service --force-new-deployment --region us-east-2`n" -ForegroundColor Gray

Write-Host "GitHub Actions:" -ForegroundColor White
Write-Host "  https://github.com/seanarthurwill/WHISTLERS/actions`n" -ForegroundColor Cyan

Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan
