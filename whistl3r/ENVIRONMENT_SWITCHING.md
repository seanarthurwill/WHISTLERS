# Environment Switching Guide

## How It Works

Vite automatically uses different `.env` files based on the mode:
- **Development mode** → Uses `.env.development` (localhost)
- **Production mode** → Uses `.env.production` (AWS API Gateway)
- **Default** → Uses `.env` (fallback)

## Quick Commands

### Local Development (localhost:5000)
```bash
npm run dev:local
```
or just:
```bash
npm run dev
```

### AWS Testing (API Gateway)
```bash
npm run dev:aws
```

### Build for Local
```bash
npm run build:local
```

### Build for AWS Deployment
```bash
npm run build:aws
```
or just:
```bash
npm run build
```

## Environment Files Summary

| File | Points To | When Used |
|------|-----------|-----------|
| `.env.development` | `http://localhost:5000/api` | `npm run dev` or `npm run dev:local` |
| `.env.production` | AWS API Gateway | `npm run dev:aws` or `npm run build` |
| `.env` | AWS API Gateway | Fallback if specific env not found |

## Current Configuration

### .env.development
```
VITE_API_URL=http://localhost:5000/api
```

### .env.production
```
VITE_API_URL=https://32avbpfsw6.execute-api.us-east-2.amazonaws.com/api
```

## Typical Workflow

### 1. Developing Locally
```bash
# Terminal 1: Run your local API Gateway (if you have one)
cd services/ApiGateway
dotnet run

# Terminal 2: Run React app pointing to localhost
cd whistl3r
npm run dev:local
```

### 2. Testing AWS Deployment
```bash
# Run React app pointing to AWS
cd whistl3r
npm run dev:aws
```

### 3. Production Build
```bash
# Build for AWS deployment
cd whistl3r
npm run build:aws
```

## Verifying Current Environment

Add this to any component to check which API is being used:
```javascript
console.log('API URL:', import.meta.env.VITE_API_URL);
```

## Switching On-The-Fly

If you need to switch without restarting:

1. Create `.env.local` (this overrides all other env files):
```
VITE_API_URL=http://localhost:5000/api
```

2. Or use command line:
```bash
# PowerShell
$env:VITE_API_URL="http://localhost:5000/api"; npm run dev

# Bash/Linux
VITE_API_URL=http://localhost:5000/api npm run dev
```

## Troubleshooting

**Changes not taking effect?**
- Restart the dev server after changing .env files
- Vite only reads env variables when it starts

**Which env file is being used?**
- Check the console output when starting dev server
- Look for: `mode: development` or `mode: production`

**Variables not defined?**
- Make sure they start with `VITE_`
- Restart dev server after adding new variables
