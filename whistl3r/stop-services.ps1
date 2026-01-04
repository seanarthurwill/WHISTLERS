Write-Host "Stopping all microservices..." -ForegroundColor Red

# Find and stop all dotnet processes running the services
$servicePaths = @(
    "Communication",
    "Users",
    "Assignors",
    "Organizations",
    "ApiGateway"
)

foreach ($service in $servicePaths) {
    $processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -like "*\services\$service\*" -or 
        $_.CommandLine -like "*services\$service*" -or
        $_.CommandLine -like "*services\\$service*"
    }
    
    if ($processes) {
        foreach ($process in $processes) {
            Write-Host "Stopping $service (PID: $($process.Id))..." -ForegroundColor Yellow
            Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

# Alternative method: Kill all dotnet processes that were started from the services directory
Write-Host "Cleaning up any remaining dotnet processes from services directory..." -ForegroundColor Yellow
Get-WmiObject Win32_Process -Filter "name = 'dotnet.exe'" | Where-Object {
    $_.CommandLine -like "*\dev\services\*" -or $_.CommandLine -like "*/dev/services/*"
} | ForEach-Object {
    Write-Host "Stopping process PID: $($_.ProcessId)" -ForegroundColor Yellow
    Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
}

Write-Host "All services stopped!" -ForegroundColor Green
