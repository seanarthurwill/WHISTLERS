Write-Host "Starting all microservices in debug mode..." -ForegroundColor Green
Write-Host "Services will be ready for debugger attachment" -ForegroundColor Yellow

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Communication; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Users; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Assignors; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Organizations; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\Games; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd c:\dev\services\ApiGateway; `$env:ASPNETCORE_ENVIRONMENT='Development'; dotnet run --launch-profile http"

Start-Sleep -Seconds 5

Write-Host ""
Write-Host "All services started in Debug mode!" -ForegroundColor Green
Write-Host ""
Write-Host "To attach debugger to all services:" -ForegroundColor Cyan
Write-Host "  1. Press F5 in VS Code" -ForegroundColor White
Write-Host "  2. Select '.NET Core Attach'" -ForegroundColor White
Write-Host "  3. Attach to each service process you want to debug" -ForegroundColor White
Write-Host "  4. Set breakpoints and debug across services" -ForegroundColor White
Write-Host ""
Write-Host "Service URLs:" -ForegroundColor Cyan
Write-Host "API Gateway: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Communication Service: http://localhost:5007" -ForegroundColor Yellow
Write-Host "Users Service: http://localhost:5001" -ForegroundColor DarkRed
Write-Host "Assignors Service: http://localhost:5002" -ForegroundColor Blue
Write-Host "Organizations Service: http://localhost:5003" -ForegroundColor Magenta
Write-Host "Games Service: http://localhost:5163" -ForegroundColor Green