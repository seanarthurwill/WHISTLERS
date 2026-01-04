# Docker Deployment Guide

## Quick Start

### Build and run all services:
```powershell
docker-compose -f docker-compose.prod.yml up --build
```

### Build and run in detached mode:
```powershell
docker-compose -f docker-compose.prod.yml up -d --build
```

### Stop all services:
```powershell
docker-compose -f docker-compose.prod.yml down
```

## Individual Service Commands

### Build a specific service:
```powershell
docker build -f Dockerfile.users -t whistl3r-users .
```

### Run a specific service:
```powershell
docker run -d -p 5001:8080 `
  -e ConnectionStrings__DefaultConnection="Host=whistl3r-1-instance-1.cno80gy6gzh5.us-east-2.rds.amazonaws.com;Port=5432;Database=whistl3r_data;Username=headofficial;Password=0pt1m0sPr1m3.;SSL Mode=Require;" `
  whistl3r-users
```

## Service Endpoints

With Nginx reverse proxy (port 80):
- Users: http://localhost/api/users
- Games: http://localhost/api/games
- Assignors: http://localhost/api/assignors
- Groups: http://localhost/api/groups
- Organizations: http://localhost/api/organizations
- PayScale: http://localhost/api/payscale
- Reviews: http://localhost/api/reviews

Direct service access:
- Users: http://localhost:5001
- Games: http://localhost:5002
- Assignors: http://localhost:5003
- Groups: http://localhost:5004
- Organizations: http://localhost:5005
- PayScale: http://localhost:5006
- Reviews: http://localhost:5007

## Health Checks

```powershell
# Check all services
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
curl http://localhost:5005/health
curl http://localhost:5006/health
curl http://localhost:5007/health

# Through Nginx
curl http://localhost/health
```

## Logs

### View logs for all services:
```powershell
docker-compose -f docker-compose.prod.yml logs -f
```

### View logs for specific service:
```powershell
docker-compose -f docker-compose.prod.yml logs -f users
```

## Troubleshooting

### Rebuild without cache:
```powershell
docker-compose -f docker-compose.prod.yml build --no-cache
```

### Remove all containers and volumes:
```powershell
docker-compose -f docker-compose.prod.yml down -v
```

### Check running containers:
```powershell
docker ps
```

### Inspect a container:
```powershell
docker inspect whistl3r-users-1
```

### Enter a running container:
```powershell
docker exec -it whistl3r-users-1 /bin/bash
```

## Production Deployment

### Push to Docker Hub:
```powershell
# Tag images
docker tag whistl3r-users:latest yourusername/whistl3r-users:latest

# Push to registry
docker push yourusername/whistl3r-users:latest
```

### Deploy to AWS ECS/EC2:
1. Push images to Amazon ECR
2. Create ECS task definitions
3. Configure load balancer
4. Set up auto-scaling

### Deploy to Azure Container Instances:
```powershell
az container create `
  --resource-group whistl3r-rg `
  --name whistl3r-users `
  --image whistl3r-users:latest `
  --cpu 1 --memory 1.5 `
  --ports 8080 `
  --environment-variables `
    ConnectionStrings__DefaultConnection="Host=..." `
    Jwt__SecretKey="..."
```

## Security Notes

**IMPORTANT**: The current docker-compose.prod.yml contains hardcoded credentials. For production:

1. Use Docker secrets:
```yaml
secrets:
  db_connection:
    external: true
```

2. Use environment files:
```powershell
docker-compose -f docker-compose.prod.yml --env-file .env.prod up
```

3. Create `.env.prod` file (add to .gitignore):
```
DB_HOST=whistl3r-1-instance-1.cno80gy6gzh5.us-east-2.rds.amazonaws.com
DB_NAME=whistl3r_data
DB_USER=headofficial
DB_PASSWORD=0pt1m0sPr1m3.
JWT_SECRET=your-super-secret-key-that-is-at-least-32-characters-long
```

## Network Configuration

All services are on the `whistl3r-net` bridge network, allowing internal communication between containers.

## Monitoring

Add Prometheus + Grafana for monitoring:
```yaml
prometheus:
  image: prom/prometheus
  volumes:
    - ./prometheus.yml:/etc/prometheus/prometheus.yml
  ports:
    - "9090:9090"

grafana:
  image: grafana/grafana
  ports:
    - "3000:3000"
```
