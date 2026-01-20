# Start all services with Docker Compose

Write-Host "Starting Whistl3r services with Docker Compose..." -ForegroundColor Green

# Build and start services
docker-compose up --build -d

Write-Host ""
Write-Host "Services are starting..." -ForegroundColor Yellow
Write-Host "Waiting for health checks..." -ForegroundColor Yellow

Start-Sleep -Seconds 10

Write-Host ""
Write-Host "Services started!" -ForegroundColor Green
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "API Gateway:      http://localhost:5000" -ForegroundColor White
Write-Host "Users Service:    http://localhost:5001" -ForegroundColor White
Write-Host "Games Service:    http://localhost:5004" -ForegroundColor White
Write-Host "Organizations:    http://localhost:5003" -ForegroundColor White
Write-Host "Assignors:        http://localhost:5002" -ForegroundColor White
Write-Host "Communication:    http://localhost:5007" -ForegroundColor White
Write-Host "PostgreSQL:       localhost:5432" -ForegroundColor White
Write-Host ""
Write-Host "View logs:" -ForegroundColor Cyan
Write-Host "  docker-compose logs -f [service-name]" -ForegroundColor White
Write-Host ""
Write-Host "Stop all services:" -ForegroundColor Cyan
Write-Host "  docker-compose down" -ForegroundColor White