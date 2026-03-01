# Script to consolidate all microservices into Whistl3rApi

$services = @("Assignors", "Communication", "Games", "Groups", "Organizations", "PayScale", "Reviews", "Users")
$targetRoot = "C:\dev\services\Whistl3rApi"

Write-Host "Consolidating microservices into Whistl3rApi..." -ForegroundColor Cyan
Write-Host ""

foreach ($service in $services) {
    $sourcePath = "C:\dev\services\$service"
    
    if (-not (Test-Path $sourcePath)) {
        Write-Host "Warning: $service not found, skipping..." -ForegroundColor Yellow
        continue
    }
    
    Write-Host "Processing $service..." -ForegroundColor White
    
    # Copy Controllers
    if (Test-Path "$sourcePath\Controllers") {
        Get-ChildItem "$sourcePath\Controllers" -Filter *.cs | ForEach-Object {
            Copy-Item $_.FullName "$targetRoot\Controllers" -Force
            Write-Host "  ✓ Controller: $($_.Name)" -ForegroundColor Green
        }
    }
    
    # Copy Models
    if (Test-Path "$sourcePath\Models") {
        Get-ChildItem "$sourcePath\Models" -Filter *.cs | ForEach-Object {
            Copy-Item $_.FullName "$targetRoot\Models" -Force
            Write-Host "  ✓ Model: $($_.Name)" -ForegroundColor Green
        }
    }
    
    # Copy Services
    if (Test-Path "$sourcePath\Services") {
        Get-ChildItem "$sourcePath\Services" -Filter *.cs | ForEach-Object {
            Copy-Item $_.FullName "$targetRoot\Services" -Force
            Write-Host "  ✓ Service: $($_.Name)" -ForegroundColor Green
        }
    }
    
    # Copy Data files (DbContext, etc.)
    if (Test-Path "$sourcePath\Data") {
        Get-ChildItem "$sourcePath\Data" -Filter *.cs | ForEach-Object {
            # Rename to avoid conflicts
            $newName = $_.Name -replace "ApplicationDbContext", "${service}DbContext"
            Copy-Item $_.FullName "$targetRoot\Data\$newName" -Force
            Write-Host "  ✓ Data: $newName" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "Consolidation complete!" -ForegroundColor Cyan
