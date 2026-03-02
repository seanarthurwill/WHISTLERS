# Workflow Migration: Lambda → ECS

## What Happened

Your repository had two deployment workflows:
1. **Old Lambda workflow** - Deployed to AWS Lambda functions
2. **New ECS workflow** - Deploys to AWS ECS Fargate

Both were trying to run when you pushed to master, causing the Lambda deployment to fail.

## Changes Made

### 1. Disabled Lambda Workflow
**File**: `.github/workflows/deploy-lambdas.yml` → `.github/workflows/deploy-lambdas.yml.disabled`

The old Lambda workflow has been disabled by renaming it. It's kept as backup but won't run anymore.

### 2. Fixed ECS Workflow
**File**: `services/Whistl3rApi/.github/workflows/deploy-to-ecs.yml`

Created the proper ECS deployment workflow that:
- Triggers on push to `master` branch
- Only runs when files in `services/Whistl3rApi/**` change
- Builds Docker image from your .NET 8 API
- Pushes to Amazon ECR
- Deploys to ECS Fargate cluster

## Workflow Configuration

```yaml
name: Deploy to Amazon ECS

on:
  push:
    branches:
      - master
    paths:
      - 'services/Whistl3rApi/**'  # Only trigger on API changes
  workflow_dispatch:  # Allow manual triggers

env:
  AWS_REGION: us-east-2
  ECR_REPOSITORY: whistl3r-api
  ECS_SERVICE: whistl3r-api-service
  ECS_CLUSTER: whistl3r-cluster
  CONTAINER_NAME: whistl3r-api
```

## How It Works Now

```
Push to master (with API changes)
    ↓
GitHub Actions triggered
    ↓
Checkout code
    ↓
Configure AWS credentials (from secrets)
    ↓
Login to ECR
    ↓
Build Docker image (services/Whistl3rApi/Dockerfile)
    ↓
Tag: latest + git SHA
    ↓
Push to ECR
    ↓
Update ECS task definition
    ↓
Deploy to ECS (zero downtime)
    ↓
Wait for service stability
    ↓
✅ Deployment complete!
```

## Next Steps

### 1. Add GitHub Secrets (REQUIRED)
```powershell
cd C:\dev\services\Whistl3rApi
.\add-github-secrets.ps1
```

Or manually add at: https://github.com/seanarthurwill/WHISTLERS/settings/secrets/actions

**Required Secrets:**
- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`

(Values are in `services/Whistl3rApi/.aws/github-credentials.txt`)

### 2. Test Deployment

Make a small change to trigger the workflow:

```powershell
cd C:\dev\services\Whistl3rApi

# Make a small change
Add-Content -Path README-DEPLOYMENT.md -Value "`n<!-- Test deployment -->"

# Commit and push
git add .
git commit -m "Test: Trigger ECS deployment"
git push origin master
```

### 3. Monitor Deployment

Watch the deployment at:
https://github.com/seanarthurwill/WHISTLERS/actions

You should see:
- ✅ Checkout code
- ✅ Configure AWS Credentials
- ✅ Login to Amazon ECR
- ✅ Build, tag, and push image to Amazon ECR
- ✅ Fill in the new image ID in the Amazon ECS task definition
- ✅ Deploy Amazon ECS task definition
- ✅ Deployment successful

### 4. Verify Live Deployment

After deployment completes:

```powershell
# Test health endpoint
Invoke-RestMethod -Uri http://whistl3r-alb-438377692.us-east-2.elb.amazonaws.com/health

# Check running tasks
aws ecs list-tasks --cluster whistl3r-cluster --service-name whistl3r-api-service --region us-east-2

# View logs
aws logs tail /ecs/whistl3r-api --follow --region us-east-2
```

## Troubleshooting

### Deployment Fails with "Unable to locate credentials"
**Solution**: Add AWS credentials to GitHub Secrets (see step 1 above)

### Deployment Fails at "Login to ECR"
**Solution**: Verify `AWS_ACCESS_KEY_ID` and `AWS_SECRET_ACCESS_KEY` are correct in GitHub Secrets

### Deployment Succeeds but Health Check Fails
**Solution**: 
- Check CloudWatch logs: `aws logs tail /ecs/whistl3r-api --follow --region us-east-2`
- Verify database connection in Secrets Manager
- Check security group allows port 8080

### Workflow Doesn't Trigger
**Possible causes**:
- No changes to `services/Whistl3rApi/**` files
- Push to wrong branch (must be `master`)
- Workflow syntax error (check Actions tab for errors)

## Architecture Comparison

### Old: Lambda Functions
- Multiple separate functions
- Individual deployments per function
- API Gateway for routing
- Cold start issues
- Limited to 15 minutes execution

### New: ECS Fargate
- Single unified API
- Containerized .NET 8 application
- Application Load Balancer
- Always warm (2+ tasks running)
- No execution time limits
- Easy horizontal scaling
- Better for long-running operations

## Rollback Plan

If you need to go back to Lambda deployments:

```powershell
cd C:\dev

# Re-enable Lambda workflow
Rename-Item -Path ".github\workflows\deploy-lambdas.yml.disabled" -NewName "deploy-lambdas.yml"

# Disable ECS workflow
Rename-Item -Path "services\Whistl3rApi\.github\workflows\deploy-to-ecs.yml" -NewName "deploy-to-ecs.yml.disabled"

# Commit and push
git add .
git commit -m "Rollback to Lambda deployments"
git push origin master
```

## Cost Considerations

### ECS Fargate (Current)
- **Base Cost**: ~$30-40/month for 2 tasks (0.5 vCPU, 1GB RAM)
- **Scaling**: Pay per task hour
- **Always Running**: Yes (recommended for production APIs)

### Lambda (Previous)
- **Base Cost**: Free tier (1M requests/month)
- **Scaling**: Pay per request + duration
- **Always Running**: No (cold starts)

For a production API with consistent traffic, ECS is typically more cost-effective and performant.

## Questions?

Check the full deployment documentation:
- `services/Whistl3rApi/README-DEPLOYMENT.md` - Quick start
- `services/Whistl3rApi/DEPLOYMENT-SUMMARY.md` - Complete reference

---

**Migration Date**: March 1, 2026
**Status**: ✅ Complete
**Infrastructure**: AWS ECS Fargate + Application Load Balancer
