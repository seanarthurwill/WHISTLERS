# Whistl3r ECS Setup Script
Write-Host "Starting Whistl3r ECS Setup from Phase 4..." -ForegroundColor Cyan

# Phase 4: Get Account ID
$AccountId = aws sts get-caller-identity --query Account --output text
Write-Host "AWS Account ID: $AccountId" -ForegroundColor Green

# Phase 5: Create Secrets
Write-Host "`nPhase 5: Creating Secrets..." -ForegroundColor Cyan

# Generate JWT secret
$jwtSecret = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 48 | ForEach-Object { [char]$_ })
Write-Host "Generated JWT Secret: $jwtSecret" -ForegroundColor Gray

# Create DB connection secret
$dbSecretString = '{"ConnectionString":"Host=whistl3r-1-instance-1.cno80gy6gzh5.us-east-2.rds.amazonaws.com;Port=5432;Database=postgres;Username=headofficial;Password=0pt1m0sPr1m3.;SSL Mode=Require;Search Path=public"}'
aws secretsmanager create-secret --name whistl3r/db-connection --description "PostgreSQL connection string" --secret-string $dbSecretString --region us-east-2 2>&1 | Out-Null
Write-Host "DB connection secret created (or already exists)" -ForegroundColor Green

# Create JWT secret
$jwtSecretString = "{`"SecretKey`":`"$jwtSecret`"}"
aws secretsmanager create-secret --name whistl3r/jwt --description "JWT signing key" --secret-string $jwtSecretString --region us-east-2 2>&1 | Out-Null
Write-Host "JWT secret created (or already exists)" -ForegroundColor Green

# Verify secrets
$dbSecretArn = aws secretsmanager describe-secret --secret-id whistl3r/db-connection --region us-east-2 --query ARN --output text
$jwtSecretArn = aws secretsmanager describe-secret --secret-id whistl3r/jwt --region us-east-2 --query ARN --output text
Write-Host "DB Secret ARN: $dbSecretArn" -ForegroundColor Green
Write-Host "JWT Secret ARN: $jwtSecretArn" -ForegroundColor Green

# Phase 6: Create Infrastructure
Write-Host "`nPhase 6: Creating AWS Infrastructure..." -ForegroundColor Cyan

# Create ECR Repository
aws ecr create-repository --repository-name whistl3r-api --region us-east-2 --image-scanning-configuration scanOnPush=true --tags Key=Project,Value=Whistl3r 2>&1 | Out-Null
Write-Host "ECR repository created (or already exists)" -ForegroundColor Green

# Create CloudWatch Log Group
aws logs create-log-group --log-group-name /ecs/whistl3r-api --region us-east-2 2>&1 | Out-Null
Write-Host "CloudWatch log group created (or already exists)" -ForegroundColor Green

# Create ECS Cluster
aws ecs create-cluster --cluster-name whistl3r-cluster --region us-east-2 --tags key=Project,value=Whistl3r 2>&1 | Out-Null
Write-Host "ECS Cluster created (or already exists)" -ForegroundColor Green

# Get VPC Info
Write-Host "`nGetting VPC Information..." -ForegroundColor Cyan
$VpcId = aws ec2 describe-vpcs --region us-east-2 --filters "Name=is-default,Values=true" --query "Vpcs[0].VpcId" --output text
Write-Host "Using VPC: $VpcId" -ForegroundColor Green

$SubnetsJson = aws ec2 describe-subnets --region us-east-2 --filters "Name=vpc-id,Values=$VpcId" --query "Subnets[*].SubnetId" --output json
$Subnets = $SubnetsJson | ConvertFrom-Json
$SubnetId1 = $Subnets[0]
$SubnetId2 = $Subnets[1]
Write-Host "Subnet 1: $SubnetId1" -ForegroundColor Green
Write-Host "Subnet 2: $SubnetId2" -ForegroundColor Green

