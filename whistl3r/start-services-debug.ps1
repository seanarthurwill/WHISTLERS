Write-Host "Starting microservices in debug mode..." -ForegroundColor Green
Write-Host "Note: Services will wait for debugger attachment" -ForegroundColor Yellow

# Start services with debug flag
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Communication; $env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Users; $env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Assignors; $env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Organizations; $env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\ApiGateway; $env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run"

Start-Sleep -Seconds 3

Write-Host ""
Write-Host "All services started in Development mode!" -ForegroundColor Green
Write-Host "To debug a service:" -ForegroundColor Cyan
Write-Host "  1. Press F5 in VS Code" -ForegroundColor White
Write-Host "  2. Select '.NET Core Attach'" -ForegroundColor White
Write-Host "  3. Choose the service process to debug" -ForegroundColor White
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "  API Gateway:          http://localhost:5000" -ForegroundColor White
Write-Host "  Communication:        http://localhost:5007" -ForegroundColor Yellow
Write-Host "  Users:                http://localhost:5001" -ForegroundColor DarkRed
Write-Host "  Assignors:            http://localhost:5002" -ForegroundColor Blue
Write-Host "  Organizations:        http://localhost:5003" -ForegroundColor Magenta
