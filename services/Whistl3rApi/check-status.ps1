# Quick Deployment Status Check

Write-Host "`n╔════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   📊 Deployment Status Check              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# 1. Check GitHub Actions
Write-Host "1. GitHub Actions Build:" -ForegroundColor Yellow
Write-Host "   Check: https://github.com/seanarthurwill/WHISTLERS/actions" -ForegroundColor Cyan
Write-Host "   Status: Building Docker image and pushing to ECR`n" -ForegroundColor Gray

# 2. Check ECS Service
Write-Host "2. ECS Service:" -ForegroundColor Yellow
try {
    $service = aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2 --query "services[0].[status,runningCount,desiredCount]" --output json 2>&1 | ConvertFrom-Json
    Write-Host "   Status: $($service[0])" -ForegroundColor Green
    Write-Host "   Tasks: $($service[1]) running / $($service[2]) desired`n" -ForegroundColor White
} catch {
    Write-Host "   Could not check service status`n" -ForegroundColor Red
}

# 3. Check Tasks
Write-Host "3. Running Tasks:" -ForegroundColor Yellow
try {
    $tasks = aws ecs list-tasks --cluster whistl3r-cluster --service-name whistl3r-api-service --region us-east-2 --query "taskArns" --output json 2>&1 | ConvertFrom-Json
    if ($tasks -and $tasks.Count -gt 0) {
        Write-Host "   ✅ Found $($tasks.Count) task(s)" -ForegroundColor Green
        foreach ($task in $tasks) {
            $taskId = $task.Split('/')[-1]
            Write-Host "      - $taskId" -ForegroundColor Gray
        }
        Write-Host ""
    } else {
        Write-Host "   ⏳ No tasks running yet`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   Could not check tasks`n" -ForegroundColor Red
}

# 4. Check Target Health
Write-Host "4. Load Balancer Target Health:" -ForegroundColor Yellow
try {
    $targetGroupArn = "arn:aws:elasticloadbalancing:us-east-2:636017849911:targetgroup/whistl3r-tg/d2edef2a27220bb7"
    $targetHealth = aws elbv2 describe-target-health --target-group-arn $targetGroupArn --region us-east-2 --query "TargetHealthDescriptions[*].[Target.Id,TargetHealth.State,TargetHealth.Reason]" --output json 2>&1 | ConvertFrom-Json
    
    if ($targetHealth -and $targetHealth.Count -gt 0) {
        foreach ($target in $targetHealth) {
            $state = $target[1]
            $reason = if ($target[2]) { " - $($target[2])" } else { "" }
            $color = switch ($state) {
                "healthy" { "Green" }
                "initial" { "Yellow" }
                "unhealthy" { "Red" }
                default { "Gray" }
            }
            Write-Host "   $state$reason" -ForegroundColor $color
        }
        Write-Host ""
    } else {
        Write-Host "   ⏳ Targets not registered yet`n" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   Could not check target health`n" -ForegroundColor Red
}

# 5. Test API
Write-Host "5. API Health Check:" -ForegroundColor Yellow
$AlbUrl = "http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com"
try {
    $health = Invoke-RestMethod -Uri "$AlbUrl/health" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✅ API is responding!" -ForegroundColor Green
    Write-Host "      Status: $($health.status)" -ForegroundColor Gray
    Write-Host "      Environment: $($health.environment)" -ForegroundColor Gray
    Write-Host "      URL: $AlbUrl`n" -ForegroundColor Cyan
} catch {
    Write-Host "   ⏳ API not ready yet (this is normal during first deployment)" -ForegroundColor Yellow
    Write-Host "      Will be available after containers start and pass health checks`n" -ForegroundColor Gray
}

Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan

# Deployment Timeline
Write-Host "⏱️  Typical Deployment Timeline:" -ForegroundColor Yellow
Write-Host "   [0-5 min]  GitHub Actions builds Docker image" -ForegroundColor Gray
Write-Host "   [5-7 min]  Image pushed to ECR" -ForegroundColor Gray
Write-Host "   [7-9 min]  ECS starts pulling image" -ForegroundColor Gray
Write-Host "   [9-11 min] Containers start, health checks begin" -ForegroundColor Gray
Write-Host "   [11-13 min] Health checks pass, targets healthy" -ForegroundColor Gray
Write-Host "   [13-15 min] ALB routes traffic to new tasks" -ForegroundColor Gray
Write-Host "   [15+ min]  ✅ Deployment complete!`n" -ForegroundColor Green

Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan

# What to do next
Write-Host "📝 Next Steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "While waiting for deployment:" -ForegroundColor White
Write-Host "  • Monitor GitHub Actions:" -ForegroundColor Gray
Write-Host "    https://github.com/seanarthurwill/WHISTLERS/actions" -ForegroundColor Cyan
Write-Host ""
Write-Host "  • Run this check again in 5 minutes:" -ForegroundColor Gray
Write-Host "    .\check-status.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "  • Watch deployment progress:" -ForegroundColor Gray
Write-Host "    .\monitor-deployment.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "When deployment completes:" -ForegroundColor White
Write-Host "  • Test the API:" -ForegroundColor Gray
Write-Host "    .\test-deployment.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "  • View Swagger UI:" -ForegroundColor Gray
Write-Host "    Start-Process '$AlbUrl/swagger'" -ForegroundColor Cyan
Write-Host ""

Write-Host "════════════════════════════════════════════`n" -ForegroundColor Cyan