# Create Security Group
Write-Host "`nCreating Security Group..." -ForegroundColor Cyan
$existingSG = aws ec2 describe-security-groups --region us-east-2 --filters "Name=group-name,Values=whistl3r-ecs-sg" --query "SecurityGroups[0].GroupId" --output text 2>&1

if ($existingSG -and $existingSG -ne "None" -and $existingSG -notlike "*error*" -and $existingSG.Length -gt 5) {
    $SecurityGroupId = $existingSG
    Write-Host "Using existing Security Group: $SecurityGroupId" -ForegroundColor Green
} else {
    $SecurityGroupId = aws ec2 create-security-group --group-name whistl3r-ecs-sg --description "Security group for Whistl3r ECS tasks" --vpc-id $VpcId --region us-east-2 --query "GroupId" --output text
    
    # Add ingress rules
    aws ec2 authorize-security-group-ingress --group-id $SecurityGroupId --protocol tcp --port 8080 --cidr 0.0.0.0/0 --region us-east-2 2>&1 | Out-Null
    aws ec2 authorize-security-group-ingress --group-id $SecurityGroupId --protocol tcp --port 443 --cidr 0.0.0.0/0 --region us-east-2 2>&1 | Out-Null
    
    Write-Host "Security Group created: $SecurityGroupId" -ForegroundColor Green
}

# Create ALB
Write-Host "`nCreating Application Load Balancer..." -ForegroundColor Cyan
$existingALB = aws elbv2 describe-load-balancers --names whistl3r-alb --region us-east-2 --query "LoadBalancers[0].LoadBalancerArn" --output text 2>&1

if ($existingALB -and $existingALB -ne "None" -and $existingALB -notlike "*error*" -and $existingALB.StartsWith("arn:")) {
    $LoadBalancerArn = $existingALB
    Write-Host "Using existing Load Balancer" -ForegroundColor Green
} else {
    $LoadBalancerArn = aws elbv2 create-load-balancer --name whistl3r-alb --subnets $SubnetId1 $SubnetId2 --security-groups $SecurityGroupId --region us-east-2 --scheme internet-facing --type application --query "LoadBalancers[0].LoadBalancerArn" --output text
    Write-Host "Load Balancer created" -ForegroundColor Green
}

$AlbDns = aws elbv2 describe-load-balancers --load-balancer-arns $LoadBalancerArn --region us-east-2 --query "LoadBalancers[0].DNSName" --output text
Write-Host "ALB DNS: $AlbDns" -ForegroundColor Cyan

# Create Target Group
Write-Host "`nCreating Target Group..." -ForegroundColor Cyan
$existingTG = aws elbv2 describe-target-groups --names whistl3r-tg --region us-east-2 --query "TargetGroups[0].TargetGroupArn" --output text 2>&1

if ($existingTG -and $existingTG -ne "None" -and $existingTG -notlike "*error*" -and $existingTG.StartsWith("arn:")) {
    $TargetGroupArn = $existingTG
    Write-Host "Using existing Target Group" -ForegroundColor Green
} else {
    $TargetGroupArn = aws elbv2 create-target-group --name whistl3r-tg --protocol HTTP --port 8080 --vpc-id $VpcId --target-type ip --health-check-enabled --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region us-east-2 --query "TargetGroups[0].TargetGroupArn" --output text
    Write-Host "Target Group created" -ForegroundColor Green
}

# Create Listener
$existingListener = aws elbv2 describe-listeners --load-balancer-arn $LoadBalancerArn --region us-east-2 --query "Listeners[0].ListenerArn" --output text 2>&1

if ($existingListener -and $existingListener -ne "None" -and $existingListener -notlike "*error*" -and $existingListener.StartsWith("arn:")) {
    Write-Host "Listener already exists" -ForegroundColor Green
} else {
    aws elbv2 create-listener --load-balancer-arn $LoadBalancerArn --protocol HTTP --port 80 --default-actions "Type=forward,TargetGroupArn=$TargetGroupArn" --region us-east-2 2>&1 | Out-Null
    Write-Host "Listener created" -ForegroundColor Green
}

