# Fix namespaces in Whistl3rApi consolidated project
$rootPath = "C:\dev\services\Whistl3rApi"

Write-Host "Fixing namespaces in Whistl3rApi project..." -ForegroundColor Cyan

# Define namespace mappings
$namespaceMap = @{
    'UsersService' = 'Whistl3rApi'
    'GamesService' = 'Whistl3rApi'
    'OrganizationsService' = 'Whistl3rApi'
    'AssignorsService' = 'Whistl3rApi'
    'CommunicationService' = 'Whistl3rApi'
    'ReviewsService' = 'Whistl3rApi'
    'GroupsService' = 'Whistl3rApi'
    'PayScaleService' = 'Whistl3rApi'
}

# Get all .cs files
$files = Get-ChildItem -Path $rootPath -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Replace old namespaces with new one
    foreach ($old in $namespaceMap.Keys) {
        $new = $namespaceMap[$old]
        $content = $content -replace "namespace $old\.Controllers", "namespace $new.Controllers"
        $content = $content -replace "namespace $old\.Models", "namespace $new.Models"
        $content = $content -replace "namespace $old\.Services", "namespace $new.Services"
        $content = $content -replace "namespace $old\.Data", "namespace $new.Data"
        
        # Also update using statements
        $content = $content -replace "using $old\.Controllers", "using $new.Controllers"
        $content = $content -replace "using $old\.Models", "using $new.Models"
        $content = $content -replace "using $old\.Services", "using $new.Services"
        $content = $content -replace "using $old\.Data", "using $new.Data"
    }
    
    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  Updated: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "`nNamespace fix complete!" -ForegroundColor Green
