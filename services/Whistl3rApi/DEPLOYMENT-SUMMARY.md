# Whistl3r API - AWS ECS Deployment Summary

## ✅ Setup Complete!

All AWS resources have been created successfully.

## 📋 Important Information

### AWS Resources Created

- **ECS Cluster**: whistl3r-cluster
- **ECS Service**: whistl3r-api-service
- **ECR Repository**: whistl3r-api
- **Load Balancer**: whistl3r-alb
- **Target Group**: whistl3r-tg
- **Security Group**: sg-0829c29072280a5fb
- **VPC**: vpc-093e6317f01f98b83
- **Subnets**: subnet-012a5a5da24916221, subnet-059184881e6f08409

### Application URLs

- **API Base URL**: http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com
- **Health Check**: http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com/health
- **Swagger UI**: http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com/swagger

### GitHub Actions Credentials

⚠️ **Important**: You need to add AWS credentials to GitHub Secrets for automated deployments.

The credentials were created during setup and should be added at:
- Go to: https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions

**Required Secrets:**
- **AWS_ACCESS_KEY_ID**: (from the `github-actions-whistl3r` IAM user)
- **AWS_SECRET_ACCESS_KEY**: (from the `github-actions-whistl3r` IAM user)

> **Note**: The actual credential values were provided during setup. If you didn't save them, you can create new credentials by running:
> ```powershell
> aws iam create-access-key --user-name github-actions-whistl3r
> ```

## 📝 Next Steps

### 1. Add GitHub Secrets

Go to your repository settings and add the AWS credentials:
```
https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions
```

### 2. Update CORS in Program.cs

Add the ALB URL to your CORS policy:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "http://localhost:3000",
            "http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com"  // Add this line
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});
```

### 3. Commit and Push

Your GitHub Actions workflow is already configured. Simply push to master:

```powershell
git add .
git commit -m "Add ECS deployment configuration"
git push origin master
```

This will trigger the automated deployment!

## 🧪 Testing

### Test Health Endpoint

```powershell
Invoke-RestMethod -Uri http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com/health
```

### View ECS Service Status

```powershell
aws ecs describe-services --cluster whistl3r-cluster --services whistl3r-api-service --region us-east-2
```

### View Running Tasks

```powershell
aws ecs list-tasks --cluster whistl3r-cluster --service-name whistl3r-api-service --region us-east-2
```

### View Logs

```powershell
aws logs tail /ecs/whistl3r-api --follow --region us-east-2
```

## 🔄 Manual Deployment

If you need to deploy manually:

```powershell
# Login to ECR
aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin 636017849911.dkr.ecr.us-east-2.amazonaws.com

# Build image
docker build -t whistl3r-api .

# Tag image
docker tag whistl3r-api:latest 636017849911.dkr.ecr.us-east-2.amazonaws.com/whistl3r-api:latest

# Push to ECR
docker push 636017849911.dkr.ecr.us-east-2.amazonaws.com/whistl3r-api:latest

# Force new deployment
aws ecs update-service --cluster whistl3r-cluster --service whistl3r-api-service --force-new-deployment --region us-east-2
```

## 🔧 Troubleshooting

### Container Won't Start

Check logs:
```powershell
aws logs tail /ecs/whistl3r-api --follow --region us-east-2
```

### Task Definition Issues

Re-register task definition:
```powershell
aws ecs register-task-definition --cli-input-json file://.aws/task-definition.json --region us-east-2
```

### Force Redeploy

```powershell
aws ecs update-service --cluster whistl3r-cluster --service whistl3r-api-service --force-new-deployment --region us-east-2
```

## 🔐 Secrets Management

Your secrets are stored in AWS Secrets Manager:
- **Database Connection**: whistl3r/db-connection
- **JWT Secret**: whistl3r/jwt

To update a secret:
```powershell
aws secretsmanager update-secret --secret-id whistl3r/jwt --secret-string '{"SecretKey":"NEW-SECRET-HERE"}' --region us-east-2
```

## 📊 Monitoring

- **CloudWatch Logs**: `/ecs/whistl3r-api`
- **ECS Console**: https://us-east-2.console.aws.amazon.com/ecs/v2/clusters/whistl3r-cluster/services/whistl3r-api-service
- **Load Balancer**: https://us-east-2.console.aws.amazon.com/ec2/home?region=us-east-2#LoadBalancers:

## 🚀 Scaling

Update desired task count:
```powershell
aws ecs update-service --cluster whistl3r-cluster --service whistl3r-api-service --desired-count 3 --region us-east-2
```

## ⏱️ Note

The Application Load Balancer may take 2-3 minutes to become fully active and start routing traffic to your containers.

---

**Created**: March 1, 2026
**Region**: us-east-2 (Ohio)
**Account ID**: 636017849911
