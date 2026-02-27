# CI/CD Deployment Setup

This repository is configured to automatically deploy Lambda functions to AWS when you push to the `main` or `master` branch.

## GitHub Secrets Configuration

Before the automated deployment will work, you need to add AWS credentials as GitHub secrets:

### Steps to Add Secrets:

1. Go to your GitHub repository
2. Click on **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secrets:

| Secret Name | Description | How to Get |
|------------|-------------|------------|
| `AWS_ACCESS_KEY_ID` | Your AWS Access Key ID | From AWS IAM console |
| `AWS_SECRET_ACCESS_KEY` | Your AWS Secret Access Key | From AWS IAM console |

### Creating AWS IAM Credentials:

If you don't already have an IAM user for deployments:

1. Go to AWS Console → **IAM** → **Users**
2. Click **Create user**
3. Name it `github-actions-deploy` (or similar)
4. Attach the following policies:
   - `AmazonEC2ContainerRegistryFullAccess` (for ECR)
   - `AWSLambda_FullAccess` (for Lambda updates)
   - Or create a custom policy with minimal permissions:
     ```json
     {
       "Version": "2012-10-17",
       "Statement": [
         {
           "Effect": "Allow",
           "Action": [
             "ecr:GetAuthorizationToken",
             "ecr:BatchCheckLayerAvailability",
             "ecr:GetDownloadUrlForLayer",
             "ecr:BatchGetImage",
             "ecr:PutImage",
             "ecr:InitiateLayerUpload",
             "ecr:UploadLayerPart",
             "ecr:CompleteLayerUpload",
             "ecr:CreateRepository",
             "ecr:DescribeRepositories",
             "lambda:UpdateFunctionCode",
             "lambda:GetFunction"
           ],
           "Resource": "*"
         }
       ]
     }
     ```
5. Create **Access Key** → **Application running outside AWS**
6. Copy the Access Key ID and Secret Access Key
7. Add them as GitHub secrets

## Workflow Details

The workflow defined in `.github/workflows/deploy-lambdas.yml` will:

1. **Trigger** on every push to `main` or `master` branch
2. **Build** Docker images for all services (users, games, organizations, assignors, communication, reviews, groups, payscale)
3. **Push** images to Amazon ECR
4. **Update** Lambda functions with new images
5. **Wait** for functions to become active

## Manual Deployment

You can also trigger the deployment manually:

1. Go to **Actions** tab in GitHub
2. Select **Deploy Lambda Functions to AWS**
3. Click **Run workflow**
4. Select the branch and click **Run workflow**

## Local Deployment

To deploy from your local machine (as before):

```powershell
# Build and push to ECR
.\deploy.ps1

# Update Lambda functions
.\generated_content\update-lambdas.ps1
```

## Monitoring Deployments

- View deployment status in the **Actions** tab of your GitHub repository
- Each step shows detailed logs
- Failed deployments will show error messages

## Troubleshooting

### Deployment Fails with AWS Permission Error
- Verify AWS secrets are correctly set in GitHub
- Check IAM user has necessary permissions

### Docker Build Fails
- Check Dockerfile syntax
- Ensure all dependencies are available
- Review build logs in Actions tab

### Lambda Update Fails
- Verify Lambda functions exist in AWS
- Check function names match the expected pattern: `{service}-lambda`
- Ensure IAM user has `lambda:UpdateFunctionCode` permission

## Services Deployed

The following services are automatically built and deployed:
- users-lambda
- games-lambda
- organizations-lambda
- assignors-lambda
- communication-lambda
- reviews-lambda
- groups-lambda
- payscale-lambda

## AWS Configuration

- **Region**: us-east-2
- **Account ID**: 636017849911
- **ECR Repository Pattern**: `{service}-lambda`
- **Lambda Function Pattern**: `{service}-lambda`