# Phase 7: Register Task Definition
Write-Host "`nPhase 7: Registering Task Definition..." -ForegroundColor Cyan
aws ecs register-task-definition --cli-input-json file://.aws/task-definition.json --region us-east-2 2>&1 | Out-Null
Write-Host "Task Definition registered" -ForegroundColor Green

# Phase 8: Create ECS Service
Write-Host "`nPhase 8: Creating ECS Service..." -ForegroundColor Cyan
$existingService = aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2 --query "services[0].serviceName" --output text 2>&1

if ($existingService -eq "whistl3r-api-service") {
    Write-Host "Service already exists. Updating instead..." -ForegroundColor Yellow
    aws ecs update-service --cluster whistl3r-cluster --service whistl3r-api-service --task-definition whistl3r-api --desired-count 2 --force-new-deployment --region us-east-2 2>&1 | Out-Null
    Write-Host "Service updated" -ForegroundColor Green
} else {
    aws ecs create-service --cluster whistl3r-cluster --service-name whistl3r-api-service --task-definition whistl3r-api --desired-count 2 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[$SubnetId1,$SubnetId2],securityGroups=[$SecurityGroupId]}" --load-balancers "targetGroupArn=$TargetGroupArn,containerName=whistl3r-api,containerPort=8080" --health-check-grace-period-seconds 60 --region us-east-2 2>&1 | Out-Null
    Write-Host "ECS Service created" -ForegroundColor Green
}

# Phase 9: Create GitHub Actions User
Write-Host "`nPhase 9: Setting up GitHub Actions User..." -ForegroundColor Cyan
$existingUser = aws iam get-user --user-name github-actions-whistl3r --query "User.UserName" --output text 2>&1

if ($existingUser -eq "github-actions-whistl3r") {
    Write-Host "GitHub Actions user already exists" -ForegroundColor Yellow
    Write-Host "If you need new credentials, delete and recreate the user" -ForegroundColor Gray
} else {
    aws iam create-user --user-name github-actions-whistl3r 2>&1 | Out-Null
    aws iam attach-user-policy --user-name github-actions-whistl3r --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser
    aws iam attach-user-policy --user-name github-actions-whistl3r --policy-arn arn:aws:iam::aws:policy/AmazonECS_FullAccess
    
    $accessKeyJson = aws iam create-access-key --user-name github-actions-whistl3r
    $accessKey = $accessKeyJson | ConvertFrom-Json
    
    Write-Host "`nGitHub Secrets (add these to your repo):" -ForegroundColor Yellow
    Write-Host "AWS_ACCESS_KEY_ID: $($accessKey.AccessKey.AccessKeyId)" -ForegroundColor Cyan
    Write-Host "AWS_SECRET_ACCESS_KEY: $($accessKey.AccessKey.SecretAccessKey)" -ForegroundColor Cyan
    Write-Host "`nGo to: https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions" -ForegroundColor Gray
}

# Summary
Write-Host "`nSetup Complete!" -ForegroundColor Green
Write-Host "`nImportant Information:" -ForegroundColor Yellow
Write-Host "ALB URL: http://$AlbDns" -ForegroundColor Cyan
Write-Host "Health Check: http://$AlbDns/health" -ForegroundColor Cyan
Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Add AWS credentials to GitHub Secrets (if not already done)"
Write-Host "2. Update CORS in Program.cs to include: http://$AlbDns"
Write-Host "3. Commit and push to master branch to trigger deployment"
Write-Host "`nTest deployment:" -ForegroundColor Yellow
Write-Host "  Invoke-RestMethod -Uri http://$AlbDns/health" -ForegroundColor Gray
Write-Host "`nNote: ALB may take 2-3 minutes to become active" -ForegroundColor Gray
