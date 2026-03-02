# Test Whistl3r Deployment Script

$AlbUrl = "http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Testing Whistl3r ECS Deployment" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test 1: Health Check
Write-Host "Test 1: Health Check Endpoint..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "$AlbUrl/health" -TimeoutSec 10
    Write-Host "✓ Health Check Passed!" -ForegroundColor Green
    Write-Host "  Status: $($healthResponse.status)" -ForegroundColor Gray
    Write-Host "  Environment: $($healthResponse.environment)" -ForegroundColor Gray
    Write-Host "  Timestamp: $($healthResponse.timestamp)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Health Check Failed (ALB may still be provisioning)" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host ""

# Test 2: ECS Service Status
Write-Host "Test 2: ECS Service Status..." -ForegroundColor Yellow
try {
    $serviceStatus = aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2 --query "services[0].[status,runningCount,desiredCount]" --output json | ConvertFrom-Json
    Write-Host "✓ Service Status: $($serviceStatus[0])" -ForegroundColor Green
    Write-Host "  Running Tasks: $($serviceStatus[1]) / $($serviceStatus[2])" -ForegroundColor Gray
} catch {
    Write-Host "✗ Could not retrieve service status" -ForegroundColor Red
}

Write-Host ""

# Test 3: Task Status
Write-Host "Test 3: Running Tasks..." -ForegroundColor Yellow
try {
    $tasks = aws ecs list-tasks --cluster whistl3r-cluster --service-name whistl3r-api-service --region us-east-2 --query "taskArns" --output json | ConvertFrom-Json
    if ($tasks.Count -gt 0) {
        Write-Host "✓ Found $($tasks.Count) running task(s)" -ForegroundColor Green
        foreach ($task in $tasks) {
            Write-Host "  - $task" -ForegroundColor Gray
        }
    } else {
        Write-Host "⚠ No tasks running yet (deployment may be in progress)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Could not retrieve task status" -ForegroundColor Red
}

Write-Host ""

# Test 4: Target Health
Write-Host "Test 4: Load Balancer Target Health..." -ForegroundColor Yellow
try {
    $targetGroupArn = "arn:aws:elasticloadbalancing:us-east-2:636017849911:targetgroup/whistl3r-tg/d2edef2a27220bb7"
    $targetHealth = aws elbv2 describe-target-health --target-group-arn $targetGroupArn --region us-east-2 --query "TargetHealthDescriptions[*].[Target.Id,TargetHealth.State]" --output json | ConvertFrom-Json
    
    if ($targetHealth.Count -gt 0) {
        Write-Host "✓ Found $($targetHealth.Count) target(s)" -ForegroundColor Green
        foreach ($target in $targetHealth) {
            $state = $target[1]
            $color = if ($state -eq "healthy") { "Green" } elseif ($state -eq "initial") { "Yellow" } else { "Red" }
            Write-Host "  - Target: $($target[0]) - State: $state" -ForegroundColor $color
        }
    } else {
        Write-Host "⚠ No targets registered yet" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Could not retrieve target health" -ForegroundColor Red
}

Write-Host ""

# Test 5: Recent Logs
Write-Host "Test 5: Recent CloudWatch Logs..." -ForegroundColor Yellow
try {
    Write-Host "✓ Fetching last 5 log entries..." -ForegroundColor Green
    aws logs tail /ecs/whistl3r-api --since 5m --region us-east-2 --format short 2>$null | Select-Object -Last 10
} catch {
    Write-Host "⚠ No logs available yet or log group not accessible" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Complete" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Quick Access URLs:" -ForegroundColor Yellow
Write-Host "  API Health: $AlbUrl/health" -ForegroundColor Cyan
Write-Host "  Swagger UI: $AlbUrl/swagger" -ForegroundColor Cyan
Write-Host "`nAWS Console Links:" -ForegroundColor Yellow
Write-Host "  ECS Service: https://us-east-2.console.aws.amazon.com/ecs/v2/clusters/whistl3r-cluster/services/whistl3r-api-service" -ForegroundColor Cyan
Write-Host "  CloudWatch Logs: https://us-east-2.console.aws.amazon.com/cloudwatch/home?region=us-east-2#logsV2:log-groups/log-group//ecs/whistl3r-api" -ForegroundColor Cyan
Write-Host "  Load Balancer: https://us-east-2.console.aws.amazon.com/ec2/home?region=us-east-2#LoadBalancers:" -ForegroundColor Cyan
Write-Host ""
